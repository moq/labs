using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Moq.Sdk;
using Moq.Tests.Linq;
using Sample;
using Stunts;
using Xunit;
using static Moq.Syntax;

namespace Moq.Tests.Linq
{
    // TODO rename the class, rename all the tests properly
    // TODO merge with SupportedQuerying
    public class QueryableMocksFixture
    {
        [Fact]
        public void ShouldSupportMultipleSetups()
        {
            var target = Mock.Of<IFoo>(f => f.Name == "Foo" &&
                                            f.Find("1").Baz(Any<string>(s => s.Length > 0)).Value == 99 &&
                                            f.Bar.Id == "25" &&
                                            f.Bar.Ping(Any<string>()) == "ack" &&
                                            f.Bar.Ping("error") == "error" &&
                                            f.Bar.Baz(Any<string>()).Value == 5);

            Assert.Equal("Foo", target.Name);
            Assert.Equal(99, target.Find("1").Baz("asdf").Value);
            Assert.Equal("25", target.Bar.Id);
            Assert.Equal("ack", target.Bar.Ping("blah"));
            Assert.Equal("error", target.Bar.Ping("error"));
            Assert.Equal(5, target.Bar.Baz("foo").Value);
        }

        [Fact]
        public void ShouldSupportEnum()
        {
            var target = Mock.Of<ICalculator>(c => c.Mode == CalculatorMode.Scientific);

            Assert.Equal(CalculatorMode.Scientific, target.Mode);
        }

        [Fact]
        public void ShouldSupportMethod()
        {
            var expected = Mock.Of<IBar>();
            var target = Mock.Of<IFoo>(x => x.Find(Any<string>()) == expected);

            Assert.Equal(expected, target.Find("3"));
        }

        [Fact]
        public void ShouldSupportIndexer()
        {
            var target = Mock.Of<IBaz>(x => x["3", Any<bool>()] == 10);

            Assert.NotEqual(10, target["1", true]);
            Assert.Equal(10, target["3", true]);
            Assert.Equal(10, target["3", false]);
        }

        [Fact]
        public void ShouldSupportBooleanMethod()
        {
            var target = Mock.Of<IBaz>(x => x.HasElements("3"));
            //var target = Mock.Of<ICalculator>(x => x.HasElements("3"));

            Assert.True(target.HasElements("3"));
        }

        [Fact]
        public void ShouldSupportBooleanMethodNegation()
        {
            var target = Mock.Of<IBaz>(x => !x.HasElements("3"));

            Assert.False(target.HasElements("3"));
        }

        [Fact]
        public void ShouldSupportMultipleMethod()
        {
            var target = Mock.Of<IBaz>(x => !x.HasElements("1") && x.HasElements("2"));

            Assert.False(target.HasElements("1"));
            Assert.True(target.HasElements("2"));
        }

        [Fact]
        public void ShouldSupportSettingDtoProtectedVirtualPropertyValue()
        {
            var target = Mock.Of<Dto>(x => x.ProtectedVirtualValue == "foo");

            Assert.Equal("foo", target.ProtectedVirtualValue);
        }

        [Fact]
        public void ShouldAllowFluentOnReadOnlyGetterProperty()
        {
            var target = Mock.Of<IFoo>(x => x.Bars == new[] { Mock.Of<IBar>(b => b.Id == "1"), Mock.Of<IBar>(b => b.Id == "2") });

            Assert.Equal(2, target.Bars.Count());
        }

        [Fact]
        public void ShouldFailIfNotVirtualMethod()
        {
            Assert.Throws<Exception>(() => Mock.Of<Dto>(x => x.Value == "foo")); // TODO change exception by property or method not mockeable

        }
    }

    // TODO refactor properly
    #region Temp

    [Obsolete]
    public class Dto
    {
        public string Value { get; set; }
        public string ProtectedValue { get; protected set; }
        public virtual string ProtectedVirtualValue { get; protected set; }
    }

    [Obsolete]
    public interface IFoo
    {
        IBar Bar { get; set; }
        string Name { get; set; }
        IBar Find(string id);
        AttributeTargets Targets { get; set; }
        IEnumerable<IBar> Bars { get; }
    }

    [Obsolete]
    public interface IBar
    {
        IBaz Baz(string value);
        string Id { get; set; }
        string Ping(string command);
    }

    [Obsolete]
    public interface IBaz
    {
        int Value { get; set; }
        int this[string key1, bool key2] { get; set; }
        bool IsValid { get; set; }
        bool HasElements(string key1);
    }

    #endregion
}

// TODO refactor properly
namespace Mocks
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Obsolete]
    public partial class IFooMock : IFoo, IStunt, IMocked
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
        public IBar Bar { get => pipeline.Execute<IBar>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public string Name { get => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public AttributeTargets Targets { get => pipeline.Execute<AttributeTargets>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public IEnumerable<IBar> Bars => pipeline.Execute<IEnumerable<IBar>>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));

        [CompilerGenerated]
        public IBar Find(string id) => pipeline.Execute<IBar>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), id));
    }

    [Obsolete]
    public partial class DtoMock : Dto, IStunt, IMocked
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
        public override string ProtectedVirtualValue { get => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); protected set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }
    }

    [Obsolete]
    public partial class IBazMock : IBaz, IStunt, IMocked
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

        [CompilerGenerated]
        public int Value { get => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public bool IsValid { get => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public int this[string key1, bool key2] { get => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), key1, key2)); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), key1, key2, value)); }

        [CompilerGenerated]
        public bool HasElements(string key1) => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), key1));

    }

    [Obsolete]
    public partial class IBarMock : IBar, IStunt, IMocked
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

        [CompilerGenerated]
        public string Id { get => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        [CompilerGenerated]
        public IBaz Baz(string value) => pipeline.Execute<IBaz>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));

        [CompilerGenerated]
        public string Ping(string command) => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), command));
    }

}
