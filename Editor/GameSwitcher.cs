using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NelsonRodrigues.GameSwitcher {
    public partial class GameSwitcher : EditorWindow {
        private static readonly string ConfigurationResource = "GameSwitcherConfiguration.asset";
        private static readonly string SkinnedUserData = "{GameSwitcher:skinned}";
        
        private GameSwitcherConfiguration configuration;
        private Editor configurationEditor;
        private bool showConfiguration = false;
        
        private DirectoryInfo skinsFolder;
        
        [MenuItem("Window/Game Switcher", false, (int)'G')]
        public static void ShowNextToInspector() {
            Assembly editorAssembly = typeof(Editor).Assembly;
            Type inspectorWindowType = editorAssembly.GetType("UnityEditor.InspectorWindow");
            
            GameSwitcher switcher = EditorWindow.GetWindow<GameSwitcher>(
                "Game Switcher", 
                true, 
                inspectorWindowType
            );
        }

        private string GenerateAssetKey(string game, string guid) {
            return game + ":" + guid;
        }

        private void InitialiseConfiguration() {
            if (this.configuration != null) {
                return;
            }

            this.configuration = (GameSwitcherConfiguration)EditorGUIUtility.Load(
                GameSwitcher.ConfigurationResource
            );

            if (this.configuration == null) {
                this.configuration = ScriptableObject.CreateInstance<GameSwitcherConfiguration>();
                
                AssetDatabase.CreateAsset(
                    this.configuration,
                    "Assets/Editor Default Resources/" + GameSwitcher.ConfigurationResource
                );
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void OnEnable() {
            this.InitialiseConfiguration();
            this.configurationEditor = Editor.CreateEditor(this.configuration);

            this.skinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            this.OnAssetSkinnerEnable();
        }
        
        private void OnGUI() {
            using (new GUILayout.VerticalScope()) {
                this.showConfiguration = EditorGUILayout.Foldout(this.showConfiguration, "Configuration");
                
                if (this.showConfiguration) {
                    this.configurationEditor.OnInspectorGUI();
                }

                this.OnAssetSkinnerGUI();
            }
                        
            this.Repaint();
        }
    }
}
