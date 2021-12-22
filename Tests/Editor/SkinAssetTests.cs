using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class SkinAssetTests
    {
        [SetUp]
        public void Setup() { }

        [Test]
        public void SkinOneAsset()
        {
            Texture2D square = Resources.FindObjectsOfTypeAll<Texture2D>()
                .FirstOrDefault(tex => tex.name.Equals(TestableAssets.SpriteAssetName));

            Assert.IsNotNull(square, $"Could not find {TestableAssets.SpriteAssetName} on resources");

            // Assert.IsFalse(ShapeShifter.IsSkinned(AssetDatabase.GetAssetPath(square)));
        }
    }
}