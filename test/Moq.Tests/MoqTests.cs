using System;
using System.ComponentModel;
using Xunit;
using Moq.Sdk;
using Moq.Proxy;
using static Moq.Syntax;

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
            calculator.PoweringUp += handler;

            calculator.PoweringUp += Raise.Event();

            Assert.True(raised);

            raised = false;
            calculator.PoweringUp -= handler;
            calculator.PoweringUp -= handler;

            calculator.PoweringUp += Raise();

            Assert.False(raised);

            var property = "";
            calculator.PropertyChanged += (sender, args) => property = args.PropertyName;

            calculator.PropertyChanged += Raise<PropertyChangedEventHandler>(new PropertyChangedEventArgs("Mode"));

            Assert.Equal("Mode", property);

            var progress = 0;
            calculator.Progress += (_, i) => progress = i;

            calculator.Progress += Raise(10);

            Assert.Equal(10, progress);
        }

        [Fact]
        public void CanSetupPropertyViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();
            
            calculator.Mode.Returns("Basic");

            var mode = calculator.Mode;
            
            Assert.Equal("Basic", mode);
        }

        [Fact]
        public void CanSetupPropertyTwiceViaReturns()
        {
            var calculator = Mock.Of<ICalculator>();

            calculator.Mode.Returns("Basic");
            calculator.Mode.Returns("Advanced");

            var mode = calculator.Mode;

            Assert.Equal("Advanced", mode);
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
                .Callback(() => called1 = true)
                .Callback(() => called2 = true)
                .Returns((int x, int y) => x + y);

            calculator.Add(2, 2);

            Assert.True(called1);
            Assert.True(called2);
        }

        [Fact]
        public void CannotInvokeCallbackAfterReturn()
        {
            var calculator = Mock.Of<ICalculator>();
            var called = false;

            calculator.Add(Any<int>(), Any<int>())
                .Returns((int x, int y) => x + y)
                // The reason this can't work is that Returns does not keep 
                // invoking the next handler in the behavior pipeline, and 
                // therefore short-circuits the behaviors at this point.
                .Callback(() => called = true);

            calculator.Add(2, 2);

            Assert.False(called);
        }
    }
}