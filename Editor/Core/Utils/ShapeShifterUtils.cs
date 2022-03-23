using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Saver;
using Miniclip.ShapeShifter.Switcher;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    internal static class ShapeShifterUtils
    {
        internal static string GenerateUniqueAssetSkinKey(string game, string guid) => game + ":" + guid;

        internal static void SavePendingChanges()
        {
            AssetDatabase.SaveAssets();
            AssetSaver.SaveToActiveGameSkin();
            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }

        public static void DeleteDSStoreFiles()
        {
            foreach (string gameName in ShapeShifterConfiguration.Instance.GameNames)
            {
                GameSkin gameSkin = new GameSkin(gameName);

                if (!gameSkin.HasValidFolders())
                {
                    continue;
                }

                IEnumerable<string> ds_stores = Directory.EnumerateFiles(
                    gameSkin.MainFolderPath,
                    ".DS_Store",
                    SearchOption.AllDirectories
                );

                foreach (string dsStore in ds_stores)
                {
                    File.Delete(dsStore);
                }
            }
        }

        public static string GetUniqueTemporaryGameName(string gameName)
        {
            int nameAttempt = 0;

            var gameNames = ShapeShifterConfiguration.Instance.GameNames;
            while (true)
            {
                nameAttempt++;
                string newName = $"{gameName} {nameAttempt}";

                if (gameNames.Contains(newName))
                    continue;

                return newName;
            }
        }

        public static string GetAssetPath(this UnityEngine.Object obj)
        {
            return AssetDatabase.GetAssetPath(obj);
        }
    }
}