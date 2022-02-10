using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Switcher;
using UnityEditor;

namespace Miniclip.ShapeShifter.Utils
{
    internal static class ShapeShifterUtils
    {
        internal static string GenerateUniqueAssetSkinKey(string game, string guid) => game + ":" + guid;

        internal static string GetGameName(int index) => ShapeShifterConfiguration.GetGameNameAtIndex(index);

        internal static void SavePendingChanges()
        {
            AssetDatabase.SaveAssets();

            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }

        public static void DeleteDSStoreFiles()
        {
            foreach (string gameName in ShapeShifterConfiguration.GetGameNames())
            {
                GameSkin gameSkin = new GameSkin(gameName);

                if (!gameSkin.HasValidFolders())
                {
                    continue;
                }
                
                IEnumerable<string> ds_stores = Directory.EnumerateFiles(
                    gameSkin.MainFolder,
                    ".DS_Store",
                    SearchOption.AllDirectories
                );

                foreach (string dsStore in ds_stores)
                {
                    File.Delete(dsStore);
                }
            }
        }
    }
}