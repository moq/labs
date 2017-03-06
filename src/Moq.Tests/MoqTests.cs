using Moq.Proxy.Dynamic;
using Xunit;

namespace Moq.Tests
{
    public class MoqTests
    {
        [Fact]
        public void CanSetupPropertyViaReturns()
        {
            var calculator = Mock.Of<ICalculator>(new DynamicProxyFactory());

            calculator.Mode.Returns("Basic");

            var mode = calculator.Mode;

            Assert.Equal("Basic", mode);
        }
    }
}