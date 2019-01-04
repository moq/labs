using System;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockNamingTests
    {
        [Fact]
        public void SortsImplementedInterfaces()
            => Assert.Equal("MockNamingTestsIFormattableIServiceProviderMock",
                MockNaming.GetName(typeof(MockNamingTests), new[] { typeof(IServiceProvider), typeof(IFormattable) }));
    }
}
