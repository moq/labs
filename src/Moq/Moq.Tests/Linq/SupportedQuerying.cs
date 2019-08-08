using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Moq.Sdk;
using Sample;
using Stunts;
using Xunit;
using static Moq.Syntax;

namespace Moq.Tests.Linq
{
    // TODO merge with QueryableMocksFixture
    public class SupportedQuerying
    {
        [Fact]
        public void GivenABooleanProperty_WhenImplicitlyQueryingTrueOneOf_ThenSetsPropertyToTrue()
        {
            var target = Mock.Of<ICalculator>(x => x.IsOn);

            Assert.True(target.IsOn);
        }


        [Fact]
        public void GivenABooleanProperty_WhenExplicitlyQueryingTrueOneOf_ThenSetsPropertyToTrue()
        {
            var target = Mock.Of<ICalculator>(x => x.IsOn == true);

            Assert.True(target.IsOn);
        }


        [Fact]
        public void GivenABooleanProperty_WhenQueryingWithFalse_ThenSetsProperty()
        {
            var target = Mock.Of<FooDefaultIsValid>(x => x.IsValid == false);

            Assert.False(target.IsValid);
        }

        [Fact]
        public void GivenABooleanProperty_WhenQueryingTrueEquals_ThenSetsProperty()
        {
            var target = Mock.Of<ICalculator>(x => true == x.IsOn);

            Assert.True(target.IsOn);
        }

        [Fact]
        public void GivenABooleanProperty_WhenQueryingFalseEquals_ThenSetsProperty()
        {
            var target = Mock.Of<FooDefaultIsValid>(x => false == x.IsValid);

            Assert.False(target.IsValid);
        }

        [Fact]
        public void GivenABooleanProperty_WhenQueryingNegatedProperty_ThenSetsProperty()
        {
            var target = Mock.Of<FooDefaultIsValid>(x => !x.IsValid);

            Assert.False(target.IsValid);
        }

        [Obsolete]
        public class FooDefaultIsValid : IFoo2
        {
            public FooDefaultIsValid()
            {
                this.IsValid = true;
            }

            public virtual bool IsValid { get; set; }

            public virtual string Value { get; set; }

            public virtual string Do(int value)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void GivenTwoProperties_WhenCombiningQueryingWithImplicitBoolean_ThenSetsBothProperties()
        {
            var target = Mock.Of<ICalculator>(c => c.IsOn && c.Mode == CalculatorMode.Scientific);

            Assert.True(target.IsOn);
            Assert.Equal(CalculatorMode.Scientific, target.Mode);
        }

        [Fact]
        public void GivenTwoProperties_WhenCombiningQueryingWithExplicitBoolean_ThenSetsBothProperties()
        {
            var target = Mock.Of<ICalculator>(c => c.IsOn == true && c.Mode == CalculatorMode.Scientific);

            Assert.True(target.IsOn);
            Assert.Equal(CalculatorMode.Scientific, target.Mode);
        }

        [Fact]
        public void GivenAMethodWithOneParameter_WhenUsingSpecificArgumentValue_ThenSetsReturnValue()
        {
            var foo = Mock.Of<IFoo2>(x => x.Do(5) == "foo");

            Assert.Equal("foo", foo.Do(5));
        }

        [Fact]
        public void GivenAMethodWithOneParameter_WhenUsingItIsAnyForArgument_ThenSetsReturnValue()
        {
            var foo = Mock.Of<IFoo2>(x => x.Do(Any<int>()) == "foo");

            Assert.Equal("foo", foo.Do(5));
        }

        [Fact]
        public void GivenAMethodWithOneParameter_WhenUsingItIsForArgument_ThenSetsReturnValue()
        {
            var foo = Mock.Of<IFoo2>(x => x.Do(Any<int>(i => i > 0)) == "foo");

            Assert.Equal("foo", foo.Do(5));
            Assert.Equal(default(string), foo.Do(-5));
        }

        [Fact]
        public void GivenAMethodWithOneParameter_WhenUsingCustomMatcherForArgument_ThenSetsReturnValue()
        {
            var foo = Mock.Of<IFoo2>(x => x.Do(Any<int>()) == "foo");

            Assert.Equal("foo", foo.Do(5));
        }

        [Fact]
        public void GivenAReadonlyProperty_WhenQueryingByProperties_ThenSetsThemDirectly()
        {
            var foo = Mock.Of<Foo2>(x => x.Id == 1);

            Assert.Equal(1, foo.Id);
        }

        [Obsolete]
        public class Foo2
        {
            public virtual int Id { get { return 0; } }
        }

        [Obsolete]
        public interface IFoo2
        {
            bool IsValid { get; set; }
            string Value { get; set; }
            string Do(int value);
        }
    }
}

namespace Mocks
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Obsolete]
    public partial class IFoo2Mock : Moq.Tests.Linq.SupportedQuerying.IFoo2, IStunt, IMocked
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly BehaviorPipeline pipeline = new BehaviorPipeline();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [CompilerGenerated]
        ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

        #region IMocked
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IMock mock;

        [DebuggerDisplay("Invocations = {Invocations.Count}", Name = nameof(IMocked.Mock))]
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [CompilerGenerated]
        IMock IMocked.Mock => LazyInitializer.EnsureInitialized(ref mock, () => new DefaultMock(this));
        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string DebuggerDisplay => ((IMocked)this).Mock.State.TryGetValue<string>("Name", out var name) ? name : GetType().Name;

        [CompilerGenerated]
        public bool IsValid { get => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public string Value { get => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public string Do(int value) => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Obsolete]
    public partial class FooDefaultIsValidMock : Moq.Tests.Linq.SupportedQuerying.FooDefaultIsValid, IStunt, IMocked
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly BehaviorPipeline pipeline = new BehaviorPipeline();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [CompilerGenerated]
        ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

        #region IMocked
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IMock mock;

        [DebuggerDisplay("Invocations = {Invocations.Count}", Name = nameof(IMocked.Mock))]
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [CompilerGenerated]
        IMock IMocked.Mock => LazyInitializer.EnsureInitialized(ref mock, () => new DefaultMock(this));
        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string DebuggerDisplay => ((IMocked)this).Mock.State.TryGetValue<string>("Name", out var name) ? name : GetType().Name;

        [CompilerGenerated]
        public override bool IsValid { get => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public override string Value { get => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Obsolete]
    public partial class Foo2Mock : Moq.Tests.Linq.SupportedQuerying.Foo2, IStunt, IMocked
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly BehaviorPipeline pipeline = new BehaviorPipeline();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [CompilerGenerated]
        ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

        #region IMocked
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IMock mock;

        [DebuggerDisplay("Invocations = {Invocations.Count}", Name = nameof(IMocked.Mock))]
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        [CompilerGenerated]
        IMock IMocked.Mock => LazyInitializer.EnsureInitialized(ref mock, () => new DefaultMock(this));
        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string DebuggerDisplay => ((IMocked)this).Mock.State.TryGetValue<string>("Name", out var name) ? name : GetType().Name;

        [CompilerGenerated]
        public override int Id { get => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); }
    }
}