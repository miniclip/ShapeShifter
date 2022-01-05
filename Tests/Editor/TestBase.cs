using NUnit.Framework;

namespace Miniclip.ShapeShifter.Tests
{
    public abstract class TestBase
    {
        [SetUp]
        public void Setup()
        {
            TestUtils.Reset();
        }
        
        [TearDown]
        public void Teardown()
        {
            TestUtils.TearDown();
        }
    }
}