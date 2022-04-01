using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    class TestUtils
    {
        internal static string TempFolderName = "Assets/ShapeShifterTestAssets";

        internal static string SpriteAssetName = "shapeshifter.test.sprite";
        internal static string TextFileAssetName = "shapeshifter.test.textfile";
        internal static string FolderAssetName = "shapeshifter.test.folder";

        internal static GameSkin game0 = new GameSkin("Test0");
        internal static GameSkin game1 = new GameSkin("Test1");
        
        internal static string[] TestGameNames =
        {
            "Test0", "Test1",
        };

        private static List<string> cachedGameNames;

        internal static string GetAssetPath(string name)
        {
            string[] assetGUIDs = AssetDatabase.FindAssets(name);
            string assetGUID = assetGUIDs.FirstOrDefault(
                guid => PathUtils.IsPathRelativeToAssets(AssetDatabase.GUIDToAssetPath(guid))
            );
            return AssetDatabase.GUIDToAssetPath(assetGUID);
        }

        internal static string GetPackageAssetPath(string name)
        {
            string[] assetGUIDs = AssetDatabase.FindAssets(name);
            string assetGUID = assetGUIDs.FirstOrDefault(
                guid => PathUtils.IsPathRelativeToPackages(AssetDatabase.GUIDToAssetPath(guid))
            );
            return AssetDatabase.GUIDToAssetPath(assetGUID);
        }

        internal static T GetAsset<T>(string name)
        where T : Object
        {
            string assetPath = GetAssetPath(name);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsNotNull(asset, $"Could not load {name}");
            return asset;
        }

        internal static void Reset()
        {
            CopyAllTestResourcesFromPackagesToAssetsFolder();

            AssetDatabase.Refresh();

            cachedGameNames = new List<string>(ShapeShifterConfiguration.Instance.GameNames);
            ShapeShifter.RemoveAllGames(deleteFolders:false);

            foreach (string testGameName in TestGameNames)
            {
                ShapeShifterConfiguration.Instance.AddGame(testGameName);
            }

            ShapeShifterConfiguration.Instance.SetDirty(false);
        }

        private static void CopyAllTestResourcesFromPackagesToAssetsFolder()
        {
            FileUtils.TryCreateDirectory(TempFolderName, true);

            CopyResourceFromPackagesToAssetsFolder(SpriteAssetName);
            CopyResourceFromPackagesToAssetsFolder(TextFileAssetName);
            CopyResourceFromPackagesToAssetsFolder(FolderAssetName);
        }

        private static void CopyResourceFromPackagesToAssetsFolder(string assetName)
        {
            string packageAssetPath = GetPackageAssetPath(assetName);
            string destination = Path.Combine(TempFolderName, Path.GetFileName(packageAssetPath));
            AssetDatabase.CopyAsset(packageAssetPath, destination);
        }

        internal static void TearDown()
        {
            if (AssetSkinner.IsSkinned(GetAssetPath(SpriteAssetName)))
            {
                AssetSkinner.RemoveSkins(GetAssetPath(SpriteAssetName));
            }

            if (AssetSkinner.IsSkinned(GetAssetPath(TextFileAssetName)))
            {
                AssetSkinner.RemoveSkins(GetAssetPath(TextFileAssetName));
            }

            if (AssetSkinner.IsSkinned(GetAssetPath(FolderAssetName)))
            {
                AssetSkinner.RemoveSkins(GetAssetPath(FolderAssetName));
            }

            FileUtil.DeleteFileOrDirectory(TempFolderName);
            FileUtil.DeleteFileOrDirectory(TempFolderName + ".meta");

            foreach (string configurationGameName in ShapeShifterConfiguration.Instance.GameNames.ToList())
            {
                if (!configurationGameName.Contains("Test"))
                {
                    continue;
                }
                
                ShapeShifterConfiguration.Instance.RemoveGame(configurationGameName, true);
            }

            GitUtils.Stage(TempFolderName);
            AssetDatabase.Refresh();

            foreach (string cachedGameName in cachedGameNames)
            {
                ShapeShifterConfiguration.Instance.AddGame(cachedGameName);
            }
        }
    }
}