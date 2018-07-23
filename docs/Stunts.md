# Stunts

As explained in the [Overview](Overview.md), *Stunts* provides a low-level API to configure a set of behaviors that apply to a manual or generated proxy. In addition to the run-time library dependency, it also provides analyzers and code fixes that allow automated design-time (or compile-time) generation of proxies that can be configured at run-time.

> The **Stunts** name was inspired by the [Test Double](http://xunitpatterns.com/Test%20Double.html) name which in turn comes from the *Stunt Double* concept from film making. This project allows your objects to pull arbitrary *stunts* at run-time based on your instructions.

The following sections explain each area in detail.

## Stunts API

The core [Stunts](../src/Stunts/Stunts) API is the only run-time dependency an app or library using stunts has. It's a `netstandard1.3` library that provides the basic interception implemented as a [BehaviorPipeline](../src/Stunts/Stunts/BehaviorPipeline.cs). The implementation is a simple chain of responsibility that invokes all configured behaviors that apply to the current invocation. The API is intentionally designed to be intuitive to use even if you write proxies manually, since its usefulnes can potentially go well beyond mocking and testing.

A typical stunt is implemented as follows: 

```csharp
public class MyProxy : IStunt
{
    readonly BehaviorPipeline pipeline = new BehaviorPipeline();

    ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;
}
```

The explicit implementation of the `IStunt` interface avoids cluttering your API. With that in place, you can dynamically configure the behavior of the class:

```csharp
using Stunts;

public class Program
{
    public static void Main()
    {
        var instance = new MyProxy();
        instance.AddBehavior((invocation, next) => invocation.CreateExceptionReturn(new NotImplementedException()));
        // do something
    }
}
```

There are `AddBehavior` and `InsertBehavior` extension method overloads provided in the `Stunts` namespace:

![CoreBuild Standard](docs/img/Stunts_Extensions.png)

The behavior pipeline is a sorted list of behaviors that may apply to a given invocation. There are two ways of expressing behaviors: as a pair of delegates or implementations of [IStuntBehavior](../src/Stunts/Stunts/IStuntBehavior.cs). The former are called *anonymous behaviors*. In the example above, we created an anonymous behavior that applies to all calls (we didn't provide an `AppliesTo` delegate for filtering) and throws an exception. 

So far, we just configured a behavior, but nothing calls into the behavior pipeline, so it will never be run. Let's add an `IDisposable` interface and implement it by invoking the pipeline:

```csharp
public class MyClass : IStunt, IDisposable
{
    private readonly BehaviorPipeline pipeline = new BehaviorPipeline();

    ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

    public void Dispose() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
}
```

Note how the `Dispose` method simply calls into the pipeline, passing in an implementation of [IMethodInvocation](../src/Stunts/Stunts/IMethodInvocation.cs), which is then passed to each behavior that matches the call. We could now extend the behavior we had to only apply to the `Dispose` method:

```csharp
    instance.AddBehavior(
        (invocation, next) => invocation.CreateExceptionReturn(new NotImplementedException()), 
        invocation => invocation.MethodBase.Name == nameof(IDisposable.Dispose));
```

The signatures of those two delegates for the anonymous behavior match the `IStuntBehavior` interface methods:

```csharp
public interface IStuntBehavior
{
    bool AppliesTo(IMethodInvocation invocation);
    IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext);
}
```

The design of the behavior pipeline involves a full stack-preserving chain of responsibility, achieved via the `GetNextBehavior` delegate, which you can optionally invoke from your behavior to get the next applicable behavior and invoke it. In this way, each behavior adds to the stack, making it easier to troubleshoot when something doesn't work as expected, since you can see all behaviors that applied to an invocation right in the call stack.

Unlike Castle DynamicProxy interception API, the `IMethodInvocation` is immutable, which simplifies state management and eliminates possible concurrency issues. Any behavior can nevertheless modify the `invocation` passed to the next behavior delegate by just creating a new `MethodInvocation` with different values. To short-circuit the call, we can simply return from our behavior by calling either `CreateExceptionReturn` or `CreateValueReturn`

All of the supported .NET calling conventions should be supported by *Stunts*, as shown in the next section.

### Full Example

Given the following interface that exercises most (all?) calling conventions in a .NET interface:

```csharp
public interface ICalculator
{
    event EventHandler TurnedOn;
    
    bool IsOn { get; }
    
    CalculatorMode Mode { get; set; }
    
    int Add(int x, int y);

    bool TryAdd(ref int x, ref int y, out int result);
    
    void TurnOn();

    int? this[string name] { get; set; }

    void Store(string name, int value);

    int? Recall(string name);

    void Clear(string name);
}
```

The interface could be implemented as a stunt using the behavior pipeline as follows:

```csharp
public class ICalculatorStunt : ICalculator, IStunt
{
    readonly BehaviorPipeline pipeline = new BehaviorPipeline();

    ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;

    public int? this[string name] 
    {
        get => pipeline.Execute<int?>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name)); 
        set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name, value)); 
    }

    public bool IsOn => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));

    public CalculatorMode Mode 
    {
        get => pipeline.Execute<CalculatorMode>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); 
        set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); 
    }

    public int Add(int x, int y) => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y));

    public int Add(int x, int y, int z) => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y, z));

    public void Clear(string name) => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name));

    public int? Recall(string name) => pipeline.Execute<int?>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name));

    public void Store(string name, int value) => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name, value));

    public bool TryAdd(ref int x, ref int y, out int result)
    {
        result = default(int);
        var returns = pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y, result));
        x = (int)returns.Outputs["x"];
        y = (int)returns.Outputs["y"];
        result = (int)returns.Outputs["result"];
        return (bool)returns.ReturnValue;
    }

    public void TurnOn() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));

    public event EventHandler TurnedOn 
    {
        add => pipeline.Execute<EventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); 
        remove => pipeline.Execute<EventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); 
    }
}
```

This is exactly the code that is generated by the *Stunt* analyzer+codefix, so you typically don't need to write this by hand.