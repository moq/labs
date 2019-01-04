using System;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class StrictMockBehaviorTests
    {
        [Fact]
        public void AppliesToAllInvocations() 
            => Assert.True(new StrictMockBehavior().AppliesTo(new FakeInvocation()));

        [Fact]
        public void ThrowsStrictMockException()
            => Assert.Throws<StrictMockException>(() =>
                new StrictMockBehavior().Execute(new FakeInvocation(), () => throw new NotImplementedException()));

        [Fact]
        public void ThrowsIfNullInvocation()
            => Assert.Throws<ArgumentNullException>(() =>
                new StrictMockBehavior().Execute(null, () => throw new NotImplementedException()));

        [Fact]
        public void DoesNotThrowIfSetupScopeActive()
        {
            using (new SetupScope())
            {
                new StrictMockBehavior().Execute(new FakeInvocation(), () => (m, n) => m.CreateValueReturn(null));
            }
        }
    }
}
