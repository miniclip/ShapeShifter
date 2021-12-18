using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = System.Object;

namespace Miniclip.ShapeShifter {
    public partial class ShapeShifter : EditorWindow {
        private static readonly string ConfigurationResource = "ShapeShifterConfiguration.asset";
        private static readonly string ExternalAssetsFolder = "external";
        private static readonly string InternalAssetsFolder = "internal";
        
        private static ShapeShifter instance;
        public static ShapeShifter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = ShowNextToInspector();
                }
                return instance;
            }
        }

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
        public static ShapeShifter ShowNextToInspector() {
            Assembly editorAssembly = typeof(Editor).Assembly;
            Type inspectorWindowType = editorAssembly.GetType("UnityEditor.InspectorWindow");
            
            return GetWindow<ShapeShifter>(
                "Shape Shifter", 
                true, 
                inspectorWindowType
            );
        }

        private string GenerateAssetKey(string game, string guid) => game + ":" + guid;

        private void OnEnable() {
            this.InitialiseConfiguration();
            this.defaultConfigurationEditor = Editor.CreateEditor(
                this.configuration,
                typeof(ShapeShifterConfigurationEditor)
            );
            
            this.skinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            IOUtils.TryCreateDirectory(skinsFolder.FullName);
            
            this.OnAssetSkinnerEnable();
            this.OnExternalAssetSkinnerEnable();
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

                if (GUILayout.Button("Git Test"))
                {
                    string folder = Directory.GetParent(Application.dataPath).Name;
                    Debug.Log(folder);
                    UnityEngine.Object[] objects = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
                    if (objects.Length > 0)
                    {
                        var path = AssetDatabase.GetAssetPath(objects[0]);
                        Debug.Log(path);
                        Debug.Log(Path.Combine(folder, path));
                    }
                }
                
                if (GUILayout.Button("Git Untrack File"))
                {
                    UnityEngine.Object[] objects = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
                    if (objects.Length > 0)
                    {
                        var path = AssetDatabase.GetAssetPath(objects[0]);
                        GitUtils.Untrack(path, true);
                    }
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
