using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Miniclip.ShapeShifter {
    
    public partial class ShapeShifter {

        private int selectedGame;
        private string lastSwitched;
        private bool showSwitcher = true;
        
        private void CopyFromOriginToSkinnedExternal(DirectoryInfo directory) {
            string relativePath = this.GenerateRelativePathFromKey(directory.Name);
            string origin = Path.Combine(Application.dataPath, relativePath);
            string target = Path.Combine(directory.FullName, Path.GetFileName(origin));
            IOUtils.CopyFile(origin, target);
        }
        
        private void CopyFromSkinnedExternalToOrigin(DirectoryInfo directory) {
            string relativePath = this.GenerateRelativePathFromKey(directory.Name);
            string target = Path.Combine(Application.dataPath, relativePath);
            string searchPattern = Path.GetFileName(target);
            FileInfo origin = directory.GetFiles(searchPattern)[0];
            origin.CopyTo(target, true);
        }
        
        private void CopyFromSkinsToUnity(DirectoryInfo directory) {
            string guid = directory.Name;

            // Ensure it has the same name, so we don't end up copying .DS_Store
            string target = AssetDatabase.GUIDToAssetPath(guid);
            string searchPattern = Path.GetFileName(target);

            FileInfo[] files = directory.GetFiles(searchPattern);

            if (files.Length > 0) {
                FileInfo origin = files[0];
                //Debug.Log($"[Shape Shifter] Copying from: {origin.FullName} to {target}");
                origin.CopyTo(target, true);
                return;
            }

            DirectoryInfo[] directories = directory.GetDirectories();

            if (directories.Length > 0) {
                target = Path.Combine(
                    Application.dataPath.Replace("/Assets", string.Empty), 
                    target
                );
                
                IOUtils.CopyFolder(directories[0], new DirectoryInfo(target));
            }
        }

        private void CopyFromUnityToSkins(DirectoryInfo directory) {
            string guid = directory.Name;
            string origin = AssetDatabase.GUIDToAssetPath(guid);
            string target = Path.Combine(directory.FullName, Path.GetFileName(origin));
            
            //Debug.Log($"[Shape Shifter] Copying from: {origin} to {target}");
            IOUtils.CopyFile(origin, target);
        }

        private void OnAssetSwitcherGUI() {
            this.showSwitcher = EditorGUILayout.Foldout(this.showSwitcher, "Asset Switcher");

            if (! this.showSwitcher) {
                return;
            }
            
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
            GUIStyle labelStyle = GUI.skin.GetStyle("Label"); 
                
            using (new GUILayout.VerticalScope(boxStyle)) {
                GUIStyle titleStyle = new GUIStyle (labelStyle){
                    alignment = TextAnchor.MiddleCenter
                };
                
                string currentGame = string.IsNullOrEmpty(this.lastSwitched) ? "Unknown" : this.lastSwitched;
                
                GUILayout.Box($"Current game: {currentGame}", titleStyle);

                this.selectedGame = GUILayout.SelectionGrid (
                    this.selectedGame, 
                    this.configuration.GameNames.ToArray(), 
                    2,
                    buttonStyle
                );
                
                GUILayout.Space(10.0f);

                if (GUILayout.Button("Switch!", buttonStyle)) {
                    this.SwitchToGame(this.selectedGame);
                }
                
                if (GUILayout.Button("Overwrite selected skin", buttonStyle)) {
                    this.OverwriteSelectedSkin(this.selectedGame);
                }
            }
        }
        
        private void OverwriteSelectedSkin(int selected) {
            this.SavePendingChanges();
                
            string game = this.configuration.GameNames[selected];
            
            if (this.lastSwitched != game) {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"This will overwrite the {game} skins with the current assets. ");
                
                if (string.IsNullOrEmpty(this.lastSwitched)) {
                    stringBuilder.Append("You haven't switched games this session.");
                } else {
                    stringBuilder.Append($"The last asset switch was to {this.lastSwitched}");
                }

                stringBuilder.Append(" Are you sure?");
                
                if (!EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    stringBuilder.ToString(),
                    "Yeah, I'm sure, go ahead.",
                    "Wait, what? No, stop!"
                )) {
                    return;
                }
            }
            
            this.PerformCopiesWithTracking(
                selected,
                "Overwrite selected skin",
                this.CopyFromUnityToSkins,
                this.CopyFromOriginToSkinnedExternal
            );
        }

        private void PerformCopiesWithTracking(
            int selected,
            string description,
            Action<DirectoryInfo> internalAssetOperation,
            Action<DirectoryInfo> externalAssetOperation
        ) {
            string game = this.configuration.GameNames[selected];
            Debug.Log($"[Shape Shifter] {description}: {game}");

            string gameFolderPath = Path.Combine(this.skinsFolder.FullName, game);

            if (Directory.Exists(gameFolderPath)) {
                // this will fail the total by 0-3, as it counts the game, the internal and the external directories
                // but that doesn't make enough of a difference to justify making this a more complex calculation
                int totalDirectories = Directory.EnumerateDirectories(
                    gameFolderPath,
                    "*",
                    SearchOption.AllDirectories
                ).Count();
                
                float progress = 0.0f;
                float progressBarStep = 1.0f / totalDirectories;

                this.PerformOperationOnPath(
                    gameFolderPath, 
                    InternalAssetsFolder,
                    internalAssetOperation,
                    description,
                    progressBarStep,
                    ref progress
                );
 
                this.PerformOperationOnPath(
                    gameFolderPath, 
                    ExternalAssetsFolder,
                    externalAssetOperation,
                    description,
                    progressBarStep,
                    ref progress
                );

                this.RefreshAllAssets();
            } else {
                EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    $"Could not {description.ToLower()}: {game}. Skins folder does not exist!",
                    "Fine, I'll take a look."
                );

                this.selectedGame = 0;
                // TODO: ^this shouldn't just be assigned to 0, the operations should be atomic.
                // If they fail, nothing should change.
            }

            EditorUtility.ClearProgressBar();
        }
        
        private void PerformOperationOnPath(
            string gameFolderPath,
            string assetFolder,
            Action<DirectoryInfo> operation,
            string description,
            float progressBarStep,
            ref float progress
        ) {
            string assetFolderPath = Path.Combine(gameFolderPath, assetFolder);

            if (Directory.Exists(assetFolderPath)) {
                DirectoryInfo internalFolder = new DirectoryInfo(assetFolderPath);

                foreach (DirectoryInfo directory in internalFolder.GetDirectories()) {
                    operation(directory);

                    progress += progressBarStep;
                    EditorUtility.DisplayProgressBar("Shape Shifter", $"{description}...", progress);
                }
            }
        }

        private void RefreshAllAssets() {
            // Force Unity to lose and regain focus, so it resolves any new changes on the packages
            // TODO: Replace this in Unity 2020 with PackageManager.Client.Resolve
            Process process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "osascript",
                    Arguments = "-e 'tell application \"Finder\" to activate' -e 'delay 0.5' -e 'tell application \"Unity\" to activate'",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            
            AssetDatabase.Refresh();
        }

        private void SwitchToGame(int selected) {

            if (configuration.ModifiedAssetPaths.Count > 0)
            {
                bool abortSwitch = EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    $"There are unsaved changes in your skinned assets. You should make sure to save them into your Active Game folder",
                    "OK",
                    "Cancel"
                );
                
                Debug.Log(abortSwitch.ToString());
            }
            
            this.lastSwitched = this.configuration.GameNames[selected];
            this.PerformCopiesWithTracking(
                selected,
                "Switch to game",
                this.CopyFromSkinsToUnity,
                this.CopyFromSkinnedExternalToOrigin
            );
        }
    }
}