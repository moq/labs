using System;
using System.Reflection;
using Xunit;

namespace Stunts.Tests
{
    public class StuntFactoryTests
    {
        [Fact]
        public void CreateStuntFromCallingAssemblyWithNamingConvention()
        {
            var stunt = StuntFactory.Default.CreateStunt(
                Assembly.GetExecutingAssembly(), 
                typeof(IStuntFactory), 
                new[] { typeof(IDisposable) }, 
                Array.Empty<object>());

            Assert.IsType<IDisposableIStuntFactoryStunt>(stunt);
        }

        [Fact]
        public void CanReplaceDefaultFactory()
        {
            var existing = StuntFactory.Default;
            var factory = new IDisposableIStuntFactoryStunt();
            StuntFactory.Default = factory;

            Assert.Same(factory, StuntFactory.Default);

            StuntFactory.Default = existing;
        }
    }
}

namespace Stunts
{
    public class IDisposableIStuntFactoryStunt : IStuntFactory, IDisposable
    {
        public object CreateStunt(Assembly stuntsAssembly, Type baseType, Type[] implementedInterfaces, object[] construtorArguments)
            => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();
    }
}
