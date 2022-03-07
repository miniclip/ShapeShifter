using System;
using System.IO;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class TestPathUtils : TestBase
    {
        [Test]
        public void TestPathOperations()
        {
            string basePath = Directory.GetCurrentDirectory();

            Assert.IsFalse(PathUtils.IsPathRelativeToAssets(basePath));

            string assetPath = AssetDatabase.GetAssetPath(TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName));

            Assert.IsTrue(PathUtils.IsPathRelativeToAssets(assetPath));

            string assetFullPath = PathUtils.GetFullPath(assetPath);

            Assert.IsFalse(PathUtils.IsPathRelativeToAssets(assetFullPath));

            Assert.IsTrue(PathUtils.IsPathRelativeToAssets(PathUtils.GetPathRelativeToAssetsFolder(assetFullPath)));
        }

        [Test]
        public void TestFullPathFromSkinPath()
        {
            var squareSprite = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);
            Assert.IsNotNull(squareSprite, $"Could not find {TestUtils.SpriteAssetName} on resources");

            string assetPath = AssetDatabase.GetAssetPath(squareSprite);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            Assert.IsFalse(AssetSkinner.IsSkinned(assetPath), "Asset should not be skinned");

            AssetSkinner.SkinAsset(assetPath);

            Assert.IsTrue(AssetSkinner.IsSkinned(assetPath), "Asset should be skinned");
            Assert.IsTrue(ShapeShifter.ActiveGameSkin.HasAssetSkin(guid), "Active Game skin should have this GUID.");
            Assert.IsTrue(PathUtils.IsPathRelativeToAssets(assetPath));

            var assetSkin = ShapeShifter.ActiveGameSkin.GetAssetSkin(guid);
            Assert.IsNotNull(assetSkin);

            string assetSkinPath = assetSkin.FolderPath;

            Assert.IsFalse(string.IsNullOrEmpty(assetSkinPath));
            Assert.IsTrue(Path.IsPathRooted(assetSkinPath));
        }

        [Test]
        public void TestIfFileOrDirectoryExists()
        {
            string fakePath = Path.Combine(Application.dataPath, "fake");

            Assert.IsFalse(PathUtils.FileOrDirectoryExists(fakePath));

            var squareSprite = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);
            string assetPath = AssetDatabase.GetAssetPath(squareSprite);

            Assert.IsTrue(PathUtils.FileOrDirectoryExists(assetPath));
        }

        [Test]
        public void TestIfIsFileOrDirectory()
        {
            string directoryPath1 = Application.dataPath;
            string directoryPath2 = Application.persistentDataPath;

            Assert.IsTrue(PathUtils.IsDirectory(directoryPath1));
            Assert.IsTrue(PathUtils.IsDirectory(directoryPath2));

            Assert.IsFalse(PathUtils.IsFile(directoryPath1));
            Assert.IsFalse(PathUtils.IsFile(directoryPath2));

            ArgumentException exception = Assert.Throws<ArgumentException>(() => PathUtils.IsDirectory(""));
            Assert.IsTrue(
                exception.Message.Contains("Path given is empty or null"),
                "Exception message is not the expected"
            );

            Sprite squareSprite = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);
            string assetPath = AssetDatabase.GetAssetPath(squareSprite);

            Assert.IsTrue(PathUtils.IsFile(assetPath));
            Assert.IsFalse(PathUtils.IsDirectory(assetPath));
        }

        [Test]
        public void TestCountingAssetsInFolder()
        {
            int assetCountInFolder = PathUtils.GetAssetCountInFolder(TestUtils.TempFolderName);
            Assert.IsTrue(assetCountInFolder == 2, $"Counted {assetCountInFolder} assets in folder. Expected 2.");
        }

        [Test]
        public void TestValidatePathSafety()
        {
            DirectoryInfo assets = new DirectoryInfo(Application.dataPath);
            DirectoryInfo project = assets.Parent;
            string repositoryPath = GitUtils.MainRepositoryPath;
            
            Assert.Throws<ArgumentException>(() => FileUtils.ValidatePathSafety("/Users"));
            Assert.Throws<ArgumentException>(() => FileUtils.ValidatePathSafety(repositoryPath));
            Assert.Throws<ArgumentException>(() => FileUtils.ValidatePathSafety(string.Empty));
            Assert.Throws<ArgumentException>(() => FileUtils.ValidatePathSafety(assets.FullName));
            Assert.Throws<ArgumentException>(() => FileUtils.ValidatePathSafety(project?.FullName));

            Assert.Throws<ArgumentException>(
                () => FileUtils.ValidatePathSafety(assets.FullName + Path.DirectorySeparatorChar)
            );
            Assert.Throws<ArgumentException>(
                () => FileUtils.ValidatePathSafety(project?.FullName + Path.DirectorySeparatorChar)
            );
        }
    }
}