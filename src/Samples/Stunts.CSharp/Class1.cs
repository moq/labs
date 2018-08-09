using System;
using Sample;
using System.Linq;
using Stunts;
using System.Reflection;

public class Tests
{
    public void WhenFakingCalculatorBase()
    {
        var fake = Stunt.Of<CalculatorBase>();
    }

    public void WhenFakingFormatterThenCanInvokeIt()
    {
        var stunt1 = Stunt.Of<ICustomFormatter, IDisposable>();
        var stunt2 = Stunt.Of<ICustomFormatter>();

        stunt1.AddBehavior(new DefaultValueBehavior());
        stunt1.InsertBehavior(0,
            (m, n) => m.CreateValueReturn(string.Format((string)m.Arguments[0], m.Arguments.Skip(1).ToArray())),
            m => m.MethodBase.Name == nameof(ICustomFormatter.Format),
            "Format");

        stunt2.AddBehavior(new DefaultValueBehavior());

        var result1 = stunt1.Format("Hello {0}", "World", null);
        var result2 = stunt2.Format("Hello {0}", "World", null);

        Console.WriteLine(result1); 
        Console.WriteLine(result2);

        var bar = Stunt.Of<IBar>();
    }

    public void when_using_custom_generator()
    {
        var ping = Randomizer.Of<IPing>();

        Console.WriteLine(ping.Ping());
        Console.WriteLine(ping.Ping());
    }
}

public interface IPing
{
    int Ping();
}

public static class Randomizer
{
    static readonly Random random = new Random();

    [StuntGenerator]
    public static T Of<T>()
        => Stunt.Of<T>()
                .AddBehavior(
                    (invocation, next) => invocation.CreateValueReturn(random.Next()), 
                    invocation => invocation.MethodBase is MethodInfo info && info.ReturnType == typeof(int));
}



namespace Sample
{
    public interface IBar
    {
        void Bar();
    }

    public interface IFoo
    {
        void Do();

        int Bar();
    }
}
