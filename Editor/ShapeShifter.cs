using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Miniclip.ShapeShifter {
    [Serializable]
    public partial class ShapeShifter : EditorWindow {

        private static readonly string ConfigurationResourceFolderPath = "Assets/Editor Default Resources/";
        private static readonly string ConfigurationResource = "ShapeShifterConfiguration.asset";
        internal static readonly string ExternalAssetsFolder = "external";
        internal static readonly string InternalAssetsFolder = "internal";
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
            if (Configuration == null)
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
            missingGuidsToPathDictionary.Clear();

            List<string> missingAssets = new List<string>();
            
            if (ActiveGameSkin.HasInternalSkins())
            {
                var internalGUIDs = ActiveGameSkin.GetGUIDs(SkinType.Internal);

                foreach (string internalGUID in internalGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(internalGUID);

                    if (string.IsNullOrEmpty(assetPath))
                    {
                        missingAssets.Add(internalGUID);
                        continue;
                    }

                    if (!File.Exists(PathUtils.GetFullPath(assetPath)))
                    {
                        missingAssets.Add(internalGUID);
                        continue;
                    }
                }

                // get all deleted meta files
                IEnumerable<GitUtils.ChangedFileGitInfo> deletedMetas = GitUtils.GetDeletedFiles()
                    .Where(deletedMeta => deletedMeta.path.Contains(".meta"));

                //Restore meta files and do not call AssetDatabase refresh to prevent from being deleted again
                //Store in dictionary mapping guid to path, since AssetDatabase.GUIDToAssetPath will not work
                foreach (var deletedMeta in deletedMetas)
                {
                    ShapeShifterLogger.Log($"Restoring {deletedMeta.path}");
                    GitUtils.DiscardChanges(PathUtils.GetFullPath(deletedMeta.path));

                    string metaGUID = ExtractGUIDFromMetaFile(PathUtils.GetFullPath(deletedMeta.path));

                    string fullpath = PathUtils.GetFullPath(deletedMeta.path).Replace(".meta", "");

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

        internal static bool IsInitialized
        {
            get => ShapeShifterEditorPrefs.GetBool(IsInitializedKey);
            private set => ShapeShifterEditorPrefs.SetBool(IsInitializedKey, value);
        }
        
        internal static void InitializeShapeShifterCore()
        {
            if (IsInitialized) //TODO: To be changed for a settings provider?
            {
                return;
            }
            
            ShapeShifterLogger.Log("Setting up");

            InitialiseFolders();
            InitialiseConfiguration();
            RestoreMissingAssets();

            
            IsInitialized = true;
        }

        private static void InitialiseFolders()
        {
            SkinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
            IOUtils.TryCreateDirectory(SkinsFolder.FullName, false);
        }

        private static void InitialiseConfiguration() {
            
            if (configuration != null) {
                return;
            }

            configuration = (ShapeShifterConfiguration)EditorGUIUtility.Load(
                ConfigurationResource
            );

            string configurationPath = Path.Combine(
                ConfigurationResourceFolderPath,
                ConfigurationResource
            );
            
            if (configuration == null && File.Exists(configurationPath))
            {
                configuration = AssetDatabase.LoadAssetAtPath<ShapeShifterConfiguration>(configurationPath);
            }

            
            if (configuration == null)
            {
                configuration = CreateInstance<ShapeShifterConfiguration>();


                if (!AssetDatabase.IsValidFolder(ConfigurationResourceFolderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                AssetDatabase.CreateAsset(
                    configuration,
                    ConfigurationResourceFolderPath + ConfigurationResource
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

                if (GUILayout.Button("Test"))
                {
                    GitUtils.ChangedFileGitInfo[] changedFiles = GitUtils.GetAllChangedFilesGitInfo();

                    var d1 = GitUtils.GetDeletedFiles(changedFiles);

                    var d2 = GitUtils.GetUnstagedFiles(changedFiles);
                    
                    Debug.Log("Unstaged");
                    foreach (GitUtils.ChangedFileGitInfo fileGitInfo in d2)
                    {
                        Debug.Log(fileGitInfo.path);
                    }
                }
                
                if (GUILayout.Button("Can Stage"))
                {
                    var selection = Selection.GetFiltered<Object>(SelectionMode.Assets).FirstOrDefault();

                    Debug.Log($"CanStage: {GitUtils.CanStage(AssetDatabase.GetAssetPath(selection))}");
                }

                if (GUILayout.Button("Can Unstage"))
                {
                    var selection = Selection.GetFiltered<Object>(SelectionMode.Assets).FirstOrDefault();

                    Debug.Log($"CanStage: {GitUtils.CanUnstage(AssetDatabase.GetAssetPath(selection))}");
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
