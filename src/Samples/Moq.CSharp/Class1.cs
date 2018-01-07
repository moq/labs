using System;
using Moq;
using Moq.Sdk;

namespace Sample.CSharp
{
    public class Class1
    {
        public void Test()
        {
            var fake = Mock.Of<ICalculator>();


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
    void DoBar();
}

public interface IFoo
{
    void Do();

    string Id { get; }
    string Title { get; set; }
}