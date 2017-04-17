using System.Reflection;
using Moq.Proxy;

static class Mock
{
    [ProxyGenerator]
    public static T Of<T>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly());
}