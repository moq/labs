#pragma warning disable CS0436
using Xunit;
using Moq.StaticProxy.UnitTests;

namespace Moq.Scenarios.RecursiveSetupProperties
{
    public class Test : IRunnable
    {
        //[Fact]
        public void RunScenario() => new StaticProxy.UnitTests.Scenarios().Run(ThisAssembly.Constants.Scenarios.RecursiveSetupProperties);

        void IRunnable.Run()
        {
            var mock = Mock.Of<IFoo>();

            mock.Setup(x => mock.Bar.Baz.Name).Returns("hi");

            Assert.Equal("hi", mock.Bar.Baz.Name);
        }
    }

    public interface IFoo
    {
        IBar Bar { get; }
    }

    public interface IBar
    {
        IBaz Baz { get; }
    }

    public interface IBaz
    {
        string Name { get; }
    }
}