using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public class ShapeShifterConfiguration : ScriptableObject
    {
        public Editor DefaultConfigurationEditor { get; set; }

        public Editor ExternalConfigurationEditor { get; set; }

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

        private static readonly string ConfigurationResource = $"{SharedInfo.SHAPESHIFTER_NAME}Configuration.asset";
        private static readonly string ConfigurationResourceFolderPath = "Assets/Editor Default Resources/";

        internal static void Initialise()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = (ShapeShifterConfiguration)EditorGUIUtility.Load(
                ConfigurationResource
            );

            string configurationPath = Path.Combine(
                ConfigurationResourceFolderPath,
                ConfigurationResource
            );

            if (Instance == null && File.Exists(configurationPath))
            {
                Instance = AssetDatabase.LoadAssetAtPath<ShapeShifterConfiguration>(configurationPath);
            }

            if (Instance == null)
            {
                Instance = CreateInstance<ShapeShifterConfiguration>();

                if (!AssetDatabase.IsValidFolder(ConfigurationResourceFolderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                AssetDatabase.CreateAsset(
                    Instance,
                    ConfigurationResourceFolderPath + ConfigurationResource
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
                    "Shapeshifter has no configured games, creating a default one and making it active"
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