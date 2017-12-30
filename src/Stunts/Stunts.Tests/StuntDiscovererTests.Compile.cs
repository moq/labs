using System;
using Stunts;

public class Tests
{
    public void WhenFakingFormatterThenCanInvokeIt()
    {
        var stunt1 = Stunt.Of<ICustomFormatter>();
        var stunt2 = Stunt.Of<ICustomFormatter>();

        var result1 = stunt1.Format("Hello {0}", "World", null);
        var result2 = stunt2.Format("Hello {0}", "World", null);
    }
}