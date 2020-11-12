#pragma warning disable CS0436
using Moq.StaticProxy.UnitTests;
using Xunit;

namespace Moq.Scenarios.RecursiveSetupMethods
{
    public class Test : IRunnable
    {
        //[Fact]
        public void RunScenario() => new StaticProxy.UnitTests.Scenarios().Run(ThisAssembly.Constants.Scenarios.RecursiveSetupMethods);

        void IRunnable.Run()
        {
            var mock = Mock.Of<IFoo>();

            mock.Setup(x => x.GetBar().GetBaz().Name).Returns("hi");

            Assert.Equal("hi", mock.GetBar().GetBaz().Name);
        }
    }

    public interface IFoo
    {
        IBar GetBar();
    }

    public interface IBar
    {
        IBaz GetBaz();
    }

    public interface IBaz
    {
        string Name { get; }
    }
}