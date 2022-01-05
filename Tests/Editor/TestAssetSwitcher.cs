using System;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests

{
    public class TestAssetSwitcher
    {
        [SetUp]
        public void Setup()
        {
            TestUtils.Reset();
        }

        [Test]
        public void TestSwitchWithTextAsset()
        {
            TextAsset testAsset = TestUtils.GetAsset<TextAsset>(TestUtils.TextFileAssetName);
            string assetPath = AssetDatabase.GetAssetPath(testAsset);
            string assetFullPath = PathUtils.GetFullPath(assetPath);

            string textBefore = File.ReadAllText(assetFullPath);

            Assert.IsNotNull(testAsset, "Text asset not found");
            Assert.IsNotEmpty(assetPath, "Text asset path is empty");
            Assert.IsTrue(!ShapeShifter.IsSkinned(assetPath));

            ShapeShifter.SkinAsset(assetPath);
            
            ShapeShifter.SwitchToGame(1, true);
            Assert.IsTrue(ShapeShifter.ActiveGame == 1, "Active game should be 1");

            string textAfter = File.ReadAllText(assetFullPath);
            Assert.IsTrue(string.Equals(textBefore, textAfter, StringComparison.Ordinal));

            ShapeShifter.SwitchToGame(0, true);
            Assert.IsTrue(ShapeShifter.ActiveGame == 0, "Active game should be 0");

            File.WriteAllText(assetFullPath, ShapeShifter.ActiveGameSkin.Name);

            textAfter = File.ReadAllText(assetFullPath);
            Assert.IsFalse(
                string.Equals(textBefore, textAfter, StringComparison.Ordinal),
                "Text file content should still be the same"
            );

            ShapeShifter.OverwriteSelectedSkin(0, true);

            ShapeShifter.SwitchToGame(1, true);

            File.WriteAllText(assetFullPath, ShapeShifter.ActiveGameSkin.Name);

            textAfter = File.ReadAllText(assetFullPath);
            Assert.IsFalse(
                string.Equals(textBefore, textAfter, StringComparison.Ordinal),
                "Text file content should not be the same"
            );

            ShapeShifter.OverwriteSelectedSkin(1, true);

            ShapeShifter.SwitchToGame(0, true);

            string textGame0 = File.ReadAllText(assetFullPath);
            Assert.IsTrue(
                string.Equals(textGame0, "Game0", StringComparison.Ordinal),
                "Text file content should be Game0"
            );

            ShapeShifter.SwitchToGame(1, true);

            string textGame1 = File.ReadAllText(assetFullPath);
            Assert.IsTrue(
                string.Equals(textGame1, "Game1", StringComparison.Ordinal),
                "Text file content should be Game1"
            );
        }

        [TearDown]
        public void TearDown()
        {
            TestUtils.TearDown();
        }
    }
}