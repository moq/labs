using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Stunts;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockBehaviorTests
    {
        [Fact]
        public void RecordsInvocation()
        {
            var behavior = new MockTrackingBehavior();
            var mock = new Mocked();

            behavior.Invoke(new MethodInvocation(mock, typeof(object).GetMethod(nameof(object.ToString))),
                () => (m, n) => m.CreateValueReturn(null));

            Assert.Equal(1, mock.Mock.Invocations.Count);
        }

        [Fact]
        public void ThrowsForNonIMocked()
        {
            var behavior = new MockTrackingBehavior();

            Assert.Throws<ArgumentException>(() => behavior.Invoke(new MethodInvocation(
                new object(),
                typeof(Mocked).GetProperty(nameof(IMocked.Mock)).GetGetMethod()),
                null));
        }

        [Fact]
        public void WhenAddingMockBehavior_ThenCanInterceptSelectively()
        {
            var calculator = (ICalculator)MockFactory.Default.CreateMock(Assembly.GetExecutingAssembly(), typeof(ICalculator), new Type[0], new object[0]);

            calculator.AddBehavior((m, n) => m.CreateValueReturn(CalculatorMode.Scientific), m => m.MethodBase.Name == "get_Mode");
            calculator.AddBehavior(new DefaultValueBehavior());
            calculator.AddBehavior(new DefaultEqualityBehavior());

            var mode = calculator.Mode;
            var add = calculator.Add(3, 2);

            Assert.Equal(CalculatorMode.Scientific, mode);
            Assert.Equal(0, add);
        }
    }
}