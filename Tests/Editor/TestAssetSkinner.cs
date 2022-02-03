using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class TestAssetSkinner : TestBase
    {
      
        [Test]
        public void TestSkinSprite()
        {
            Sprite squareSprite = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);
            AssetSkinner.SkinAsset(AssetDatabase.GetAssetPath(squareSprite));
            Assert.IsTrue(AssetSkinner.IsSkinned(AssetDatabase.GetAssetPath(squareSprite)), "Asset should be skinned");
        }

        [Test]
        public void TestSkinFolder()
        {
            DefaultAsset folderAsset = TestUtils.GetAsset<DefaultAsset>(TestUtils.FolderAssetName);
            
            Assert.IsNotNull(folderAsset, "Could not find test folder asset");

            string assetPath = AssetDatabase.GetAssetPath(folderAsset);
            string fullAssetPath = PathUtils.GetFullPath(assetPath);
            
            AssetSkinner.SkinAsset(assetPath);
            
            Assert.IsTrue(AssetSkinner.IsSkinned(assetPath));
            
            DirectoryInfo directoryInfo = new DirectoryInfo(fullAssetPath);

            int assetCountInFolder = PathUtils.GetAssetCountInFolder(fullAssetPath);
            
            Assert.IsTrue(directoryInfo.EnumerateFiles().Count() == 6);
            Assert.IsTrue(assetCountInFolder == 3);
            
        }
        

        [Test]
        public void TestGitIgnoreAfterSkinningOperations()
        {
            Sprite squareSprite = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);
            AssetSkinner.SkinAsset(AssetDatabase.GetAssetPath(squareSprite));
            string assetPath = AssetDatabase.GetAssetPath(squareSprite);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            
            Assert.IsTrue(GitIgnore.IsIgnored(guid), "GUID should be in git ignore");
            var ignoredPath = GitIgnore.GetIgnoredPathByGUID(guid);
            Assert.IsTrue(ignoredPath == PathUtils.GetPathRelativeToRepositoryFolder(assetPath), $"Asset path {assetPath} is different from {ignoredPath}");
            Assert.IsTrue(PathUtils.GetFullPath(ignoredPath) == PathUtils.GetFullPath(assetPath), $"Asset path {assetPath} is different from {ignoredPath}");
            
            AssetSkinner.RemoveSkins(assetPath);
            Assert.IsTrue(!GitIgnore.IsIgnored(guid), "GUID should not be in git ignore");
        }
        
    }
}