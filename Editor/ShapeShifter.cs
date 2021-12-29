using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = System.Object;

namespace Miniclip.ShapeShifter {
    [Serializable]
    public partial class ShapeShifter : EditorWindow {
        private static readonly string ConfigurationResource = "ShapeShifterConfiguration.asset";
        private static readonly string ExternalAssetsFolder = "external";
        private static readonly string InternalAssetsFolder = "internal";
        private static readonly string IsInitializedKey = "isInitialized";
        
        private static ShapeShifterConfiguration configuration;

        public static ShapeShifterConfiguration Configuration
        {
            get
            {
                if (configuration != null)
                {
                    return configuration;
                }
                InitialiseConfiguration();
                return configuration;
            }
            set => configuration = value;
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
        
        private static Editor defaultConfigurationEditor;
        private bool showConfiguration = false;
        
        private static DirectoryInfo skinsFolder;
        public static DirectoryInfo SkinsFolder
        {
            get
            {
                if (skinsFolder == null)
                {
                    InitialiseFolders();
                }
                return skinsFolder;
            }
            set => skinsFolder = value;
        }

        [MenuItem("Window/Shape Shifter/Open ShapeShifter Window", false, (int) 'G')]
        public static void OpenShapeShifter()
        {
            InitializeShapeShifterCore();

            ShowNextToInspector(focus: true);
        }
        
        [MenuItem("Window/Shape Shifter/Print Initialise state", false, (int) 'G')]
        public static void PrintInitialiseState()
        {
            ShapeShifterLogger.Log($"Initialised: {ShapeShifterEditorPrefs.GetBool(IsInitializedKey)}");
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

        private static string GenerateAssetKey(string game, string guid) => game + ":" + guid;

        private void OnEnable()
        {
            if (configuration == null)
            {
                this.Close();
                return;
            }
            this.OnAssetSkinnerEnable();
            this.OnExternalAssetSkinnerEnable();
            
        }
        internal static Dictionary<string, string> missingGuidsToPathDictionary = new Dictionary<string, string>();

        internal static void RestoreMissingAssets()
        {
            InitializeShapeShifterCore();

            List<string> missingAssets = new List<string>();

            string assetFolderPath = Path.Combine(GetGameFolderPath(ActiveGame), InternalAssetsFolder);

            if (Directory.Exists(assetFolderPath))
            {
                DirectoryInfo internalFolder = new DirectoryInfo(assetFolderPath);

                foreach (DirectoryInfo directory in internalFolder.GetDirectories())
                {
                    string guid = directory.Name;

                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (string.IsNullOrEmpty(assetPath))
                    {
                        missingAssets.Add(guid);
                        continue;
                    }

                    if (!File.Exists(PathUtils.GetFullPath(assetPath)))
                    {
                        missingAssets.Add(guid);
                        continue;
                    }
                }

                // get all deleted meta files
                IEnumerable<string> deletedMetas = GitUtils.GetUncommittedDeletedFiles()
                    .Where(deletedMeta => deletedMeta.Contains(".meta"));

                //Restore meta files and do not call AssetDatabase refresh to prevent from being deleted again
                //Store in dictionary mapping guid to path, since AssetDatabase.GUIDToAssetPath will not work
                foreach (var deletedMeta in deletedMetas)
                {
                    ShapeShifterLogger.Log($"Restoring {deletedMeta}");
                    GitUtils.DiscardChanges(PathUtils.GetFullPath(deletedMeta));

                    string metaGUID = ExtractGUIDFromMetaFile(PathUtils.GetFullPath(deletedMeta));

                    string fullpath = PathUtils.GetFullPath(deletedMeta).Replace(".meta", "");

                    missingGuidsToPathDictionary.Add(metaGUID, PathUtils.GetPathRelativeToAssetsFolder(fullpath));
                }

                PerformCopiesWithTracking(
                    ActiveGame,
                    "Add missing skins",
                    CopyIfMissingInternal,
                    CopyFromSkinnedExternalToOrigin //TODO: CopyIfMissingExternal
                );
            }
        }

        private static string ExtractGUIDFromMetaFile(string path)
        {
            if (Path.GetExtension(path) != ".meta")
            {
                ShapeShifterLogger.LogError($"Trying to extract guid from non meta file : {path}");
                return string.Empty;
            }
            
            using (StreamReader reader = new StreamReader(path)) {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    
                    if (!line.StartsWith("guid"))
                        continue;

                    return line.Split(' ')[1];
                }
            }

            return string.Empty;
        }

        internal static void InitializeShapeShifterCore()
        {
            if (ShapeShifterEditorPrefs.GetBool(IsInitializedKey)) //TODO: To be changed for a settings provider?
            {
                return;
            }
            
            InitialiseFolders();
            InitialiseConfiguration();

            ShapeShifterEditorPrefs.SetBool(IsInitializedKey, true);
        }

        private static void InitialiseFolders()
        {
            SkinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            IOUtils.TryCreateDirectory(SkinsFolder.FullName, false);
        }

        private static void InitialiseConfiguration() {
            
            Debug.Log("##! Initialise Configuration");
            
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
                //Update Highlighted Game
                highlightedGame = ActiveGame;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private void OnGUI() {

            if (configuration == null)
            {
                this.Close();
            }
            
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

                if (GUILayout.Button("Check for missing assets"))
                {
                   RestoreMissingAssets();
                }


                GUILayout.FlexibleSpace();
            }
            
            
            this.Repaint();
        }

        private static void SavePendingChanges() {
            AssetDatabase.SaveAssets();
            
            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }

        public static void SaveChanges()
        {
            if (Configuration.ModifiedAssetPaths.Count > 0)
                OverwriteSelectedSkin(ActiveGame);
        }
    }
}
