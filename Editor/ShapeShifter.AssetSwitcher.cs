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
    
    public partial class ShapeShifter
    {

        private static string ACTIVE_GAME_PLAYER_PREFS_KEY = "ACTIVE_GAME_PLAYER_PREFS_KEY";
        
        private static int highlightedGame;
        // private string lastSwitched;
        
        private bool showSwitcher = true;

        private static string ActiveGameName => configuration.GameNames[ActiveGame];
        
        public static int ActiveGame
        {
            get
            {
                if (!EditorPrefs.HasKey(ACTIVE_GAME_PLAYER_PREFS_KEY))
                {
                    ShapeShifterLogger.Log(
                        "Could not found any active game on EditorPrefs, setting by default game 0"
                    );
                    ActiveGame = 0;
                }
                
                return EditorPrefs.GetInt(ACTIVE_GAME_PLAYER_PREFS_KEY);
            }
            set
            {
                ShapeShifterLogger.Log(
                    $"Setting active game on EditorPrefs: {value}"
                );
                EditorPrefs.SetInt(ACTIVE_GAME_PLAYER_PREFS_KEY, value);
            }
        }

        private static void CopyFromOriginToSkinnedExternal(DirectoryInfo directory) {
            string relativePath = GenerateRelativePathFromKey(directory.Name);
            string origin = Path.Combine(Application.dataPath, relativePath);
            string target = Path.Combine(directory.FullName, Path.GetFileName(origin));
            IOUtils.CopyFile(origin, target);
        }
        
        private static void CopyFromSkinnedExternalToOrigin(DirectoryInfo directory) {
            string relativePath = GenerateRelativePathFromKey(directory.Name);
            string target = Path.Combine(Application.dataPath, relativePath);
            string searchPattern = Path.GetFileName(target);
            FileInfo origin = directory.GetFiles(searchPattern)[0];
            origin.CopyTo(target, true);
        }
        
        private static void CopyFromSkinsToUnity(DirectoryInfo directory) {
            string guid = directory.Name;

            // Ensure it has the same name, so we don't end up copying .DS_Store
            string target = AssetDatabase.GUIDToAssetPath(guid);
            string searchPattern = Path.GetFileName(target)+"*";

            FileInfo[] files = directory.GetFiles(searchPattern);

            if (files.Length > 0) {
                foreach (FileInfo fileInfo in files)
                {
                    //ShapeShifterLogger.Log($"[Shape Shifter] Copying from: {origin.FullName} to {target}");
                    if (fileInfo.Extension == ".meta")
                    {
                        fileInfo.CopyTo(target + ".meta", true);
                    }
                    else
                    {
                        fileInfo.CopyTo(target, true);
                    }
                }
               
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

        private static void CopyFromUnityToSkins(DirectoryInfo skinDirectory) {
            
            if (IOUtils.IsFolderEmpty(skinDirectory))
            {
                //There shouldn't be an empty skin folder, most likely it was removed outside of ShapeShifter. E.g. discarding changes in a git client.
                skinDirectory.Delete();
                return;
            }

            string guid = skinDirectory.Name;
            string origin = AssetDatabase.GUIDToAssetPath(guid);
            string target = Path.Combine(skinDirectory.FullName, Path.GetFileName(origin));

            if (AssetDatabase.IsValidFolder(origin))
            {
                DirectoryInfo originInfo = new DirectoryInfo(origin);
                DirectoryInfo targetInfo = new DirectoryInfo(target);
                IOUtils.CopyFolder(originInfo, targetInfo);
            }
            else
            {
                IOUtils.CopyFile(origin, target);
            }
        }

        private void OnAssetSwitcherGUI() {
            this.showSwitcher = EditorGUILayout.Foldout(this.showSwitcher, "Asset Switcher");

            if (! this.showSwitcher || configuration.GameNames.Count == 0) {
                return;
            }
            
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
            GUIStyle labelStyle = GUI.skin.GetStyle("Label"); 
                
            using (new GUILayout.VerticalScope(boxStyle)) {
                GUIStyle titleStyle = new GUIStyle (labelStyle){
                    alignment = TextAnchor.MiddleCenter
                };

                string currentGame = ActiveGameName;

                GUILayout.Box($"Current game: {currentGame}", titleStyle);

                highlightedGame = GUILayout.SelectionGrid(
                    highlightedGame,
                    configuration.GameNames.ToArray(),
                    2,
                    buttonStyle
                );

                GUILayout.Space(10.0f);

                if (GUILayout.Button("Switch!", buttonStyle))
                {
                    SwitchToGame(highlightedGame);
                }

                if (GUILayout.Button($"Overwrite {configuration.GameNames[highlightedGame]} skin", buttonStyle))
                {
                    if (EditorUtility.DisplayDialog(
                        "ShapeShifter",
                        $"This will overwrite you current {configuration.GameNames[highlightedGame]} assets with the assets currently inside unity. Are you sure?",
                        "Yes, overwrite it.",
                        "Nevermind"
                    ))
                    {
                        OverwriteSelectedSkin(highlightedGame);
                    }
                }
            }
        }
        
        private static void OverwriteSelectedSkin(int selected) {
            SavePendingChanges();
                
            string game = configuration.GameNames[selected];

            if (ActiveGameName != game)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"This will overwrite the {game} skins with the current assets. ");

                stringBuilder.Append($"The last asset switch was to {ActiveGameName}");
                

                stringBuilder.Append(" Are you sure?");

                if (!EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    stringBuilder.ToString(),
                    "Yeah, I'm sure, go ahead.",
                    "Wait, what? No, stop!"
                ))
                {
                    return;
                }
            }
            
            PerformCopiesWithTracking(
                selected: selected,
                description: "Overwrite selected skin",
                CopyFromUnityToSkins,
                CopyFromOriginToSkinnedExternal
            );
        }

        private static void PerformCopiesWithTracking(
            int selected,
            string description,
            Action<DirectoryInfo> internalAssetOperation,
            Action<DirectoryInfo> externalAssetOperation
        ) {
            string game = configuration.GameNames[selected];
            ShapeShifterLogger.Log($"{description}: {game}");

            string gameFolderPath = Path.Combine(skinsFolder.FullName, game);

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

                PerformOperationOnPath(
                    gameFolderPath, 
                    InternalAssetsFolder,
                    internalAssetOperation,
                    description,
                    progressBarStep,
                    ref progress
                );
 
                PerformOperationOnPath(
                    gameFolderPath, 
                    ExternalAssetsFolder,
                    externalAssetOperation,
                    description,
                    progressBarStep,
                    ref progress
                );

                RefreshAllAssets();
            } else {
                EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    $"Could not {description.ToLower()}: {game}. Skins folder does not exist!",
                    "Fine, I'll take a look."
                );

                ActiveGame = 0;
                // TODO: ^this shouldn't just be assigned to 0, the operations should be atomic.
                // If they fail, nothing should change.
            }

            EditorUtility.ClearProgressBar();
        }
        
        private static void PerformOperationOnPath(
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

        private static void RefreshAllAssets() {
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

        private static void SwitchToGame(int selected) {

            if (configuration.ModifiedAssetPaths.Count > 0)
            {
                bool continueSwitch = EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    $"There are unsaved changes in your skinned assets. You should make sure to save them into your Active Game folder",
                    "OK",
                    "Cancel Switch"
                );

                if (!continueSwitch)
                {
                    return;
                }
                
                OverwriteSelectedSkin(ActiveGame);
            }
            
            PerformCopiesWithTracking(
                selected,
                "Switch to game",
                CopyFromSkinsToUnity,
                CopyFromSkinnedExternalToOrigin
            );
            ActiveGame = selected;
            configuration.ModifiedAssetPaths.Clear();
        }
        
        private static void CopyIfMissingInternal(DirectoryInfo directory)
        {
                string guid = directory.Name;

                // Ensure it has the same name, so we don't end up copying .DS_Store
                string target = AssetDatabase.GUIDToAssetPath(guid);
                string searchPattern = Path.GetFileName(target)+"*";

                FileInfo[] files = directory.GetFiles(searchPattern);

                if (files.Length > 0) {
                    foreach (FileInfo fileInfo in files)
                    {
                        //ShapeShifterLogger.Log($"[Shape Shifter] Copying from: {origin.FullName} to {target}");
                        if (fileInfo.Extension == ".meta")
                        {
                            string destFileName = target + ".meta";
                            if (File.Exists(PathUtils.GetFullPath(destFileName)))
                            {
                                ShapeShifterLogger.LogWarning($"{destFileName} already exists, skipping");
                                continue;
                            }
                            
                            fileInfo.CopyTo(destFileName, true);
                        }
                        else
                        {
                            if (File.Exists(PathUtils.GetFullPath(target)))
                            {
                                ShapeShifterLogger.LogWarning($"{target} already exists, skipping");
                                continue;
                            }
                            
                            fileInfo.CopyTo(target, true);
                        }
                    }
               
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
    }
}