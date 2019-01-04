using System;
using Xunit;
using Stunts;
using System.Reflection;
using Moq.Sdk.Tests;

namespace Moq.Sdk.Tests
{
    public class MockFactoryTests
    {
        [Fact]
        public void CanCreateMockFromFactory()
        {
            var mock = MockFactory.Default.CreateMock<IServiceProvider>(Assembly.GetExecutingAssembly());

            Assert.NotNull(mock);
            Assert.IsAssignableFrom<IStunt>(mock);
        }

        [Fact]
        public void CanReplaceMockFactory()
        {
            var factory = MockFactory.Default;
            try
            {
                MockFactory.Default = new MyMockFactory();
            }
            finally
            {
                MockFactory.Default = factory;
            }
        }

        class MyMockFactory : IMockFactory
        {
            public object CreateMock(Assembly mocksAssembly, Type baseType, Type[] implementedInterfaces, object[] construtorArguments)
                => throw new NotImplementedException();
        }
    }
}

namespace Mocks
{
    public class IServiceProviderMock : FakeMock, IServiceProvider
    {
        public object GetService(Type serviceType)
            => Pipeline.Execute<object>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), serviceType));
    }
}
