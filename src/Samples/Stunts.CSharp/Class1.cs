using System;
using Sample;
using System.Linq;
using Stunts;

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

        var foo = Stunt.Of<IBar>();
    }
}

namespace Sample
{
    public interface IBar { }

    public interface IFoo
    {
        void Do();

        int Bar();
    }
}
