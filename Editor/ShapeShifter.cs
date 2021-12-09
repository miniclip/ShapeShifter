using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Miniclip.MUShapeShifter {
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
            
            GetWindow<ShapeShifter>(
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
                ConfigurationResource
            );

            if (this.configuration == null)
            {
                this.configuration = CreateInstance<ShapeShifterConfiguration>();

                string folderPath = "Assets/Editor Default Resources/";

                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                AssetDatabase.CreateAsset(
                    this.configuration,
                    folderPath + ConfigurationResource
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
                
                GUILayout.Label("Test Buttons");
                
                if(GUILayout.Button("Skin Sync"))
                {
                    SkinAssets(configuration.Sprites.ToArray());
                }

                if(GUILayout.Button("Skin Coroutine"))
                {
                    SkinAssets(configuration.Sprites.ToArray(), useAsync:true);
                }

                if (GUILayout.Button("Print skinnable assets"))
                {
                    Debug.Log(
                        $"Total: {configuration.Sprites.Count}. "
                        + $"Eligible to skin: {GetEligibleAssetPaths(configuration.Sprites.ToArray()).Count()}"
                    );
                }
                
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
