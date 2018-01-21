using System;
using Moq;

namespace Sample.CSharp
{
    public class Class1
    {
        public void Test()
        {
            var mock = Mock.Of<CalculatorBase, ICustomFormatter, IDisposable>();

            mock.Setup(m => m.Memory.Recall()).Returns(1);

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

        public void Test1()
        {
            var fake = Mock.Of<ICalculator>();

            fake.Memory.Recall().Returns(5);
        }

        public void Test2()
        {
            var fake = Mock.Of<ICalculator>();

            fake.Memory.Recall().Returns(5);
        }
    }
}

public interface IBar
{
    void DoBar();
}

public interface IFoo
{
    void Do(bool donow);

    string Id { get; }
    string Title { get; set; }
}