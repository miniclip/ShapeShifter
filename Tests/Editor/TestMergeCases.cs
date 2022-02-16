using System.IO;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class TestMergeCases : TestBase
    {
        [Test]
        public void TestMergeUnskinnedWithChangesIntoSkinned()
        {
            TextAsset textAsset = TestUtils.GetAsset<TextAsset>(TestUtils.TextFileAssetName);

            Assert.IsNotNull(textAsset);

            DirectoryInfo repoDirectoryInfo = new DirectoryInfo(GitUtils.MainRepositoryPath);

            string currentBranch = GitUtils.GetCurrentBranch(repoDirectoryInfo);

            string branch_1 = "test/test_merge_cases_1";
            string branch_2 = "test/test_merge_cases_2";

            GitUtils.CreateBranch(repoDirectoryInfo, branch_1);

            Assert.IsTrue(GitUtils.GetCurrentBranch(repoDirectoryInfo) == branch_1);
            StageAndCommitTextFile(textAsset, repoDirectoryInfo, branch_1);

            GitUtils.CreateBranch(repoDirectoryInfo, branch_2);
            Assert.IsTrue(GitUtils.GetCurrentBranch(repoDirectoryInfo) == branch_2);
            AssetSkinner.SkinAsset(textAsset.GetAssetPath());
            GitUtils.Commit(repoDirectoryInfo, branch_2, "Commit skinning of text file", false);

            GitUtils.SwitchBranch(branch_1, repoDirectoryInfo);
            Assert.IsTrue(GitUtils.GetCurrentBranch(repoDirectoryInfo) == branch_1);

            //switch to branch 1

            //do 50+% change on file
        }

        private static void StageAndCommitTextFile(TextAsset textAsset, DirectoryInfo repoDirectoryInfo,
            string branch_1)
        {
            string textAssetPath = AssetDatabase.GetAssetPath(textAsset);
            GitUtils.Stage(textAssetPath);
            GitUtils.Stage(textAssetPath + ".meta");
            GitUtils.Commit(repoDirectoryInfo, branch_1, "Commited text test file", false);
        }
    }
}