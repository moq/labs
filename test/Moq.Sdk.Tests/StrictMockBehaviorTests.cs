using Xunit;

namespace Moq.Sdk.Tests
{
    public class StrictMockBehaviorTests
    {
        [Fact]
        public void ThrowsStrictMockException()
        {
            Assert.Throws<StrictMockException>(() =>
                new StrictMockBehavior().Invoke(null, null));
        }
    }
}
