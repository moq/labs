using System;
using System.ComponentModel;
using Xunit;
using Moq.Sdk;
using static Moq.Syntax;
using Sample;

namespace Moq.Tests
{
    public class CallBaseTests
    {
        [Fact]
        public void callbase1()
        {
            var mock = Mock.Of<Calculator>();

            mock.TurnOn();

            Assert.False(mock.TurnOnCalled);
        }

        [Fact]
        public void callbase2()
        {
            var mock = Mock.Of<Calculator>().CallBase();

            mock.TurnOn();

            Assert.True(mock.TurnOnCalled);
        }

        [Fact]
        public void callbase3()
        {
            var mock = Mock.Of<Calculator>();

            mock.Setup(x => x.TurnOn()).CallBase();

            mock.TurnOn();

            Assert.True(mock.TurnOnCalled);
        }

        [Fact]
        public void callbase4()
        {
            var mock = Mock.Of<Calculator>(MockBehavior.Strict).CallBase();

            Assert.Throws<StrictMockException>(() => mock.TurnOn());
        }

        [Fact]
        public void callbase5()
        {
            var mock = Mock.Of<Calculator>(MockBehavior.Strict).CallBase();

            mock.Setup(x => x.TurnOn());

            mock.TurnOn();

            Assert.True(mock.TurnOnCalled);
            Assert.Throws<StrictMockException>(() => mock.Recall(""));
        }

        [Fact]
        public void callbase6()
        {
            var mock = Mock.Of<Calculator>(MockBehavior.Strict);

            mock.Setup(x => x.TurnOn()).CallBase();

            mock.TurnOn();

            Assert.True(mock.TurnOnCalled);
            Assert.Throws<StrictMockException>(() => mock.Recall(""));
        }
    }
}