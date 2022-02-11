using System;
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
        [SerializeField]
        [HideInInspector]
        private List<string> gameNames = new List<string>();

        public List<string> GameNames => gameNames;
        public bool IsDirty => isDirty;

        [SerializeField]
        [HideInInspector]
        private List<string> skinnedExternalAssetPaths = new List<string>();
        public List<string> SkinnedExternalAssetPaths => skinnedExternalAssetPaths;

        internal static ShapeShifterConfiguration Instance { get; private set; }

        internal Editor DefaultConfigurationEditor { get; private set; }
        internal Editor ExternalConfigurationEditor { get; private set; }

        private bool isDirty;

        public void SetDirty(bool isDirty = true) => Instance.isDirty = isDirty;

        public void Save() => isDirty = false;

        public string GetGameNameAtIndex(int index)
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

        public void RenameGame(int index, string newName)
        {
            GameSkin gameSkin = new GameSkin(gameNames[index]);
            gameSkin.RenameFolder(newName);
            gameNames[index] = newName;
        }

        internal void AddGame(string gameName)
        {
            if (Instance.GameNames.Contains(gameName))
            {
                return;
            }

            Instance.GameNames.Add(gameName);

            GameSkin gameSkin = new GameSkin(gameName);

            IOUtils.TryCreateDirectory(gameSkin.MainFolder);
        }

        internal void RemoveGame(string gameName, bool deleteFolder)
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

        internal static bool IsInitialized() => Instance != null && Instance.GameNames.Count > 0;

        internal static void Initialise()
        {
            if (Instance == null)
            {
                Instance = (ShapeShifterConfiguration)EditorGUIUtility.Load(
                    ShapeShifterConstants.CONFIGURATION_RESOURCE
                );
            }

            string configurationPath = Path.Combine(
                ShapeShifterConstants.CONFIGURATION_RESOURCE_FOLDER_PATH,
                ShapeShifterConstants.CONFIGURATION_RESOURCE
            );

            if (Instance == null && File.Exists(configurationPath))
            {
                Instance = AssetDatabase.LoadAssetAtPath<ShapeShifterConfiguration>(configurationPath);
            }

            if (Instance == null)
            {
                Instance = CreateInstance<ShapeShifterConfiguration>();

                if (!AssetDatabase.IsValidFolder(ShapeShifterConstants.CONFIGURATION_RESOURCE_FOLDER_PATH))
                {
                    AssetDatabase.CreateFolder("Assets", "Editor Default Resources");
                }

                AssetDatabase.CreateAsset(
                    Instance,
                    ShapeShifterConstants.CONFIGURATION_RESOURCE_FOLDER_PATH
                    + ShapeShifterConstants.CONFIGURATION_RESOURCE
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
                Instance.AddGame(Application.productName);
                AssetSwitcher.SwitchToGame(0);
                EditorUtility.SetDirty(Instance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}