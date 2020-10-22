using Xunit;

namespace Moq.Sdk.Tests
{
    public class SetupScopeTests
    {
        [Fact]
        public void LifeCycle()
        {
            using (new SetupScope())
            {
                Assert.True(SetupScope.IsActive);
            }

            Assert.False(SetupScope.IsActive);
        }
    }
}
