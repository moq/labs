using System;
using System.ComponentModel;
using Xunit;
using Moq.Sdk;
using static Moq.Syntax;
using Stunts;

namespace Moq.Tests
{
    public class MoqTests
    {
        [Fact]
        public void CanRaiseEvents()
        {
            var calculator = Mock.Of<ICalculator>();
            calculator.InsertBehavior(0, new EventBehavior());

            var raised = false;

            EventHandler handler = (sender, args) => raised = true;
            calculator.TurnedOn += handler;

            calculator.TurnedOn += Raise.Event();

            Assert.True(raised);

            raised = false;
            calculator.TurnedOn -= handler;
            calculator.TurnedOn -= handler;

            calculator.TurnedOn += Raise();

            Assert.False(raised);
        }

        [Fact]
        public void CanRaiseEventsWithArgs()
        {
            var mock = Mock.Of<INotifyPropertyChanged>();
            mock.InsertBehavior(0, new EventBehavior());

            var property = "";
            mock.PropertyChanged += (sender, args) => property = args.PropertyName;

            mock.PropertyChanged += Raise<PropertyChangedEventHandler>(new PropertyChangedEventArgs("Mode"));

            Assert.Equal("Mode", property);
        }

        [Fact]
        public void CanSetupPropertyViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Mode.Returns(CalculatorMode.Standard);

            var mode = calculator.Mode;
            
            Assert.Equal(CalculatorMode.Standard, mode);
        }

        [Fact]
        public void CanSetupPropertyDirectly()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Mode = CalculatorMode.Scientific;

            var mode = calculator.Mode;

            Assert.Equal(CalculatorMode.Scientific, mode);
        }

        [Fact]
        public void CanSetupPropertyTwiceViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Mode.Returns(CalculatorMode.Standard);
            calculator.Mode.Returns(CalculatorMode.Scientific);

            var mode = calculator.Mode;

            Assert.Equal(CalculatorMode.Scientific, mode);
        }

        [Fact]
        public void CanSetupMethodWithArgumentsViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Add(2, 3).Returns(5);

            var result = calculator.Add(2, 3);

            Assert.Equal(5, result);
        }

        [Fact]
        public void CanSetupMethodWithDifferentArgumentsViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Add(2, 2).Returns(4);
            calculator.Add(2, 3).Returns(5);

            calculator.Add(10, Any<int>()).Returns(10);

            calculator.Add(Any<int>(i => i > 20), Any<int>()).Returns(20);

            Assert.Equal(5, calculator.Add(2, 3));
            Assert.Equal(4, calculator.Add(2, 2));
            Assert.Equal(10, calculator.Add(10, 2));
            Assert.Equal(20, calculator.Add(25, 20));
        }

        [Fact]
        public void CanReturnFunction()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Add(2, 2).Returns(() => 4);

            Assert.Equal(4, calculator.Add(2, 2));
        }

        [Fact]
        public void CanReturnFunctionWithArgs()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Add(Any<int>(), Any<int>()).Returns((int x, int y) => x + y);

            Assert.Equal(4, calculator.Add(2, 2));
            Assert.Equal(5, calculator.Add(2, 3));
        }

        [Fact]
        public void CanInvokeCallback()
        {
            var calculator = Mock.Of<ICalculator>();
            var called = false;

            calculator.Add(Any<int>(), Any<int>())
                .Callback(() => called = true)
                .Returns((int x, int y) => x + y);

            calculator.Add(2, 2);

            Assert.True(called);
        }

        [Fact]
        public void CanInvokeTwoCallbacks()
        {
            var calculator = Mock.Of<ICalculator>();
            var called1 = false;
            var called2 = false;

            calculator.Add(Any<int>(), Any<int>())
                .Callback((int _, int __) => called1 = true)
                .Callback((int _, int __) => called2 = true)
                .Returns((int x, int y) => x + y);

            calculator.Add(2, 2);

            Assert.True(called1);
            Assert.True(called2);
        }

        [Fact]
        public void CanInvokeCallbackAfterReturn()
        {
            var calculator = Mock.Of<ICalculator>();
            var called = false;

            calculator.Add(Any<int>(), Any<int>())
                .Returns((int x, int y) => x + y)
                .Callback(() => called = true);

            calculator.Add(2, 2);

            Assert.True(called);
        }

        [Fact]
        public void ThrowsWithException()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator
                .Add(2, 3)
                .Throws(new ArgumentException());

            Assert.Throws<ArgumentException>(() => calculator.Add(2, 3));
        }

        [Fact]
        public void ThrowsWithExceptionType()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator
                .Add(2, 3)
                .Throws<ArgumentException>();

            Assert.Throws<ArgumentException>(() => calculator.Add(2, 3));
        }

        [Fact]
        public void CanSetupPropertyViaReturnsForStrictMock()
        {
            var calculator = Mock.Of<ICalculator>(MockBehavior.Strict);

            // NOTE: since the mock is strict, we need to tell we're going to set it up
            using (calculator.Setup())
            {
                calculator.Mode.Returns(CalculatorMode.Scientific);
            }

            var mode = calculator.Mode;

            Assert.Equal(CalculatorMode.Scientific, mode);
            Assert.Throws<StrictMockException>(() => calculator.Add(2, 4));
        }

        [Fact]
        public void CanSetupPropertyForStrictMock()
        {
            var calculator = Mock.Of<ICalculator>(MockBehavior.Strict);

            calculator.Setup(c => c.Mode).Returns(CalculatorMode.Scientific);

            var mode = calculator.Mode;

            Assert.Equal(CalculatorMode.Scientific, mode);
            Assert.Throws<StrictMockException>(() => calculator.Add(2, 4));
        }

        [Fact]
        public void CanSetupPropertyWithValueForStrictMock()
        {
            var calculator = Mock.Of<ICalculator>(MockBehavior.Strict);

            calculator.Setup(c => c.Mode = CalculatorMode.Scientific);

            var mode = calculator.Mode;

            Assert.Equal(CalculatorMode.Scientific, mode);
            Assert.Throws<StrictMockException>(() => calculator.IsOn);
        }

        [Fact]
        public void CanSetupVoidMethod()
        {
            var calculator = Mock.Of<ICalculator>(MockBehavior.Strict);

            calculator.Setup(c => c.TurnOn())
                .Throws<InvalidOperationException>();

            Assert.Throws<InvalidOperationException>(() => calculator.TurnOn());
        }

    }
}