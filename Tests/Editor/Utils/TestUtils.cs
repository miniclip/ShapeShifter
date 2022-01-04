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

        internal static string SpriteAssetName = "shapeshifter.test.square";

        internal static Sprite SkinTestSprite()
        {
            Sprite testSprite = GetTestSprite();

            ShapeShifter.SkinAsset(AssetDatabase.GetAssetPath(testSprite));
            return testSprite;
        }

        internal static Sprite GetTestSprite()
        {
            Sprite testSprite = GetAsset<Sprite>(SpriteAssetName);
            Assert.IsNotNull(testSprite, $"Could not find {SpriteAssetName} on resources");
            return testSprite;
        }

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
            string squareAssetPath = GetPackageAssetPath(SpriteAssetName);
            string source = Path.GetFullPath(squareAssetPath);
            string destination = Path.Combine(Path.GetFullPath(TempFolderName), Path.GetFileName(squareAssetPath));
            IOUtils.TryCreateDirectory(TempFolderName, true);
            IOUtils.CopyFile(source, destination);

            AssetDatabase.Refresh();

            if (ShapeShifter.IsSkinned(squareAssetPath))
            {
                ShapeShifter.RemoveSkins(squareAssetPath);
            }

            if (ShapeShifter.IsSkinned(GetAssetPath(SpriteAssetName)))
            {
                ShapeShifter.RemoveSkins(GetAssetPath(SpriteAssetName));
            }

            Assert.IsFalse(ShapeShifter.IsSkinned(squareAssetPath));
        }

        internal static void TearDown()
        {
            if (ShapeShifter.IsSkinned(GetAssetPath(SpriteAssetName)))
            {
                ShapeShifter.RemoveSkins(GetAssetPath(SpriteAssetName));
            }

            FileUtil.DeleteFileOrDirectory(TempFolderName);
            FileUtil.DeleteFileOrDirectory(TempFolderName + ".meta");
            GitUtils.Stage(TempFolderName);
            AssetDatabase.Refresh();
        }
    }
}