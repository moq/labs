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

        [Fact]
        public void CanSetupMethodWithArgumentsViaReturns()
        {
            var calculator = Mock.Of<ICalculator>(new DynamicProxyFactory());

            calculator.Add(2, 3).Returns(5);

            var result = calculator.Add(2, 3);

            Assert.Equal(5, result);
        }

        [Fact]
        public void CanSetupMethodWithDifferentArgumentsViaReturns()
        {
            var calculator = Mock.Of<ICalculator>(new DynamicProxyFactory());

            calculator.Add(2, 2).Returns(4);
            calculator.Add(2, 3).Returns(5);
            calculator.Add(10, Arg.Any<int>()).Returns(10);

            Assert.Equal(5, calculator.Add(2, 3));
            Assert.Equal(4, calculator.Add(2, 2));
            Assert.Equal(10, calculator.Add(10, 2));
        }
    }
}