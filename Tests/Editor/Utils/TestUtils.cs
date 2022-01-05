using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    class TestUtils
    {
        private static string TempFolderName = "Assets/ShapeShifterTestAssets";

        internal static string SpriteAssetName = "shapeshifter.test.sprite";
        internal static string TextFileAssetName = "shapeshifter.test.textfile";
        internal static string FolderAssetName = "shapeshifter.test.folder";
        
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
            
            cachedGameNames = new List<string>(ShapeShifter.Configuration.GameNames);
            ShapeShifter.Configuration.GameNames.Clear();
            ShapeShifter.Configuration.GameNames.Add("Game0");
            ShapeShifter.Configuration.GameNames.Add("Game1");

        }

        private static void CopyAllTestResourcesFromPackagesToAssetsFolder()
        {
            IOUtils.TryCreateDirectory(TempFolderName, true);

            CopyResourceFromPackagesToAssetsFolder(SpriteAssetName);
            CopyResourceFromPackagesToAssetsFolder(TextFileAssetName);
            CopyResourceFromPackagesToAssetsFolder(FolderAssetName);
        }

        private static void CopyResourceFromPackagesToAssetsFolder(string assetName)
        {
            string packageAssetPath = GetPackageAssetPath(assetName);
            string source = Path.GetFullPath(packageAssetPath);
            string destination = Path.Combine(Path.GetFullPath(TempFolderName), Path.GetFileName(packageAssetPath));
            FileUtil.CopyFileOrDirectory(source, destination);
        }

        internal static void TearDown()
        {
            if (ShapeShifter.IsSkinned(GetAssetPath(SpriteAssetName)))
            {
                ShapeShifter.RemoveSkins(GetAssetPath(SpriteAssetName));
            }

            if (ShapeShifter.IsSkinned(GetAssetPath(TextFileAssetName)))
            {
                ShapeShifter.RemoveSkins(GetAssetPath(TextFileAssetName));
            }

            if (ShapeShifter.IsSkinned(GetAssetPath(FolderAssetName)))
            {
                ShapeShifter.RemoveSkins(GetAssetPath(FolderAssetName));
            }

            
            FileUtil.DeleteFileOrDirectory(TempFolderName);
            FileUtil.DeleteFileOrDirectory(TempFolderName + ".meta");
            GitUtils.Stage(TempFolderName);
            AssetDatabase.Refresh();
            ShapeShifter.Configuration.GameNames = cachedGameNames;
        }
    }
}