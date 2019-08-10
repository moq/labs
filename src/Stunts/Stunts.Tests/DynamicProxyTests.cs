using Sample;
using Stunts.Sdk;
using Xunit;

namespace Stunts.Tests
{
    public class DynamicProxyTests
    {
        [Fact]
        public void CreateInterfaceStunt()
        {
            StuntFactory.Default = new DynamicStuntFactory();

            var calculator = Stunt.Of<ICalculator>();

            calculator.AddBehavior((m, n) => m.CreateValueReturn(42, m.Arguments));

            Assert.Equal(42, calculator.Add(5, 10));
        }
    }
}
