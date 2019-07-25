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
        public void CallBaseNotCalled()
        {
            var mock = Mock.Of<Calculator>();

            mock.TurnOn();

            Assert.False(mock.TurnOnCalled);
        }

        [Fact]
        public void CallBaseCalledForMockConfig()
        {
            var mock = Mock.Of<Calculator>().CallBase();

            mock.TurnOn();

            Assert.True(mock.TurnOnCalled);
        }

        [Fact]
        public void CallBaseCalledForInvocationConfig()
        {
            var mock = Mock.Of<Calculator>();

            mock.Setup(x => x.TurnOn()).CallBase();

            mock.TurnOn();

            Assert.True(mock.TurnOnCalled);
        }

        [Fact]
        public void ThrowsForStrictMockAndMissingSetup()
        {
            // Configure CallBase at the Mock level
            var mock = Mock.Of<Calculator>(MockBehavior.Strict).CallBase();

            Assert.Throws<StrictMockException>(() => mock.TurnOn());
        }

        [Fact]
        public void CallBaseCalledForStrictMockAndMockConfig()
        {
            // Configure CallBase at the Mock level
            var mock = Mock.Of<Calculator>(MockBehavior.Strict).CallBase();

            mock.Setup(x => x.TurnOn());

            mock.TurnOn();

            Assert.True(mock.TurnOnCalled);

            // And we make sure we throw for other missing setups
            Assert.Throws<StrictMockException>(() => mock.Recall(""));
        }

        [Fact]
        public void CallBaseCalledForStrickMockAndInvocationConfig()
        {
            var mock = Mock.Of<Calculator>(MockBehavior.Strict);

            // Configure CallBase at the invocation level
            mock.Setup(x => x.TurnOn()).CallBase();

            mock.TurnOn();

            Assert.True(mock.TurnOnCalled);

            // And we make sure we throw for other missing setups
            Assert.Throws<StrictMockException>(() => mock.Recall(""));
        }
    }
}