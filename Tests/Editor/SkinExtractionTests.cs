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
        private string skinnedFolderAssetPath;

        private string assetPathToExtract;
        private string assetPathToExtractGuid;
        private string extractionDestinationPath;

        public override void Setup()
        {
            base.Setup();

            DefaultAsset folderAsset = TestUtils.GetAsset<DefaultAsset>(TestUtils.FolderAssetName);
            skinnedFolderAssetPath = AssetDatabase.GetAssetPath(folderAsset);
            AssetSkinner.SkinAsset(skinnedFolderAssetPath);
            DirectoryInfo folderInfo = new DirectoryInfo(PathUtils.GetFullPath(skinnedFolderAssetPath));
            FileInfo fileToExtract = folderInfo.GetFiles().First(file => !file.Name.Contains(".meta"));

            assetPathToExtract = PathUtils.GetPathRelativeToAssetsFolder(fileToExtract.FullName);
            assetPathToExtractGuid = AssetDatabase.AssetPathToGUID(assetPathToExtract);
            extractionDestinationPath = PathUtils.GetFullPath(TestUtils.TempFolderName);
        }

        [Test]
        public void ExtractAsSkin_WhenAssetExistsInBothGames_CreatesAssetSkinWith2Versions()
        {
            bool success = SkinExtractor.ExtractAsSkin(assetPathToExtract, extractionDestinationPath);

            string newAssetPath = AssetDatabase.GUIDToAssetPath(assetPathToExtractGuid);

            Assert.IsTrue(success);

            Assert.IsTrue(AssetSkinner.IsSkinned(newAssetPath, TestUtils.game0.Name));
            Assert.IsTrue(AssetSkinner.IsSkinned(newAssetPath, TestUtils.game1.Name));

            Assert.IsTrue(!PathUtils.ArePathsEqual(PathUtils.GetFullPath(newAssetPath), assetPathToExtract));
            Assert.IsTrue(
                PathUtils.ArePathsEqual(
                    PathUtils.GetFullPath(newAssetPath),
                    Path.Combine(extractionDestinationPath, Path.GetFileName(assetPathToExtract))
                )
            );
        }

        [Test]
        public void ExtractAsSkin_WhenAssetExistsInOneGame_CreatesAssetSkinWith1Version()
        {
            FileUtils.SafeDelete(PathUtils.GetFullPath(assetPathToExtract));
            AssetDatabase.Refresh();
            ShapeShifterUtils.SavePendingChanges();

            AssetSwitcher.SwitchToGame(
                ShapeShifterConfiguration.Instance.GetGameSkinByName(TestUtils.game1.Name),
                forceSwitch: true
            );

            bool success = SkinExtractor.ExtractAsSkin(assetPathToExtract, extractionDestinationPath);
            AssetDatabase.Refresh();
            string newAssetPath = AssetDatabase.GUIDToAssetPath(assetPathToExtractGuid);

            Assert.IsTrue(success);

            Assert.IsTrue(!AssetSkinner.IsSkinned(newAssetPath, TestUtils.game0.Name));
            Assert.IsTrue(AssetSkinner.IsSkinned(newAssetPath, TestUtils.game1.Name));

            Assert.IsTrue(
                !PathUtils.ArePathsEqual(
                    PathUtils.GetFullPath(newAssetPath),
                    assetPathToExtract
                )
            );
        }

        [Test]
        public void ExtractAsSkin_WhenDestinationIsValid_ExtractedAssetIsMovedToDestination()
        {
            bool success = SkinExtractor.ExtractAsSkin(assetPathToExtract, extractionDestinationPath);

            string newAssetPath = AssetDatabase.GUIDToAssetPath(assetPathToExtractGuid);

            Assert.IsTrue(success);

            Assert.IsTrue(
                PathUtils.ArePathsEqual(
                    PathUtils.GetFullPath(newAssetPath),
                    Path.Combine(extractionDestinationPath, Path.GetFileName(assetPathToExtract))
                )
            );
        }

        [Test]
        public void ExtractAsSkin_WhenTargetFolderIsTheCurrentFolder_ExtractionIsCanceled()
        {
            bool success = SkinExtractor.ExtractAsSkin(
                assetPathToExtract,
                PathUtils.GetFullPath(skinnedFolderAssetPath)
            );

            string newAssetPath = AssetDatabase.GUIDToAssetPath(assetPathToExtractGuid);

            Assert.IsFalse(success);
            Assert.IsFalse(AssetSkinner.IsSkinned(assetPathToExtract));
            Assert.IsTrue(newAssetPath == assetPathToExtract);
        }
    }
}