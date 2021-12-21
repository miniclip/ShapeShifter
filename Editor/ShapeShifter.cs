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


        [MenuItem("Window/Shape Shifter", false, (int) 'G')]
        public static void OpenShapeShifter()
        {
            ShowNextToInspector(focus: true);
        }
        internal static ShapeShifter ShowNextToInspector(bool focus = false) {
            Assembly editorAssembly = typeof(Editor).Assembly;
            Type inspectorWindowType = editorAssembly.GetType("UnityEditor.InspectorWindow");
            
            return GetWindow<ShapeShifter>(
                "Shape Shifter", 
                focus, 
                inspectorWindowType
            );
        }

        private string GenerateAssetKey(string game, string guid) => game + ":" + guid;

        private void OnEnable()
        {
            this.InitialiseFolders();
            this.InitialiseConfiguration();
            this.OnAssetSkinnerEnable();
            this.OnExternalAssetSkinnerEnable();
        }

        private void InitialiseFolders()
        {
            this.skinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            IOUtils.TryCreateDirectory(skinsFolder.FullName);
            
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
                
                EditorUtility.SetDirty(configuration);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            this.defaultConfigurationEditor = Editor.CreateEditor(
                this.configuration,
                typeof(ShapeShifterConfigurationEditor)
            );
            
            if (this.configuration.GameNames.Count == 0)
            {
                ShapeShifterLogger.Log("Shapeshifter has no configured games, creating a default one and making it active");
                this.configuration.GameNames.Add("Default");
                SwitchToGame(0);
                EditorUtility.SetDirty(configuration);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                //Get Active game stored in Editor Prefs
                //Update Highlighted Game
                highlightedGame = ActiveGame;

                //Check for missing assets in project that exist in skins and copy them to project
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
