#pragma warning disable CS0436
using Moq.StaticProxy.UnitTests;
using Xunit;

namespace Moq.Scenarios.RecursiveProperties
{
    public class Test : IRunnable
    {
        //[Fact]
        public void RunScenario() => new StaticProxy.UnitTests.Scenarios().Run(ThisAssembly.Constants.Scenarios.RecursiveProperties);

        void IRunnable.Run()
        {
            var mock = Mock.Of<IFoo>();

            using (mock.Setup())
            {
                mock.Bar.Baz.Name.Returns("hi");
            }

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