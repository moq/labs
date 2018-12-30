using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

            Assert.IsType<IStuntFactoryIDisposableStunt>(stunt);
        }

        [Fact]
        public void CanReplaceDefaultFactory()
        {
            var existing = StuntFactory.Default;
            var factory = new IStuntFactoryIDisposableStunt();
            StuntFactory.Default = factory;

            Assert.Same(factory, StuntFactory.Default);

            StuntFactory.Default = existing;
        }

    }
}

namespace Stunts
{
    public class IStuntFactoryIDisposableStunt : IStuntFactory, IDisposable
    {
        public object CreateStunt(Assembly stuntsAssembly, Type baseType, Type[] implementedInterfaces, object[] construtorArguments)
            => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();
    }
}
