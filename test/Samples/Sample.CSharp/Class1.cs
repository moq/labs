using System;
using System.Reflection;
using Moq.Proxy;

namespace Sample.CSharp
{
    public class Class1
    {
        public void Test()
        {
            var mock = Mock.Of<ICustomFormatter, IDisposable>();

        }
    }
}

static class Mock
{
    [ProxyGenerator]
    public static T Of<T, T1>() => Moq.Mock.Of<T>(typeof(Mock).GetTypeInfo().Assembly, typeof(T1));

    [ProxyGenerator]
    public static T Of<T>() => Moq.Mock.Of<T>(typeof(Mock).GetTypeInfo().Assembly);
}
