using NUnit.Framework;

namespace Miniclip.ShapeShifter.Tests
{
    public abstract class TestBase
    {
        [SetUp]
        public virtual void Setup()
        {
            TestUtils.Reset();
        }
        
        [TearDown]
        public virtual void Teardown()
        {
            TestUtils.TearDown();
        }
    }
}