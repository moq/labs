using System;
using System.Reflection;
using Moq;
using Moq.Proxy;

namespace Sample.CSharp
{
    public class Class1
    {
        public void Test()
        {
            var mock = Mock.Of<ICustomFormatter, IDisposable>();
            var foo = Mock.Of<IFoo>();
            var bar = Mock.Of<IBar>();

            var value = "foo";

            foo.Id
                .Callback(() => value = "before")
                .Returns(() => value)
                .Callback(() => value = "after")
                .Returns(() => value);

            Console.WriteLine(foo.Id);
            Console.WriteLine(foo.Id);
        }
    }
}

public interface IBar
{
}

public interface IFoo
{
    string Id { get; }
    string Title { get; set; }
}

static class Mock
{
    [ProxyGenerator]
    public static T Of<T, T1>() => Moq.Mock.Of<T>(typeof(Mock).GetTypeInfo().Assembly, typeof(T1));

    [ProxyGenerator]
    public static T Of<T>() => Moq.Mock.Of<T>(typeof(Mock).GetTypeInfo().Assembly);
}
