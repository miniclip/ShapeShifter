using System;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests

{
    public class TestAssetSwitcher : TestBase
    {
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
                string.Equals(textGame0, TestUtils.TestGameNames[0], StringComparison.Ordinal),
                $"Text file content should be {TestUtils.TestGameNames[0]}"
            );

            ShapeShifter.SwitchToGame(1, true);

            string textGame1 = File.ReadAllText(assetFullPath);
            Assert.IsTrue(
                string.Equals(textGame1, TestUtils.TestGameNames[1], StringComparison.Ordinal),
                $"Text file content should be {TestUtils.TestGameNames[1]}"
            );
        }

        [Test]
        public void TestSwitchWithFolder()
        {
            DefaultAsset folderAsset = TestUtils.GetAsset<DefaultAsset>(TestUtils.FolderAssetName);
            string assetPath = AssetDatabase.GetAssetPath(folderAsset);
            string fullAssetPath = PathUtils.GetFullPath(assetPath);

            ShapeShifter.SkinAsset(assetPath);

            Assert.IsTrue(ShapeShifter.IsSkinned(assetPath), "Folder was not skinned");

            ShapeShifter.SwitchToGame(0, true);

            string skinPath = Path.Combine(ShapeShifter.ActiveGameSkin.GetAssetSkin(AssetDatabase.AssetPathToGUID(assetPath)).FolderPath, TestUtils.FolderAssetName);

            Assert.IsTrue(PathUtils.GetAssetCountInFolder(fullAssetPath) == 3, "Incorrect number of files in folder before folder modification");
            Assert.IsTrue(PathUtils.GetAssetCountInFolder(skinPath) == 3, "Skinned folder does not have same number of files as original");

            DirectoryInfo directoryInfo = new DirectoryInfo(fullAssetPath);

            var fileToDelete = directoryInfo.GetFiles().FirstOrDefault(info => info.Extension != ".meta");
            fileToDelete.Delete();

            AssetDatabase.Refresh();

            Assert.IsTrue(PathUtils.GetAssetCountInFolder(fullAssetPath) == 2, "Expected to have 1 less file after deleting one");
            Assert.IsTrue(PathUtils.GetAssetCountInFolder(skinPath) == 3, "Skinned folder should still have the original file amount");

            ShapeShifter.OverwriteSelectedSkin(0);
            
            Assert.IsTrue(PathUtils.GetAssetCountInFolder(skinPath) == 2, "Skinned folder should have 2 files only after overwrite");
            
            ShapeShifter.SwitchToGame(1);
            
            skinPath = Path.Combine(ShapeShifter.ActiveGameSkin.GetAssetSkin(AssetDatabase.AssetPathToGUID(assetPath)).FolderPath, TestUtils.FolderAssetName);

            Assert.IsTrue(PathUtils.GetAssetCountInFolder(fullAssetPath) == 3, "Expected to have 3 files after switching to unmodified skinned folder");
            Assert.IsTrue(PathUtils.GetAssetCountInFolder(skinPath) == 3, "Skinned folder should still have the original file amount");
        }

    }
}