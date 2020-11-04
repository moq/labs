#pragma warning disable CS0436
using Xunit;
using Moq.StaticProxy.UnitTests;

namespace Moq.Scenarios.RecursiveMethods
{
    public class Test : IRunnable
    {
        //[Fact]
        public void RunScenario() => new StaticProxy.UnitTests.Scenarios().Run(ThisAssembly.Constants.Scenarios.RecursiveMethods);

        void IRunnable.Run()
        {
            var mock = Mock.Of<IFoo>();

            using (mock.Setup())
            {
                mock.GetBar().GetBaz().Name.Returns("hi");
            }

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