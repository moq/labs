using System;
using System.Collections.Generic;
using System.Linq;
using Stunts;

public class RecordingBehavior : IStuntBehavior
{
    public List<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();

    public bool AppliesTo(IMethodInvocation invocation) => true;

    public IMethodReturn Execute(IMethodInvocation invocation, GetNextBehavior next)
    {
        Invocations.Add(invocation);
        return next().Invoke(invocation, next);
    }

    public override string ToString() => string.Join(Environment.NewLine, Invocations.Select(i => i.ToString()));
}