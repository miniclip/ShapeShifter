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

        public void RenameGame(GameSkin gameSkinToRename, string newName)
        {
            int index = gameNames.IndexOf(gameSkinToRename.Name);
            gameSkinToRename.Rename(newName);
            gameNames[index] = newName;
        }

        internal void AddGame(string gameSkinName)
        {
            if (gameNames.Contains(gameSkinName))
            {
                return;
            }

            gameNames.Add(gameSkinName);

            GameSkin gameSkin = new GameSkin(gameSkinName);

            IOUtils.TryCreateDirectory(gameSkin.MainFolderPath);
        }

        internal void RemoveGame(string gameName, bool deleteFolder)
        {
            if (gameNames.Contains(gameName))
            {
                gameNames.Remove(gameName);
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

                if (!AssetDatabase.IsValidFolder(ShapeShifterConstants.CONFIGURATION_RESOURCE_FOLDER_PATH)
                    && !Directory.Exists(
                        PathUtils.GetFullPath(ShapeShifterConstants.CONFIGURATION_RESOURCE_FOLDER_PATH)
                    ))
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

                string gameSkinName = Application.productName;

                Instance.AddGame(gameSkinName);
                AssetSwitcher.SwitchToGame(new GameSkin(gameSkinName));
                EditorUtility.SetDirty(Instance);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}