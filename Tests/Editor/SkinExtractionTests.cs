using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;

namespace Miniclip.ShapeShifter.Tests
{
    public class SkinExtractionTests : TestBase
    {
        [Test]
        public void ExtractAsSkin_WhenAssetExistsInBothGames_CreatesAssetSkinWith2Versions()
        {
            DefaultAsset folderAsset = TestUtils.GetAsset<DefaultAsset>(TestUtils.FolderAssetName);

            string assetPath = AssetDatabase.GetAssetPath(folderAsset);
            AssetSkinner.SkinAsset(assetPath);

            AssetDatabase.Refresh();

            DirectoryInfo folderInfo = new DirectoryInfo(PathUtils.GetFullPath(assetPath));

            FileInfo fileToExtract = folderInfo.GetFiles().First(file => !file.Name.Contains(".meta"));

            string assetPathToExtract = PathUtils.GetPathRelativeToAssetsFolder(fileToExtract.FullName);

            string guid = AssetDatabase.AssetPathToGUID(assetPathToExtract);

            Assert.IsTrue(!AssetSkinner.IsSkinned(assetPathToExtract));
            Assert.IsTrue(
                AssetSkinner.TryGetParentSkinnedFolder(assetPathToExtract, out string skinnedParentFolderPath)
            );

            string extractionDestinationPath = PathUtils.GetFullPath(TestUtils.TempFolderName);

            SkinExtractor.ExtractAsSkin(assetPathToExtract, extractionDestinationPath);

            string newAssetPath = AssetDatabase.GUIDToAssetPath(guid);

            Assert.IsTrue(newAssetPath != assetPathToExtract);

            Assert.IsTrue(AssetSkinner.IsSkinned(newAssetPath, TestUtils.game0.Name));
            Assert.IsTrue(AssetSkinner.IsSkinned(newAssetPath, TestUtils.game1.Name));
        }

        [Test]
        public void ExtractAsSkin_WhenAssetExistsInOneGame_CreatesAssetSkinWith1Version()
        {
            DefaultAsset folderAsset = TestUtils.GetAsset<DefaultAsset>(TestUtils.FolderAssetName);

            string assetPath = AssetDatabase.GetAssetPath(folderAsset);
            AssetSkinner.SkinAsset(assetPath);

            AssetDatabase.Refresh();

            DirectoryInfo folderInfo = new DirectoryInfo(PathUtils.GetFullPath(assetPath));

            FileInfo fileToDelete = folderInfo.GetFiles().First(file => !file.Name.Contains(".meta"));

            fileToDelete.Delete();

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            AssetSwitcher.SwitchToGame(ShapeShifterConfiguration.Instance.GetGameSkinByName(TestUtils.game1.Name));

            FileInfo fileToExtract = folderInfo.GetFiles().First(file => !file.Name.Contains(".meta"));

            Assert.IsTrue(fileToDelete.Name == fileToExtract.Name);

            string assetPathToExtract = PathUtils.GetPathRelativeToAssetsFolder(fileToDelete.FullName);

            string guid = AssetDatabase.AssetPathToGUID(assetPathToExtract);

            Assert.IsTrue(!AssetSkinner.IsSkinned(assetPathToExtract));
            Assert.IsTrue(
                AssetSkinner.TryGetParentSkinnedFolder(assetPathToExtract, out string skinnedParentFolderPath)
            );

            string extractionDestinationPath = PathUtils.GetFullPath(TestUtils.TempFolderName);

            SkinExtractor.ExtractAsSkin(assetPathToExtract, extractionDestinationPath);

            string newAssetPath = AssetDatabase.GUIDToAssetPath(guid);

            Assert.IsTrue(newAssetPath != assetPathToExtract);

            Assert.IsTrue(!AssetSkinner.IsSkinned(newAssetPath, TestUtils.game0.Name));
            Assert.IsTrue(AssetSkinner.IsSkinned(newAssetPath, TestUtils.game1.Name));
        }
    }
}