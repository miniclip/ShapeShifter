using System.IO;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class TestAssetRestore : TestBase
    {
        [Test]
        public void TestRestoreSprite()
        {
            Sprite asset = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);

            string assetPath = AssetDatabase.GetAssetPath(asset);
            string fullAssetPath = PathUtils.GetFullPath(assetPath);

            AssetSkinner.SkinAsset(assetPath);
            Assert.IsTrue(File.Exists(fullAssetPath));

            FileUtil.DeleteFileOrDirectory(fullAssetPath);

            Assert.IsFalse(File.Exists(fullAssetPath));

            AssetSwitcher.RestoreActiveGame();

            Assert.IsTrue(File.Exists(fullAssetPath));
        }

        [Test]
        public void TestRestoreFolder()
        {
            DefaultAsset folderAsset = TestUtils.GetAsset<DefaultAsset>(TestUtils.FolderAssetName);

            Assert.IsNotNull(folderAsset, "Could not find test folder asset");

            string assetPath = AssetDatabase.GetAssetPath(folderAsset);
            string fullAssetPath = PathUtils.GetFullPath(assetPath);

            AssetSkinner.SkinAsset(assetPath);

            Assert.IsTrue(Directory.Exists(fullAssetPath));

            FileUtil.DeleteFileOrDirectory(fullAssetPath);

            Assert.IsFalse(Directory.Exists(fullAssetPath));

            AssetSwitcher.RestoreActiveGame();

            Assert.IsTrue(Directory.Exists(fullAssetPath), "Folder should have been restored");
        }
    }
}