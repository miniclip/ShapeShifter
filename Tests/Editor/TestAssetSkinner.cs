using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
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
            ShapeShifter.SkinAsset(AssetDatabase.GetAssetPath(squareSprite));
            Assert.IsTrue(ShapeShifter.IsSkinned(AssetDatabase.GetAssetPath(squareSprite)), "Asset should be skinned");
        }

        [Test]
        public void TestSkinFolder()
        {
            DefaultAsset folderAsset = TestUtils.GetAsset<DefaultAsset>(TestUtils.FolderAssetName);
            
            Assert.IsNotNull(folderAsset, "Could not find test folder asset");

            string assetPath = AssetDatabase.GetAssetPath(folderAsset);
            string fullAssetPath = PathUtils.GetFullPath(assetPath);
            
            ShapeShifter.SkinAsset(assetPath);
            
            Assert.IsTrue(ShapeShifter.IsSkinned(assetPath));
            
            DirectoryInfo directoryInfo = new DirectoryInfo(fullAssetPath);

            int assetCountInFolder = PathUtils.GetAssetCountInFolder(fullAssetPath);
            
            Assert.IsTrue(directoryInfo.EnumerateFiles().Count() == 6);
            Assert.IsTrue(assetCountInFolder == 3);
            
        }
        

        [Test]
        public void TestGitIgnoreAfterSkinningOperations()
        {
            Sprite squareSprite = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);
            ShapeShifter.SkinAsset(AssetDatabase.GetAssetPath(squareSprite));
            string assetPath = AssetDatabase.GetAssetPath(squareSprite);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            
            Assert.IsTrue(GitUtils.IsIgnored(guid), "GUID should be in git ignore");
            var ignoredPath = GitUtils.GetIgnoredPathByGUID(guid);
            Assert.IsTrue(ignoredPath == PathUtils.GetPathRelativeToRepositoryFolder(assetPath), $"Asset path {assetPath} is different from {ignoredPath}");
            Assert.IsTrue(PathUtils.GetFullPath(ignoredPath) == PathUtils.GetFullPath(assetPath), $"Asset path {assetPath} is different from {ignoredPath}");
            
            ShapeShifter.RemoveSkins(assetPath);
            Assert.IsTrue(!GitUtils.IsIgnored(guid), "GUID should not be in git ignore");
        }
        
    }
}