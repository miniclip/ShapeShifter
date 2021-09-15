using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NelsonRodrigues.GameSwitcher {
    public partial class GameSwitcher : EditorWindow {
        private static readonly string ConfigurationResource = "GameSwitcherConfiguration.asset";
        private static readonly string ExternalAssetsFolder = "external";
        private static readonly string InternalAssetsFolder = "internal";
        private static readonly string SkinnedUserData = "{GameSwitcher:skinned}";
        
        private static readonly Type[] SupportedTypes = {
            typeof(GameObject),
            typeof(ScriptableObject),
            typeof(Texture2D)
        };
        
        private GameSwitcherConfiguration configuration;
        private Editor defaultConfigurationEditor;
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
            this.defaultConfigurationEditor = Editor.CreateEditor(
                this.configuration,
                typeof(GameSwitcherConfigurationEditor)
            );

            this.skinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            this.OnAssetSkinnerEnable();
            this.OnExternalAssetSkinnerEnable();
        }
        
        private void OnGUI() {
            using (new GUILayout.VerticalScope()) {
                this.showConfiguration = EditorGUILayout.Foldout(this.showConfiguration, "Configuration");
                
                if (this.showConfiguration) {
                    this.defaultConfigurationEditor.OnInspectorGUI();
                    
                    // TODO: hide this when it's no longer necessary, as direct access to this list may cause issues
                    this.externalConfigurationEditor.OnInspectorGUI();
                }

                this.OnAssetSwitcherGUI();
                this.OnAssetSkinnerGUI();
                this.OnExternalAssetSkinnerGUI();
                
                GUILayout.FlexibleSpace();
            }
            
            this.Repaint();
        }

        private void SavePendingChanges() {
            AssetDatabase.SaveAssets();
            
            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }
    }
}
