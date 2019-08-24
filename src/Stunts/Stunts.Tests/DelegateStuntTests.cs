using System;
using System.ComponentModel;
using Moq.Sdk;
using Xunit;

namespace Stunts.Tests
{
    public class DelegateStuntTests
    {
        [Fact]
        public void CanCreateDelegateStunt() => Stunt.Of<EventHandler>();

        [Fact]
        public void CanVerifyLooseMockDelegateWithNoReturnValue()
        {
            var recorder = new RecordingBehavior();
            var stunt = Stunt.Of<Action<int>>();
            stunt.AddBehavior(recorder);
            stunt.AddBehavior((invocation, next) => invocation.CreateValueReturn(null, invocation.Arguments));

            stunt(3);

            Assert.Single(recorder.Invocations);
        }
    }
}
