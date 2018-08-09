using System;
using System.Collections.ObjectModel;
using System.Reflection;

using Stunts;

public class MyBase { }

public class MyClassTest
{
    public void Configure()
    {
        // var instance = new MyClass();

        var cloneable = Stunt.Of<ICloneable>();

    }
}

public class MyClass : IDisposable, IStunt
{
    private readonly BehaviorPipeline pipeline = new BehaviorPipeline();

    ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

    public void Dispose() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
}

//[CompilerGenerated]
//public override bool Equals(object obj) => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), obj));
//[CompilerGenerated]
//public override int GetHashCode() => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
//[CompilerGenerated]
//public override string ToString() => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
