using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class TestAssetSkinner
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Debug.Log("Setup");
            TestUtils.Reset();
        }

        [Test]
        public void TestSkinSprite()
        {
            Sprite squareSprite = TestUtils.SkinTestSprite();

            Assert.IsTrue(ShapeShifter.IsSkinned(AssetDatabase.GetAssetPath(squareSprite)), "Asset should be skinned");
        }
        

        [Test]
        public void SkinnedAssetIsAddedToGitIgnore()
        {
            Sprite squareSprite = TestUtils.SkinTestSprite();
            string assetPath = AssetDatabase.GetAssetPath(squareSprite);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            
            Assert.IsTrue(GitUtils.IsIgnored(guid), "GUID should be in git ignore");
            var ignoredPath = GitUtils.GetIgnoredPath(guid);
            Assert.IsTrue(ignoredPath == PathUtils.GetPathRelativeToRepositoryFolder(assetPath), $"Asset path {assetPath} is different from {ignoredPath}");
            Assert.IsTrue(PathUtils.GetFullPath(ignoredPath) == PathUtils.GetFullPath(assetPath), $"Asset path {assetPath} is different from {ignoredPath}");
        }
        
        [Test]
        public void UnskinnedAssetIsRemovedFromGitIgnore()
        {
            Sprite squareSprite = TestUtils.SkinTestSprite();
            string assetPath = AssetDatabase.GetAssetPath(squareSprite);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            
            Assert.IsTrue(GitUtils.IsIgnored(guid), "GUID should be in git ignore");
            
            ShapeShifter.RemoveSkins(assetPath);
            Assert.IsTrue(!GitUtils.IsIgnored(guid), "GUID should not be in git ignore");
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            TestUtils.TearDown();
        }
    }
}