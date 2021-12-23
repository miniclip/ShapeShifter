using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class SkinAssetTests
    {
        [SetUp]
        public void Setup()
        {
            TestUtils.Reset();
        }

        [Test]
        public void SkinOneAsset()
        {
            var squareSprite = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);
            Assert.IsNotNull(squareSprite, $"Could not find {TestUtils.SpriteAssetName} on resources");
            
            Assert.IsFalse(ShapeShifter.IsSkinned(AssetDatabase.GetAssetPath(squareSprite)), "Asset should not be skinned");
            
            ShapeShifter.SkinAsset(AssetDatabase.GetAssetPath(squareSprite));
            
            Assert.IsTrue(ShapeShifter.IsSkinned(AssetDatabase.GetAssetPath(squareSprite)), "Asset should be skinned");

        }

        [TearDown]
        public void Teardown()
        {
            TestUtils.TearDown();
        }
    }
}