using System.IO;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class PathUtilsTests
    {
        [SetUp]
        public void Setup()
        {
            TestUtils.Reset();
        }

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

        [TearDown]
        public void TearDown()
        {
            TestUtils.TearDown();
        }
    }
}