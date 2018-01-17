using System;
using System.ComponentModel;
using Mocks;
using Moq;
using static Moq.Syntax;
using Moq.Sdk;
using Stunts;

namespace Sample.CSharp
{
    public class Class1
    {
        public void Test()
        {
            var fake = Mock.Of<ICalculator>();

            fake.Memory.Recall().Returns(5);

            //var m = new FooMock();
            //m.AddBehavior(new MockTrackingBehavior());
            //m.AddBehavior(new EventBehavior());
            //m.AddBehavior(new PropertyBehavior());
            //m.AddBehavior(new DefaultValueBehavior());
            //m.AddBehavior(new DefaultEqualityBehavior());

            //var name = "";

            //m.PropertyChanged += (sender, args) => name = args.PropertyName;

            //m.PropertyChanged += Raise<PropertyChangedEventHandler>(new PropertyChangedEventArgs("Foo"));

            //m.GetFormat(Any<Type>()).Returns(new object());

            var mock = Mock.Of<CalculatorBase, ICustomFormatter, IDisposable>();

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