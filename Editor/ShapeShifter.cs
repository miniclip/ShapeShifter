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
        
        private static ShapeShifterConfiguration configuration;
        private static Editor defaultConfigurationEditor;
        private bool showConfiguration = false;
        
        private static DirectoryInfo skinsFolder;


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
            InitializeShapeShifterCore();
            this.OnAssetSkinnerEnable();
            this.OnExternalAssetSkinnerEnable();
        }

        internal static void InitializeShapeShifterCore()
        {
            InitialiseFolders();
            InitialiseConfiguration();
        }

        private static void InitialiseFolders()
        {
            skinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            IOUtils.TryCreateDirectory(skinsFolder.FullName);
        }

        private static void InitialiseConfiguration() {
            if (configuration != null) {
                return;
            }

            configuration = (ShapeShifterConfiguration)EditorGUIUtility.Load(
                ConfigurationResource
            );

            if (configuration == null)
            {
                configuration = CreateInstance<ShapeShifterConfiguration>();

                string folderPath = "Assets/Editor Default Resources/";

                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                AssetDatabase.CreateAsset(
                    configuration,
                    folderPath + ConfigurationResource
                );
                
                EditorUtility.SetDirty(configuration);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            defaultConfigurationEditor = Editor.CreateEditor(
                configuration,
                typeof(ShapeShifterConfigurationEditor)
            );
            
            AssetDatabase.Refresh();

            if (configuration.GameNames.Count == 0)
            {
                ShapeShifterLogger.Log("Shapeshifter has no configured games, creating a default one and making it active");
                configuration.GameNames.Add("Default");
                SwitchToGame(0);
                EditorUtility.SetDirty(configuration);
            }
            else
            {
                //Get Active game stored in Editor Prefs
                //Update Highlighted Game
                highlightedGame = ActiveGame;

                //Check for missing assets in project that exist in skins and copy them to project

                PerformCopiesWithTracking(
                    ActiveGame,
                    "Add missing skins",
                    CopyIfMissingInternal,
                    CopyFromSkinnedExternalToOrigin
                );
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private void OnGUI() {
            using (new GUILayout.VerticalScope()) {
                this.showConfiguration = EditorGUILayout.Foldout(this.showConfiguration, "Configuration");

                if (this.showConfiguration && defaultConfigurationEditor != null)
                {
                    defaultConfigurationEditor.OnInspectorGUI();

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

        private static void SavePendingChanges() {
            AssetDatabase.SaveAssets();
            
            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }
        
    }
}
