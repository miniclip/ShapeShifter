using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Saver;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class TestAssetSaver : TestBase
    {
        [Test]
        public void TestIfModifyingAssetAndSavingProjectWillTriggerOverwrite()
        {
            TextAsset testAsset = TestUtils.GetAsset<TextAsset>(TestUtils.TextFileAssetName);
            string assetPath = AssetDatabase.GetAssetPath(testAsset);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            string assetFullPath = PathUtils.GetFullPath(assetPath);

            Assert.IsNotNull(testAsset, "Text asset not found");
            Assert.IsNotEmpty(assetPath, "Text asset path is empty");
            Assert.IsTrue(!AssetSkinner.IsSkinned(assetPath));
            Assert.IsTrue(!UnsavedAssetsManager.HasUnsavedChanges(), "Should not be detecting unsaved changes");

            AssetSkinner.SkinAsset(assetPath);

            AssetSwitcher.SwitchToGame(TestUtils.game0, true);

            File.WriteAllText(assetFullPath, "new content");

            AssetDatabase.Refresh();
            
            var modifiedAssets = UnsavedAssetsManager.GetCurrentModifiedAssetsFromEditorPrefs();
            
            Assert.IsTrue(modifiedAssets.ContainsAssetPath(assetPath), "AssetPath should be in the modified assets list");
            
            Assert.IsTrue(UnsavedAssetsManager.HasUnsavedChanges(), "Should be detecting unsaved changes");

            ShapeShifterUtils.SavePendingChanges();
            string skinnedFileFullPath = Path.Combine(
                ShapeShifter.ActiveGameSkin.GetAssetSkin(guid).FolderPath,
                Path.GetFileName(assetPath)
            );
            string skinnedText = File.ReadAllText(skinnedFileFullPath);

            Assert.IsTrue(
                skinnedText == "new content",
                "Changes were not saved into skinned asset when save operation happened"
            );
        }

        [Test]
        public void TestIfModifyingFolderAndSavingProjectWillTriggerOverwrite()
        {
            DefaultAsset folderAsset = TestUtils.GetAsset<DefaultAsset>(TestUtils.FolderAssetName);
            string assetPath = AssetDatabase.GetAssetPath(folderAsset);
            string fullAssetPath = PathUtils.GetFullPath(assetPath);

            AssetSkinner.SkinAsset(assetPath);

            Assert.IsTrue(AssetSkinner.IsSkinned(assetPath), "Folder was not skinned");

            AssetSwitcher.SwitchToGame(TestUtils.game0, true);

            string skinPath = Path.Combine(
                ShapeShifter.ActiveGameSkin.GetAssetSkin(AssetDatabase.AssetPathToGUID(assetPath)).FolderPath,
                TestUtils.FolderAssetName
            );

            Assert.IsTrue(
                PathUtils.GetAssetCountInFolder(fullAssetPath) == 3,
                "Incorrect number of files in folder before folder modification"
            );
            Assert.IsTrue(
                PathUtils.GetAssetCountInFolder(skinPath) == 3,
                "Skinned folder does not have same number of files as original"
            );

            DirectoryInfo directoryInfo = new DirectoryInfo(fullAssetPath);

            var fileToDelete = directoryInfo.GetFiles().FirstOrDefault(info => info.Extension != ".meta");
            fileToDelete.Delete();

            AssetDatabase.Refresh();

            Assert.IsTrue(
                PathUtils.GetAssetCountInFolder(fullAssetPath) == 2,
                "Expected to have 1 less file after deleting one"
            );
            Assert.IsTrue(
                PathUtils.GetAssetCountInFolder(skinPath) == 3,
                "Skinned folder should still have the original file amount"
            );

            var modifiedAssets = UnsavedAssetsManager.GetCurrentModifiedAssetsFromEditorPrefs();
            
            Assert.IsTrue(modifiedAssets.ContainsAssetPath(assetPath), "AssetPath should be in the modified assets list");
            
            ShapeShifterUtils.SavePendingChanges();

            Assert.IsTrue(
                PathUtils.GetAssetCountInFolder(skinPath) == 2,
                "Skinned folder should have 2 files only after overwrite"
            );

            AssetSwitcher.SwitchToGame(TestUtils.game1);

            skinPath = Path.Combine(
                ShapeShifter.ActiveGameSkin.GetAssetSkin(AssetDatabase.AssetPathToGUID(assetPath)).FolderPath,
                TestUtils.FolderAssetName
            );

            Assert.IsTrue(
                PathUtils.GetAssetCountInFolder(fullAssetPath) == 3,
                "Expected to have 3 files after switching to unmodified skinned folder"
            );
            Assert.IsTrue(
                PathUtils.GetAssetCountInFolder(skinPath) == 3,
                "Skinned folder should still have the original file amount"
            );
        }

        [Test]
        public void TestModifyingSpriteMeta()
        {
            Sprite spriteAsset = TestUtils.GetAsset<Sprite>(TestUtils.SpriteAssetName);

            string assetPath = AssetDatabase.GetAssetPath(spriteAsset);

            AssetSkinner.SkinAsset(assetPath);

            AssetSwitcher.SwitchToGame(TestUtils.game0, true);

            TextureImporter textureImporter =
                (TextureImporter)TextureImporter.GetAtPath(assetPath);
            Assert.IsTrue(textureImporter.textureType == TextureImporterType.Sprite);
            Assert.IsFalse(textureImporter.textureType == TextureImporterType.Default);

            textureImporter.textureType = TextureImporterType.Default;
            Assert.IsTrue(textureImporter.textureType == TextureImporterType.Default);
            textureImporter.SaveAndReimport();
            
            var modifiedAssets = UnsavedAssetsManager.GetCurrentModifiedAssetsFromEditorPrefs();
            
            Assert.IsTrue(modifiedAssets.ContainsAssetPath(assetPath), "AssetPath should be in the modified assets list");
            
            ShapeShifterUtils.SavePendingChanges();

            AssetSwitcher.SwitchToGame(TestUtils.game1, true);

            textureImporter =
                (TextureImporter)TextureImporter.GetAtPath(assetPath);
            Assert.IsTrue(textureImporter.textureType == TextureImporterType.Sprite);
            Assert.IsFalse(textureImporter.textureType == TextureImporterType.Default);

            AssetSwitcher.SwitchToGame(TestUtils.game0, true);

            textureImporter =
                (TextureImporter)TextureImporter.GetAtPath(assetPath);
            Assert.IsTrue(textureImporter.textureType == TextureImporterType.Default);
            Assert.IsFalse(textureImporter.textureType == TextureImporterType.Sprite);
        }
    }
}