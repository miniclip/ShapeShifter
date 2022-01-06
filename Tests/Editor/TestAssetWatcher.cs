using System.IO;
using Miniclip.ShapeShifter.Utils;
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

            ShapeShifter.SkinAsset(assetPath);

            Assert.IsTrue(
                !ShapeShifter.Configuration.ModifiedAssetPaths.Contains(assetPath),
                "Asset was just skinned now, it should not be in the modified assets list"
            );

            File.WriteAllText(fullAssetPath, ShapeShifter.ActiveGameSkin.Name);

            AssetDatabase.Refresh();

            Assert.IsTrue(
                ShapeShifter.Configuration.ModifiedAssetPaths.Contains(assetPath),
                "Asset was modified and it is not showing in modified assets list"
            );
        }

        [Test]
        public void TestRenamingAsset()
        {
            TextAsset textAsset = TestUtils.GetAsset<TextAsset>(TestUtils.TextFileAssetName);
            string assetPathBeforeRename = AssetDatabase.GetAssetPath(textAsset);
            string guid = AssetDatabase.AssetPathToGUID(assetPathBeforeRename);
            string fullAssetPathBeforeRename = PathUtils.GetFullPath(assetPathBeforeRename);

            ShapeShifter.SkinAsset(assetPathBeforeRename);

            Assert.IsTrue(ShapeShifter.IsSkinned(assetPathBeforeRename));

            string assetIgnoredPathBeforeRename =
                GitUtils.GetIgnoredPathByGUID(AssetDatabase.AssetPathToGUID(assetPathBeforeRename));
            Assert.IsTrue(
                fullAssetPathBeforeRename == PathUtils.GetFullPath(assetIgnoredPathBeforeRename),
                "Asset path in .gitignore is not correct"
            );

            AssetDatabase.RenameAsset(assetPathBeforeRename, "newname");
            AssetDatabase.Refresh();

            string assetPathAfterRename = AssetDatabase.GUIDToAssetPath(guid);
            string fullAssetPathAfterRename = PathUtils.GetFullPath(assetPathAfterRename);
            Assert.IsTrue(assetPathBeforeRename != assetPathAfterRename, "New asset path should be different by now");

            Assert.IsTrue(
                ShapeShifter.Configuration.ModifiedAssetPaths.Contains(assetPathBeforeRename),
                "Asset was modified and it is not showing in modified assets list"
            );

            string assetIgnorePathAfterRename =
                GitUtils.GetIgnoredPathByGUID(AssetDatabase.AssetPathToGUID(assetPathBeforeRename));

            Assert.IsTrue(fullAssetPathAfterRename == PathUtils.GetFullPath(assetIgnorePathAfterRename));
        }
    }
}