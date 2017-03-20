using System;
using System.Collections.Generic;
using Moq.Proxy;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class MockExtensionsTests
    {
        [Fact]
        public void WhenAddingMockBehavior_ThenCanAddLambda()
        {
            var mock = new TestMock();

            mock.AddMockBehavior(m => true, (m, n) => null);

            Assert.Equal(1, mock.Mock.Behaviors.Count);
        }

        [Fact]
        public void WhenAddingMockBehavior_ThenCanAddInterface()
        {
            var mock = new TestMock();

            mock.AddMockBehavior(new TestMockBehavior());

            Assert.Equal(1, mock.Mock.Behaviors.Count);
        }

        [Fact]
        public void WhenAddingMockBehaviorToObject_ThenCanAddLambda()
        {
            object mock = new TestMock();

            mock.AddMockBehavior(m => true, (m, n) => null);

            Assert.Equal(1, ((IMocked)mock).Mock.Behaviors.Count);
        }

        [Fact]
        public void WhenAddingMockBehaviorToObject_ThenCanAddInterface()
        {
            object mock = new TestMock();

            mock.AddMockBehavior(new TestMockBehavior());

            Assert.Equal(1, ((IMocked)mock).Mock.Behaviors.Count);
        }

        [Fact]
        public void WhenInsertingMockBehavior_ThenCanAddLambda()
        {
            var mock = new TestMock();

            mock.AddMockBehavior(m => true, (m, n) => null);
            mock.InsertMockBehavior(0, m => true, (m, n) => throw new NotImplementedException());

            Assert.Equal(2, mock.Mock.Behaviors.Count);
            Assert.Throws<NotImplementedException>(() => mock.Mock.Behaviors[0].Invoke(null, null));
        }

        [Fact]
        public void WhenInsertingMockBehavior_ThenCanAddInterface()
        {
            var mock = new TestMock();
            var behavior = new TestMockBehavior();

            mock.AddMockBehavior(m => true, (m, n) => null);
            mock.InsertMockBehavior(0, behavior);

            Assert.Equal(2, mock.Mock.Behaviors.Count);
            Assert.Same(behavior, mock.Mock.Behaviors[0]);
        }

        [Fact]
        public void WhenInsertingMockBehaviorToObject_ThenCanAddLambda()
        {
            object mock = new TestMock();

            mock.AddMockBehavior(m => true, (m, n) => null);
            mock.InsertMockBehavior(0, m => true, (m, n) => throw new NotImplementedException());

            Assert.Equal(2, ((IMocked)mock).Mock.Behaviors.Count);
            Assert.Throws<NotImplementedException>(() => ((IMocked)mock).Mock.Behaviors[0].Invoke(null, null));
        }

        [Fact]
        public void WhenInsertingMockBehaviorToObject_ThenCanAddInterface()
        {
            object mock = new TestMock();
            var behavior = new TestMockBehavior();

            mock.AddMockBehavior(m => true, (m, n) => null);
            mock.InsertMockBehavior(0, behavior);

            Assert.Equal(2, ((IMocked)mock).Mock.Behaviors.Count);
            Assert.Same(behavior, ((IMocked)mock).Mock.Behaviors[0]);
        }

        [Fact]
        public void WhenAddingMockBehaviorToObjectWithLambda_ThenThrowsIfNotMock() =>
            Assert.Throws<ArgumentException>(() => new object().AddMockBehavior(m => true, (m, n) => null));

        [Fact]
        public void WhenAddingMockBehaviorToObjectWithInterface_ThenThrowsIfNotMock() =>
            Assert.Throws<ArgumentException>(() => new object().AddMockBehavior(new TestMockBehavior()));

        [Fact]
        public void WhenInsertingMockBehaviorToObjectWithLambda_ThenThrowsIfNotMock() =>
            Assert.Throws<ArgumentException>(() => new object().InsertMockBehavior(0, m => true, (m, n) => null));

        [Fact]
        public void WhenInsertingMockBehaviorToObjectWithInterface_ThenThrowsIfNotMock() =>
            Assert.Throws<ArgumentException>(() => new object().InsertMockBehavior(0, new TestMockBehavior()));

        class TestMockBehavior : IMockBehavior
        {
            public bool AppliesTo(IMethodInvocation invocation) => false;

            public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext) => null;
        }

        class Mock : IMock
        {
            public IList<IMockBehavior> Behaviors { get; } = new List<IMockBehavior>();

            public IList<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();
        }

        class TestMock : IMocked
        {
            public IMock Mock { get; } = new Mock();
        }
    }
}
