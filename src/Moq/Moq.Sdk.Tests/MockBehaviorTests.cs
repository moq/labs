using System;
using Sample;
using Stunts;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockBehaviorTests
    {
        [Fact]
        public void CreatesBehaviorWithNullDisplayName()
            => Assert.Equal("<unnamed>", new DelegateMockBehavior((m, i, n) => n().Invoke(m, i, n), default(string)).ToString());

        [Fact]
        public void CreatesBehaviorWithDisplayName()
            => Assert.Equal("test", new DelegateMockBehavior((m, i, n) => n().Invoke(m, i, n), "test").ToString());

        [Fact]
        public void CreatesBehaviorWithLazyDisplayName()
            => Assert.Equal("test", new DelegateMockBehavior((m, i, n) => n().Invoke(m, i, n), new Lazy<string>(() => "test")).ToString());

        [Fact]
        public void ExecutesAnonymousBehavior()
        {
            var called = false;
            var behavior = new DelegateMockBehavior((m, i, n) => { called = true; return i.CreateValueReturn(null); }, "test");
            var mock = new FakeMock();

            behavior.Execute(mock.Mock, new MethodInvocation(mock, typeof(object).GetMethod(nameof(object.ToString))), () => null);

            Assert.True(called);
        }

        [Fact]
        public void RecordsInvocation()
        {
            var behavior = new MockTrackingBehavior();
            var mock = new Mocked();

            behavior.Execute(new MethodInvocation(mock, typeof(object).GetMethod(nameof(object.ToString))),
                () => (m, n) => m.CreateValueReturn(null));

            Assert.Equal(1, mock.Mock.Invocations.Count);
        }

        [Fact]
        public void ThrowsForNonIMocked()
        {
            var behavior = new MockTrackingBehavior();

            Assert.Throws<ArgumentException>(() => behavior.Execute(new MethodInvocation(
                new object(),
                typeof(Mocked).GetProperty(nameof(IMocked.Mock)).GetGetMethod()),
                null));
        }

        [Fact]
        public void WhenAddingMockBehavior_ThenCanInterceptSelectively()
        {
            var calculator = new CalculatorInterfaceStunt();

            // TODO: this is not adding a mock behavior but a regular stunt behavior
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