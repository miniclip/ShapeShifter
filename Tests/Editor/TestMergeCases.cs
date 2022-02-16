using System.IO;
using Miniclip.ShapeShifter.Utils;
using NUnit.Framework;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class TestMergeCases : TestBase
    {
        [Test]
        public void Test()
        {
            TextAsset textAsset = TestUtils.GetAsset<TextAsset>(TestUtils.TextFileAssetName);

            Assert.IsNotNull(textAsset);

            DirectoryInfo repoDirectoryInfo = new DirectoryInfo(GitUtils.MainRepositoryPath);

            string currentBranch = GitUtils.GetCurrentBranch(repoDirectoryInfo);
            
            Debug.Log("##! " + currentBranch);
            
            
            
            // GitUtils.CreateBranch(repoDirectoryInfo, "hello");
        }
    }
}