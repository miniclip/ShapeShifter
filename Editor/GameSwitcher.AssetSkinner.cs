using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace NelsonRodrigues.GameSwitcher {
   
    public partial class GameSwitcher {
        private Vector2 scrollPosition;
        private bool showSkinner = true;
        private FileSystemWatcher watcher;

        private HashSet<string> dirtyAssets = new HashSet<string>();
        private Dictionary<string, Texture2D> previewPerAsset = new Dictionary<string, Texture2D>();

        private void DrawAssetSection(Texture2D texture) {
            EditorGUILayout.InspectorTitlebar(true, texture);
            
            string path = AssetDatabase.GetAssetPath(texture);
            AssetImporter importer = AssetImporter.GetAtPath(path);
            bool skinned = importer.userData.Contains(GameSwitcher.SkinnedUserData);

            Color oldColor = GUI.backgroundColor;
            
            if (skinned) {
                this.DrawSkinnedAssetSection(importer);
            } else {
                this.DrawUnskinnedAssetSection(importer);
            }
            
            GUI.backgroundColor = oldColor;
        }

        private void DrawAssetPreview(string key, string game, string path) {
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            
            using (new GUILayout.VerticalScope(boxStyle)) {
                float buttonWidth = EditorGUIUtility.currentViewWidth * (1.0f / this.configuration.GameNames.Count);
                buttonWidth -= 20; // to account for a possible scrollbar or some extra padding 

                bool clicked = GUILayout.Button(
                    this.previewPerAsset[key],
                    GUILayout.Width(buttonWidth),
                    GUILayout.MaxHeight(buttonWidth)
                );
                
                if (clicked) {
                    EditorUtility.RevealInFinder(path);
                }

                using (new GUILayout.HorizontalScope(boxStyle)) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(game);
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawSkinnedAssetSection(AssetImporter importer) {
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            
            using (new GUILayout.HorizontalScope(boxStyle)) {
                foreach (string game in this.configuration.GameNames) {
                    string guid = AssetDatabase.AssetPathToGUID(importer.assetPath);
                    string assetPath = Path.Combine(
                        this.skinsFolder.FullName,
                        game,
                        guid,
                        Path.GetFileName(importer.assetPath)
                    );

                    string key = this.GenerateAssetKey(game, guid);
                    this.GenerateAssetPreview(key, assetPath);
                    this.DrawAssetPreview(key, game, assetPath);
                }
            }

            GUI.backgroundColor = Color.red;
                
            if (GUILayout.Button("Remove skins")) {
                this.RemoveSkins(importer);
            }        
        }

        private void DrawUnskinnedAssetSection(AssetImporter importer) {
            GUI.backgroundColor = Color.green;
                
            if (GUILayout.Button("Skin it!")) {
                this.SkinAsset(importer);
            }
        }

        private void GenerateAssetPreview(string key, string assetPath) {
            if (this.dirtyAssets.Contains(key) || ! this.previewPerAsset.ContainsKey(key)) {
                this.dirtyAssets.Remove(key);

                Texture2D texturePreview = new Texture2D(0, 0);
                texturePreview.LoadImage(File.ReadAllBytes(assetPath));
                this.previewPerAsset[key] = texturePreview;
            }
        }
        
        private void OnAssetSkinnerEnable() {
            if (this.watcher == null) {
                this.watcher = new FileSystemWatcher();
                this.watcher.Path = this.skinsFolder.FullName;
                this.watcher.IncludeSubdirectories = true;
            }
            
            // To account for Unity's behaviour of sending consecutive OnEnables without an OnDisable
            this.watcher.Changed -= this.OnFileSystemChanged;
            this.watcher.Changed += this.OnFileSystemChanged;
            this.watcher.EnableRaisingEvents = true;
        }
        
        private void OnAssetSkinnerGUI() {
            this.showSkinner = EditorGUILayout.Foldout(this.showSkinner, "Asset skinner");

            if (! this.showSkinner) {
                return;
            }

            GUIStyle boxStyle = GUI.skin.GetStyle("Box");

            using (new GUILayout.VerticalScope(boxStyle)) {
                GUILayout.Label ("Selected Textures:", EditorStyles.boldLabel);
                    
                Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
                    
                if (textures.Length == 0) {
                    GUILayout.Label("None.");
                } else {
                    this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);
                        
                    foreach (Texture2D texture in textures) {
                        this.DrawAssetSection(texture);
                    }
                
                    GUILayout.EndScrollView();
                }
            }
        }
        
        private void OnDisable() {
            this.watcher.EnableRaisingEvents = false;
            this.watcher.Changed -= this.OnFileSystemChanged;
            
            this.dirtyAssets.Clear();
            this.previewPerAsset.Clear();
        }

        private void OnFileSystemChanged(object sender, FileSystemEventArgs args) {
            DirectoryInfo guidDirectory = new DirectoryInfo(Path.GetDirectoryName(args.FullPath));
            string key = this.GenerateAssetKey(guidDirectory.Parent.Name , guidDirectory.Name);
            this.dirtyAssets.Add(key);
        }

        private void OnSelectionChange() {
            this.dirtyAssets.Clear();
            this.previewPerAsset.Clear();
        }

        private void RemoveSkins(AssetImporter importer) {
            foreach (string game in this.configuration.GameNames) {
                string guid = AssetDatabase.AssetPathToGUID(importer.assetPath);
                string key = this.GenerateAssetKey(game, guid);
                this.dirtyAssets.Remove(key);
                this.previewPerAsset.Remove(key);
                
                string assetFolder = Path.Combine(this.skinsFolder.FullName, game, guid);
                Directory.Delete(assetFolder, true);                
            }

            importer.userData = importer.userData.Replace(
                GameSwitcher.SkinnedUserData,
                string.Empty
            );
            importer.SaveAndReimport();
        }

        private void SkinAsset(AssetImporter importer) {
            foreach (string game in this.configuration.GameNames) {
                string guid = AssetDatabase.AssetPathToGUID(importer.assetPath);
                string assetFolder = Path.Combine(this.skinsFolder.FullName, game, guid);

                if (!Directory.Exists(assetFolder)) {
                    Directory.CreateDirectory(assetFolder);
                }
                        
                File.Copy(
                    importer.assetPath,
                    assetFolder + "/" + Path.GetFileName(importer.assetPath)
                );
            }
                    
            importer.userData += GameSwitcher.SkinnedUserData;
            importer.SaveAndReimport();
        }
    }
}