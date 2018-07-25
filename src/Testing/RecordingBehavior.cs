using System;
using System.Collections.Generic;
using System.Linq;
using Stunts;

public class RecordingBehavior : IStuntBehavior
{
    List<object> invocations = new List<object>();

    public bool AppliesTo(IMethodInvocation invocation) => true;

    public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior next)
    {
        var result = next().Invoke(invocation, next);
        if (result != null)
            invocations.Add(result);
        else
            invocations.Add(invocation);

        return result;
    }

    public override string ToString() => string.Join(Environment.NewLine, invocations.Select(i => i.ToString()));
}