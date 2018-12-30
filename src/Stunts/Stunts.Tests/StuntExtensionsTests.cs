using System;
using System.Collections.ObjectModel;
using Xunit;

namespace Stunts.Tests
{
    public class StuntExtensionsTests
    {
        [Fact]
        public void WhenAddingStuntBehavior_ThenCanAddLambda()
        {
            var stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);

            Assert.Single(stunt.Behaviors);
        }

        [Fact]
        public void WhenAddingStuntBehavior_ThenCanCustom()
        {
            IStunt stunt = new TestStunt();

            stunt.AddBehavior(new TestStuntBehavior());

            Assert.Single(stunt.Behaviors);
        }

        [Fact]
        public void WhenAddingStuntBehavior_ThenCanAddLambdaWithAppliesTo()
        {
            var stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null, m => true);

            Assert.True(stunt.Behaviors[0].AppliesTo(null));
        }

        [Fact]
        public void WhenAddingStuntBehavior_ThenCanAddLambdaWithAppliesToNamed()
        {
            var stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null, m => true, "true");
            ((IStunt)stunt).AddBehavior((m, n) => null, m => false, "false");

            Assert.Contains("true", stunt.Behaviors[0].ToString());
            Assert.Contains("false", stunt.Behaviors[1].ToString());
        }

        [Fact]
        public void WhenAddingStuntBehavior_ThenCanAddInterface()
        {
            var stunt = new TestStunt();

            stunt.AddBehavior(new TestStuntBehavior());

            Assert.Single(stunt.Behaviors);
        }

        [Fact]
        public void WhenAddingStuntBehaviorToObject_ThenCanAddLambda()
        {
            object stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);

            Assert.Single(((IStunt)stunt).Behaviors);
        }

        [Fact]
        public void WhenAddingStuntBehaviorToObject_ThenCanAddLambdaWithAppliesTo()
        {
            object stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null, m => true);

            Assert.True(((IStunt)stunt).Behaviors[0].AppliesTo(null));
        }

        [Fact]
        public void WhenAddingStuntBehaviorToObject_ThenCanAddLambdaWithAppliesToNamed()
        {
            object stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null, m => true, "true");
            ((IStunt)stunt).AddBehavior((m, n) => null, m => false, "false");

            Assert.Contains("true", ((IStunt)stunt).Behaviors[0].ToString());
            Assert.Contains("false", ((IStunt)stunt).Behaviors[1].ToString());
        }

        [Fact]
        public void WhenAddingStuntBehaviorToObject_ThenCanAddInterface()
        {
            object stunt = new TestStunt();

            stunt.AddBehavior(new TestStuntBehavior());

            Assert.Single(((IStunt)stunt).Behaviors);
        }

        [Fact]
        public void WhenInsertingStuntBehavior_ThenCanAddLambda()
        {
            var stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, (m, n) => throw new NotImplementedException());

            Assert.Equal(2, stunt.Behaviors.Count);
            Assert.Throws<NotImplementedException>(() => stunt.Behaviors[0].Execute(null, null));
        }

        [Fact]
        public void WhenInsertingStuntBehavior_ThenCanCustom()
        {
            IStunt stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, new TestStuntBehavior());

            Assert.Equal(2, stunt.Behaviors.Count);
            Assert.IsType<TestStuntBehavior>(stunt.Behaviors[0]);
        }

        [Fact]
        public void WhenInsertingStuntBehavior_ThenCanAddLambdaWithAppliesTo()
        {
            var stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, (m, n) => throw new NotImplementedException(), m => true);
            stunt.InsertBehavior(0, (m, n) => throw new ArgumentException(), m => false);

            Assert.Equal(3, stunt.Behaviors.Count);
            Assert.False(stunt.Behaviors[0].AppliesTo(null));
            Assert.True(stunt.Behaviors[1].AppliesTo(null));
            Assert.Throws<NotImplementedException>(() => stunt.Behaviors[1].Execute(null, null));
        }

        [Fact]
        public void WhenInsertingStuntBehavior_ThenCanAddLambdaWithAppliesToNamed()
        {
            var stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, (m, n) => throw new NotImplementedException(), m => true, "true");
            ((IStunt)stunt).InsertBehavior(0, (m, n) => throw new ArgumentException(), m => false, "false");

            Assert.Equal(3, stunt.Behaviors.Count);
            Assert.Contains("false", stunt.Behaviors[0].ToString());
            Assert.Contains("true", stunt.Behaviors[1].ToString());
        }

        [Fact]
        public void WhenInsertingStuntBehavior_ThenCanAddInterface()
        {
            var stunt = new TestStunt();
            var behavior = new TestStuntBehavior();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, behavior);

            Assert.Equal(2, stunt.Behaviors.Count);
            Assert.Same(behavior, stunt.Behaviors[0]);
        }

        [Fact]
        public void WhenInsertingStuntBehaviorToObject_ThenCanAddLambda()
        {
            object stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, (m, n) => throw new NotImplementedException());

            Assert.Equal(2, ((IStunt)stunt).Behaviors.Count);
            Assert.Throws<NotImplementedException>(() => ((IStunt)stunt).Behaviors[0].Execute(null, null));
        }

        [Fact]
        public void WhenInsertingStuntBehaviorToObject_ThenCanAddLambdaWithAppliesTo()
        {
            object stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, (m, n) => throw new NotImplementedException(), m => true);
            stunt.InsertBehavior(0, (m, n) => throw new ArgumentException(), m => false);

            Assert.Equal(3, ((IStunt)stunt).Behaviors.Count);
            Assert.False(((IStunt)stunt).Behaviors[0].AppliesTo(null));
            Assert.True(((IStunt)stunt).Behaviors[1].AppliesTo(null));
            Assert.Throws<NotImplementedException>(() => ((IStunt)stunt).Behaviors[1].Execute(null, null));
        }

        [Fact]
        public void WhenInsertingStuntBehaviorToObject_ThenCanAddLambdaWithAppliesToNamed()
        {
            object stunt = new TestStunt();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, (m, n) => throw new NotImplementedException(), m => true, "true");
            ((IStunt)stunt).InsertBehavior(0, (m, n) => throw new ArgumentException(), m => false, "false");

            Assert.Equal(3, ((IStunt)stunt).Behaviors.Count);
            Assert.Contains("false", ((IStunt)stunt).Behaviors[0].ToString());
            Assert.Contains("true", ((IStunt)stunt).Behaviors[1].ToString());
        }

        [Fact]
        public void WhenInsertingStuntBehaviorToObject_ThenCanAddInterface()
        {
            object stunt = new TestStunt();
            var behavior = new TestStuntBehavior();

            stunt.AddBehavior((m, n) => null);
            stunt.InsertBehavior(0, behavior);

            Assert.Equal(2, ((IStunt)stunt).Behaviors.Count);
            Assert.Same(behavior, ((IStunt)stunt).Behaviors[0]);
        }

        [Fact]
        public void WhenAddingStuntBehaviorToObjectWithLambda_ThenThrowsIfNotStunt() =>
            Assert.Throws<ArgumentException>(() => new object().AddBehavior((m, n) => null));

        [Fact]
        public void WhenAddingStuntBehaviorToObjectWithInterface_ThenThrowsIfNotStunt() =>
            Assert.Throws<ArgumentException>(() => new object().AddBehavior(new TestStuntBehavior()));

        [Fact]
        public void WhenInsertingStuntBehaviorToObjectWithLambda_ThenThrowsIfNotStunt() =>
            Assert.Throws<ArgumentException>(() => new object().InsertBehavior(0, (m, n) => null));

        [Fact]
        public void WhenInsertingStuntBehaviorToObjectWithInterface_ThenThrowsIfNotStunt() =>
            Assert.Throws<ArgumentException>(() => new object().InsertBehavior(0, new TestStuntBehavior()));

        class TestStuntBehavior : IStuntBehavior
        {
            public bool AppliesTo(IMethodInvocation invocation) => true;

            public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next) => null;
        }

        class TestStunt : IStunt
        {
            public ObservableCollection<IStuntBehavior> Behaviors { get; } = new ObservableCollection<IStuntBehavior>();
        }
    }
}
