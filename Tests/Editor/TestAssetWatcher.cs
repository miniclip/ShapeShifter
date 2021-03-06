using System.IO;
using Miniclip.ShapeShifter.Saver;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class TestAssetWatcher : TestBase
    {
        [Test]
        public void TestIfModifyingAssetIsDetected()
        {
            TextAsset textAsset = TestUtils.GetAsset<TextAsset>(TestUtils.TextFileAssetName);
            string assetPath = AssetDatabase.GetAssetPath(textAsset);
            string fullAssetPath = PathUtils.GetFullPath(assetPath);

            AssetSkinner.SkinAsset(assetPath);

            Assert.IsFalse(
                UnsavedAssetsManager.HasUnsavedChanges(),
                "Asset was just skinned now, it should not be in the modified assets list"
            );

            File.WriteAllText(fullAssetPath, ShapeShifter.ActiveGameSkin.Name);

            AssetDatabase.Refresh();

            Assert.IsTrue(
                UnsavedAssetsManager.HasUnsavedChanges(),
                "Asset was modified and it is not showing in modified assets list"
            );
        }

        [Test]
        public void TestRenameSkinnedSprite()
        {
            TestRenameFlow(TestUtils.SpriteAssetName);
        }

        [Test]
        public void TestRenameSkinnedTextFile()
        {
            TestRenameFlow(TestUtils.TextFileAssetName);
        }

        [Test]
        public void TestRenameSkinnedFolder()
        {
            TestRenameFlow(TestUtils.FolderAssetName);
        }

        private static void TestRenameFlow(string nameOfResourceToTest)
        {
            Object textAsset = TestUtils.GetAsset<Object>(nameOfResourceToTest);
            string assetPathBeforeRename = AssetDatabase.GetAssetPath(textAsset);
            string guid = AssetDatabase.AssetPathToGUID(assetPathBeforeRename);
            string fullAssetPathBeforeRename = PathUtils.GetFullPath(assetPathBeforeRename);

            AssetSkinner.SkinAsset(assetPathBeforeRename);

            Assert.IsTrue(AssetSkinner.IsSkinned(assetPathBeforeRename));

            string assetIgnoredPathBeforeRename =
                GitIgnore.GetIgnoredPathByGuid(AssetDatabase.AssetPathToGUID(assetPathBeforeRename));

            Assert.IsTrue(
                PathUtils.ArePathsEqual(fullAssetPathBeforeRename, PathUtils.GetFullPath(assetIgnoredPathBeforeRename)),
                "Asset path in .gitignore is not correct"
            );

            AssetDatabase.RenameAsset(assetPathBeforeRename, "newname");
            AssetDatabase.Refresh();

            string assetPathAfterRename = AssetDatabase.GUIDToAssetPath(guid);
            string fullAssetPathAfterRename = PathUtils.GetFullPath(assetPathAfterRename);
            Assert.IsTrue(assetPathBeforeRename != assetPathAfterRename, "New asset path should be different by now");

            string assetIgnorePathAfterRename =
                GitIgnore.GetIgnoredPathByGuid(guid);

            Assert.IsTrue(PathUtils.ArePathsEqual(fullAssetPathAfterRename, PathUtils.GetFullPath(assetIgnorePathAfterRename)), "Expected path from asset and path on git ignore to be the same");

            foreach (string gameName in ShapeShifterConfiguration.Instance.GameNames)
            {
                GameSkin gameSkin = new GameSkin(gameName);
                AssetSkin assetSkin = gameSkin.GetAssetSkin(guid);

                string skinnedAssetPath = Path.Combine(
                    assetSkin.FolderPath,
                    Path.GetFileName(assetPathAfterRename)
                );

                if (PathUtils.IsDirectory(skinnedAssetPath))
                {
                    Assert.IsTrue(Directory.Exists(skinnedAssetPath), "Renamed Asset doesn't exist");
                }
                else
                {
                    Assert.IsTrue(File.Exists(skinnedAssetPath), "Renamed Asset doesn't exist");
                }

                Assert.IsTrue(File.Exists(skinnedAssetPath + ".meta"));
            }

            AssetDatabase.RenameAsset(assetPathAfterRename, nameOfResourceToTest);
        }
    }
}