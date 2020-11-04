#pragma warning disable CS0436
using Xunit;
using Moq.StaticProxy.UnitTests;
using System;

namespace Moq.Scenarios.MultipleUses
{
    /// <summary>
    /// Multiple uses of the avatar factory method only result in one 
    /// such type being generated (IWO, no compilation errors because 
    /// of duplicate types.
    /// </summary>
    public class Test : IRunnable
    {
        //[Fact]
        public void RunScenario() => new StaticProxy.UnitTests.Scenarios().Run(ThisAssembly.Constants.Scenarios.MultipleUses);

        void IRunnable.Run()
        {
            var disposable = Mock.Of<IDisposable>();
            var services = Mock.Of<IServiceProvider>();

            Assert.NotNull(disposable);
            Assert.NotNull(services);

            Do();
        }
        
        public void Do()
        {
            var disposable2 = Mock.Of<IDisposable>();
            var services2 = Mock.Of<IServiceProvider>();

            Assert.NotNull(disposable2);
            Assert.NotNull(services2);
        }
    }
}
