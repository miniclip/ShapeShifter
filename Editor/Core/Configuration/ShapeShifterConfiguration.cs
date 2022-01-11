using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public class ShapeShifterConfiguration : ScriptableObject
    {
        public Editor DefaultConfigurationEditor { get; private set; }

        public Editor ExternalConfigurationEditor { get; private set; }

        //TODO: turn these lists into serializable HashSets 

        [SerializeField]
        [HideInInspector]
        private List<string> gameNames = new List<string>();

        [SerializeField]
        [HideInInspector]
        private List<string> modifiedAssetPaths = new List<string>();

        public List<string> ModifiedAssetPaths => modifiedAssetPaths;

        [SerializeField]
        [HideInInspector]
        private List<string> skinnedExternalAssetPaths = new List<string>();
        public List<string> SkinnedExternalAssetPaths => skinnedExternalAssetPaths;
        internal static ShapeShifterConfiguration Instance { get; private set; }

        internal List<string> GameNames
        {
            get => gameNames;
            set => gameNames = value;
        }

        private const string CONFIGURATION_RESOURCE = "ShapeShifterConfiguration.asset";
        private const string CONFIGURATION_RESOURCE_FOLDER_PATH = "Assets/Editor Default Resources/";

        internal static void Initialise()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = (ShapeShifterConfiguration)EditorGUIUtility.Load(
                CONFIGURATION_RESOURCE
            );

            string configurationPath = Path.Combine(
                CONFIGURATION_RESOURCE_FOLDER_PATH,
                CONFIGURATION_RESOURCE
            );

            if (Instance == null && File.Exists(configurationPath))
            {
                Instance = AssetDatabase.LoadAssetAtPath<ShapeShifterConfiguration>(configurationPath);
            }

            if (Instance == null)
            {
                Instance = CreateInstance<ShapeShifterConfiguration>();

                if (!AssetDatabase.IsValidFolder(CONFIGURATION_RESOURCE_FOLDER_PATH))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                AssetDatabase.CreateAsset(
                    Instance,
                    CONFIGURATION_RESOURCE_FOLDER_PATH + CONFIGURATION_RESOURCE
                );

                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Instance.DefaultConfigurationEditor = Editor.CreateEditor(
                Instance,
                typeof(ShapeShifterConfigurationEditor)
            );

            Instance.ExternalConfigurationEditor = Editor.CreateEditor(
                Instance,
                typeof(ShapeShifterExternalConfigurationEditor)
            );

            if (Instance.GameNames.Count == 0)
            {
                ShapeShifterLogger.Log(
                    "ShapeShifter has no configured games, creating a default one and making it active"
                );
                Instance.GameNames.Add("Default");
                AssetSwitcher.SwitchToGame(0);
                EditorUtility.SetDirty(Instance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}