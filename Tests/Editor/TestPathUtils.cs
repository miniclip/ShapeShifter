using System.IO;
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
            Assert.IsTrue(SharedInfo.ActiveGameSkin.HasGUID(guid), "Active Game skin should have this GUID.");
            Assert.IsTrue(PathUtils.IsPathRelativeToAssets(assetPath));
            
            var assetSkin = SharedInfo.ActiveGameSkin.GetAssetSkin(guid);
            Assert.IsNotNull(assetSkin);
            
            string assetSkinPath = assetSkin.FolderPath;
            
            Assert.IsFalse(string.IsNullOrEmpty(assetSkinPath));
            Assert.IsTrue(Path.IsPathRooted(assetSkinPath));
         }
        
    }
}