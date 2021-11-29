using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace MUShapeShifter {

    public partial class ShapeShifter {

        private Editor externalConfigurationEditor;
        private int selectedExternalAsset;
        private bool showExternalSkinner = true;

        private string DetermineRecommendedPath() {
            Process process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "git",
                    Arguments = "rev-parse --git-dir",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string repositoryRoot = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrEmpty(repositoryRoot) || ! repositoryRoot.Contains(".git")) {
                Debug.LogWarning("[ShapeShifter] Git repository not found. Is there supposed to be one?");

                //start a bit outside of the project, but not too far away, we're trying to stay within
                //the boundaries of assets related to this project, but without a repository this is
                //just guesswork
                return Application.dataPath + "/../../../";
            } 
              
            //string.Replace won't work here as sometimes there's line breaks at the end of the standard output stream 
            return repositoryRoot.Remove(repositoryRoot.IndexOf(".git", StringComparison.Ordinal));
        }
        
        private void DrawSkinnedExternalAssetSection(string relativePath) {
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            
            using (new GUILayout.HorizontalScope(boxStyle)) {
                foreach (string game in this.configuration.GameNames) {
                    string key = this.GenerateKeyFromRelativePath(relativePath);
                    string assetPath = Path.Combine(
                        this.skinsFolder.FullName,
                        game,
                        ShapeShifter.ExternalAssetsFolder,
                        key,
                        Path.GetFileName(relativePath)
                    );

                    this.GenerateAssetPreview(key, assetPath);
                    this.DrawAssetPreview(key, game, assetPath);
                }
            }

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;
                
            if (GUILayout.Button("Remove skins")) {
                this.RemoveExternalSkins(relativePath);
            }

            GUI.backgroundColor = oldColor;
        }

        private string GenerateKeyFromRelativePath(string relativePath) {
            return WebUtility.UrlEncode(relativePath).Replace(".", "{dot}");
        }

        private string GenerateRelativePathFromKey(string key) {
            return WebUtility.UrlDecode(key).Replace("{dot}", ".");
        }

        // since Path.GetRelativePath doesn't seem to be available
        private string GetRelativePath(string absolutePath, string relativeTo) {
            if (! relativeTo.EndsWith("/")) {
                relativeTo += "/";  
            }

            Uri assetPathIdentifier = new Uri(absolutePath);
            Uri relativeToPathIdentifier = new Uri(relativeTo);
            return relativeToPathIdentifier.MakeRelativeUri(assetPathIdentifier).ToString();
        }
        
        private void OnExternalAssetSkinnerEnable() {
            this.externalConfigurationEditor = Editor.CreateEditor(
                this.configuration,
                typeof(ShapeShifterExternalConfigurationEditor)
            );
        }
        
        private void OnExternalAssetSkinnerGUI() {
            this.showExternalSkinner = EditorGUILayout.Foldout(
                this.showExternalSkinner,
                "External Asset Skinner"
            );

            if (! this.showExternalSkinner) {
                return;
            }

            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            GUIStyle buttonStyle = GUI.skin.GetStyle("Button");

            using (new GUILayout.VerticalScope(boxStyle)) {
                int count = this.configuration.SkinnedExternalAssetPaths.Count;
                
                if (count > 0) {
                    this.selectedExternalAsset = GUILayout.SelectionGrid(
                        this.selectedExternalAsset,
                        this.configuration.SkinnedExternalAssetPaths.ToArray(),
                        2,
                        buttonStyle
                    );

                    if (this.selectedExternalAsset >= 0 && this.selectedExternalAsset < count) {
                        string relativePath = this.configuration.SkinnedExternalAssetPaths[this.selectedExternalAsset];
                        this.DrawSkinnedExternalAssetSection(relativePath);
                    }
                }

                if (GUILayout.Button("Skin external file")) {
                    this.SkinExternalFile();
                }
            }
        }

        private string PickFile(string recommendedPath) {
            string assetPath = EditorUtility.OpenFilePanel(
                "Pick a file, any file!",
                recommendedPath,
                string.Empty
            );

            if (string.IsNullOrEmpty(assetPath)) {
                return null;
            }
            
            if (! assetPath.Contains(recommendedPath) && ! EditorUtility.DisplayDialog (
                "Shape Shifter",
                $"The chosen asset is outside of the recommended path ({recommendedPath}). Are you sure?",
                "Yeah, go for it!",
                "Hmm... not sure, let me check!"
            )) {
                return null;
            }

            return assetPath;
        }

        private void RemoveExternalSkins(string relativePath) {
            string key = this.GenerateKeyFromRelativePath(relativePath);
            
            foreach (string game in this.configuration.GameNames) {
                this.dirtyAssets.Remove(key);
                this.previewPerAsset.Remove(key);
            
                string assetFolder = Path.Combine(
                    this.skinsFolder.FullName,
                    game,
                    ShapeShifter.ExternalAssetsFolder,
                    key
                );
            
                Directory.Delete(assetFolder, true);                
            }

            this.configuration.SkinnedExternalAssetPaths.Remove(relativePath);
        }

        private void SkinExternalFile() {
            string recommendedPath = this.DetermineRecommendedPath();
            string absoluteAssetPath = this.PickFile(recommendedPath);

            this.SkinExternalFile(absoluteAssetPath);
        }

        private void SkinExternalFile(string absoluteAssetPath, Dictionary<string, string> overridesPerGame = null) {
            if (absoluteAssetPath == null) {
                return;
            }

            string relativeAssetPath = this.GetRelativePath(absoluteAssetPath, Application.dataPath);
            
            if (this.configuration.SkinnedExternalAssetPaths.Contains(relativeAssetPath)) {
                EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    $"Could not skin: {relativeAssetPath}. It was already skinned.",
                    "Oops!"
                );
                
                return;
            }
            
            this.configuration.SkinnedExternalAssetPaths.Add(relativeAssetPath);
            
            // even though it's an "external" file, it still might be a Unity file (ex: ProjectSettings), so it's
            // still important to make sure any pending changes are saved before generating copies
            this.SavePendingChanges();

            string origin = absoluteAssetPath;
            string key = this.GenerateKeyFromRelativePath(relativeAssetPath);
            
            foreach (string game in this.configuration.GameNames) {
                string assetFolder = Path.Combine(
                    this.skinsFolder.FullName,
                    game, 
                    ShapeShifter.ExternalAssetsFolder,
                    key
                );
                
                if (!Directory.Exists(assetFolder)) {
                    Directory.CreateDirectory(assetFolder);
                }

                string target = Path.Combine(assetFolder, Path.GetFileName(origin));

                if (overridesPerGame != null && overridesPerGame.ContainsKey(game)) {
                    origin = overridesPerGame[game];
                }
                
                File.Copy(origin, target);
            }
        }
    }
}