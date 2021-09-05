using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace NelsonRodrigues.GameSwitcher {
   
    public partial class GameSwitcher {
        private int selected;
        private bool showSwitcher = true;
        
        private void OnAssetSwitcherGUI() {
            this.showSwitcher = EditorGUILayout.Foldout(this.showSwitcher, "Asset switcher");

            if (! this.showSwitcher) {
                return;
            }
            
            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
            
            using (new GUILayout.VerticalScope(boxStyle)) {
                this.selected = GUILayout.SelectionGrid (
                    this.selected, 
                    this.configuration.GameNames.ToArray(), 
                    2,
                    buttonStyle
                );
                
                GUILayout.Space(10.0f);

                if (GUILayout.Button("Switch!", buttonStyle)) {
                    this.SwitchToGame(this.selected);
                }
            }
        }       
        
        private void SwitchToGame(int selected) {
            string game = this.configuration.GameNames[selected];
            Debug.Log($"[Game Switcher] Switching to game: {game}");

            DirectoryInfo gameFolder = new DirectoryInfo(Path.Combine(this.skinsFolder.FullName, game));

            if (gameFolder.Exists) {
                DirectoryInfo[] directories = gameFolder.GetDirectories();

                float progress = 0.0f;
                float progressBarStep = 1.0f / directories.Length;

                foreach (DirectoryInfo directory in directories) {
                    string guid = directory.Name;
                    
                    // Ensure it has the same name, so we don't end up copying .DS_Store
                    string target = AssetDatabase.GUIDToAssetPath(guid);
                    string searchPattern = Path.GetFileName(target);
                    FileInfo file = directory.GetFiles(searchPattern)[0];
                   
                    Debug.Log($"[Game Switcher] Copying from: {file.FullName} to {target}");
                    file.CopyTo(target, true);

                    progress += progressBarStep;
                    EditorUtility.DisplayProgressBar("Game Switcher", "Switching games...", progress);
                }
 
                AssetDatabase.Refresh();
            } else {
                EditorUtility.DisplayDialog(
                    "Game Switcher",
                    $"Could not switch to game: {game}. Skins folder does not exist!",
                    "Fine, I'll take a look."
                );

                this.selected = 0;
                // TODO: ^this shouldn't just be assigned to 0, the switch game operation should be atomic.
                // If it fails, nothing should change.
            }

            EditorUtility.ClearProgressBar();
        }        
    }
}