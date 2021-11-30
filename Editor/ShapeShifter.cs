using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MUShapeShifter {
    public partial class ShapeShifter : EditorWindow {
        private static readonly string ConfigurationResource = "ShapeShifterConfiguration.asset";
        private static readonly string ExternalAssetsFolder = "external";
        private static readonly string InternalAssetsFolder = "internal";
        // private static readonly string SkinnedUserData = "{ShapeShifter:skinned}";
        
        private static readonly Type[] SupportedTypes = {
            typeof(AnimationClip),
            typeof(AnimatorController),
            typeof(DefaultAsset),
            typeof(GameObject),
            typeof(MonoScript),
            typeof(SceneAsset),
            typeof(ScriptableObject),
            typeof(Texture2D)
        };
        
        private ShapeShifterConfiguration configuration;
        private Editor defaultConfigurationEditor;
        private bool showConfiguration = false;
        
        private DirectoryInfo skinsFolder;

        [MenuItem("Window/Shape Shifter", false, (int)'G')]
        public static void ShowNextToInspector() {
            Assembly editorAssembly = typeof(Editor).Assembly;
            Type inspectorWindowType = editorAssembly.GetType("UnityEditor.InspectorWindow");
            
            EditorWindow.GetWindow<ShapeShifter>(
                "Shape Shifter", 
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

            this.configuration = (ShapeShifterConfiguration)EditorGUIUtility.Load(
                ShapeShifter.ConfigurationResource
            );

            if (this.configuration == null) {
                this.configuration = ScriptableObject.CreateInstance<ShapeShifterConfiguration>();
                
                AssetDatabase.CreateAsset(
                    this.configuration,
                    "Assets/Editor Default Resources/" + ShapeShifter.ConfigurationResource
                );
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void OnEnable() {
            this.InitialiseConfiguration();
            this.defaultConfigurationEditor = Editor.CreateEditor(
                this.configuration,
                typeof(ShapeShifterConfigurationEditor)
            );

            this.skinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            Directory.CreateDirectory(this.skinsFolder.FullName);
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
                if(GUILayout.Button("Skin 20 sprites test"))
                {
                    this.SavePendingChanges();

                    for (int index = 0; index < 20; index++)
                    {
                        Sprite sprite = configuration.Sprites[index];
                        string path = AssetDatabase.GetAssetPath(sprite);
                        SkinAsset(path, false);
                    }
                }
                if(GUILayout.Button("Skin All sprites test"))
                {
                    this.SavePendingChanges();

                    for (int index = 0; index < configuration.Sprites.Count; index++)
                    {
                        Sprite sprite = configuration.Sprites[index];
                        string path = AssetDatabase.GetAssetPath(sprite);
                        SkinAsset(path, false);
                    }
                }
                
                GUILayout.FlexibleSpace();
            }
            
            this.Repaint();
        }

        private void SavePendingChanges() {
            Debug.Log("Save Pending Changes");
            AssetDatabase.SaveAssets();
            
            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }
        
    }
}
