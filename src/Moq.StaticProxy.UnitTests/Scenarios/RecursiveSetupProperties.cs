#pragma warning disable CS0436
using Xunit;

namespace Moq.Scenarios.RecursiveSetupProperties
{
    public class Test : IRunnable
    {
        public void Run()
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