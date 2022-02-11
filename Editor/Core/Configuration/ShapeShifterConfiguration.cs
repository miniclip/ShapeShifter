using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [SerializeField]
        [HideInInspector]
        private List<string> gameNames = new List<string>();

        [SerializeField]
        private bool hasUnsavedChanges;

        public bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;
            set => hasUnsavedChanges = value;
        }

        [SerializeField]
        [HideInInspector]
        private List<string> skinnedExternalAssetPaths = new List<string>();
        public List<string> SkinnedExternalAssetPaths => skinnedExternalAssetPaths;
        internal static ShapeShifterConfiguration Instance { get; private set; }

        public List<string> GameNames => gameNames;

        private const string CONFIGURATION_RESOURCE = "ShapeShifterConfiguration.asset";
        private const string CONFIGURATION_RESOURCE_FOLDER_PATH = "Assets/Editor Default Resources/";

        public static void RemoveAllGames(bool deleteFolders = true)
        {
            foreach (string instanceGameName in Instance.GameNames.ToList())
            {
                RemoveGame(instanceGameName, deleteFolders);
            }
        }

        public static string GetGameNameAtIndex(int index)
        {
            if (!IsInitialized())
            {
                throw new Exception("Configuration not initialized");
            }

            if (index >= Instance.GameNames.Count)
            {
                throw new Exception("Index is bigger than current game count");
            }

            return Instance.GameNames[index];
        }

        public static IReadOnlyCollection<string> GetGameNames() => Instance.GameNames.AsReadOnly();

        internal static void Initialise()
        {
            if (Instance == null)
            {
                Instance = (ShapeShifterConfiguration)EditorGUIUtility.Load(
                    CONFIGURATION_RESOURCE
                );
            }

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
                AddGame(Application.productName);
                AssetSwitcher.SwitchToGame(0);
                EditorUtility.SetDirty(Instance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        internal static bool IsInitialized() => Instance != null && Instance.GameNames.Count > 0;

        internal static void AddGame(string gameName)
        {
            if (Instance.GameNames.Contains(gameName))
            {
                return;
            }

            Instance.GameNames.Add(gameName);

            GameSkin gameSkin = new GameSkin(gameName);

            IOUtils.TryCreateDirectory(gameSkin.MainFolder);
        }

        internal static void RemoveGame(string gameName, bool deleteFolder)
        {
            if (Instance.GameNames.Contains(gameName))
            {
                Instance.GameNames.Remove(gameName);
            }

            if (deleteFolder)
            {
                GameSkin gameSkin = new GameSkin(gameName);
                gameSkin.DeleteFolder();
            }
        }
    }
}