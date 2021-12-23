using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Miniclip.ShapeShifter.Tests
{
    public class PathUtilsTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void TestPathOperations()
        {
            var basePath = Directory.GetCurrentDirectory();
            Debug.Log(basePath);
        }
    }
}