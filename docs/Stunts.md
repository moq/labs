# Stunts

As explained in the [Overview](Overview.md), *Stunts* provides a low-level API to configure a set of behaviors that apply to a manual or generated proxy. In addition to the run-time library dependency, it also provides analyzers and code fixes that allow automated design-time (or compile-time) generation of proxies that can be configured at run-time.

> The **Stunts** name was inspired by the [Test Double](http://xunitpatterns.com/Test%20Double.html) name which in turn comes from the *Stunt Double* concept from film making. This project allows your objects to pull arbitrary *stunts* at run-time based on your instructions.

*Stunts* consist of two parts: a run-time API and optional code generation.

## Stunts API

The core [Stunts](../src/Stunts/Stunts) API is the only run-time dependency an app or library using stunts needs. It's a `netstandard1.3` library that provides the basic interception implemented as a [BehaviorPipeline](../src/Stunts/Stunts/BehaviorPipeline.cs). The implementation is a simple chain of responsibility that invokes all configured behaviors that apply to the current invocation. The API is intentionally designed to be intuitive to use even if you write your proxies (called *stunts* in this context) manually, since its usefulnes can potentially go well beyond mocking and testing.

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

![Stunts Overloads](https://raw.githubusercontent.com/moq/moq/docs/docs/img/Stunts_Extensions.png)

The behavior pipeline is a sorted list of behaviors that may apply to a given invocation. There are two ways of expressing behaviors: as a pair of delegates or implementations of [IStuntBehavior](../src/Stunts/Stunts/IStuntBehavior.cs). The former are called *anonymous behaviors*. In the example above, we created an anonymous behavior that applies to all calls (we didn't provide a value for the optional `appliesTo` delegate parameter used for filtering) and throws an exception. 

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
    IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior next);
}
```

The design of the behavior pipeline involves a full stack-preserving chain of responsibility, achieved via the `GetNextBehavior` delegate, which you can optionally invoke from your behavior to get the next applicable behavior and invoke it. In this way, each behavior adds to the call stack, making it easier to troubleshoot when something doesn't work as expected, since you can see all behaviors that applied to an invocation right in the call stack.

Unlike Castle DynamicProxy interception API, the `IMethodInvocation` is immutable, which simplifies state management and eliminates possible concurrency issues. Any behavior can nevertheless modify the `invocation` passed to the next behavior delegate by just creating a new `MethodInvocation` with different values. To short-circuit the call, we can simply return from our behavior by calling either `CreateExceptionReturn` or `CreateValueReturn` on the received invocation.

All of the supported .NET calling conventions should be supported by *Stunts*, as shown in (Stunts-Example.md).

## Stunts Code Generation

As shown in the previous section, stunts are nothing special. You can just create new instances of them and invoke their members as usual. 

Obviously, it's no fun to have to keep in sync yet another implementation of your interfaces/abstract classes, so *Stunts* provides two main mechanisms powered by the Roslyn analyzer and code fix infrastructure to make this straightforward:

### Custom Stunt

A custom stunt is basically what was shown above. You just create a class anywhere in your project, add base class and/or interfaces you want to implement, as well as the `IStunt` interface, and a new code fix will be offered to implement it entirely for you:

![Stunts CodeFix](https://raw.githubusercontent.com/moq/moq/docs/docs/img/Stunts_CodeFix.png)

Whenever you change the interface or abstract class the stunt implements or inherits, you will get the typical compilation error about missing members. Stunts will augment the built-in available code fixes if the class implements `IStunt` and offer to implement the missing member through the behavior pipeline, like the original code fix did for the initial scaffold:

![Missing Member CodeFix](https://raw.githubusercontent.com/moq/moq/docs/docs/img/Stunts_CodeFixMember.png)

This allows to quickly update the stunts to match the updated interfaces or abstract class members.

> NOTE: custom stunts are limited to one class per file

### Stunt Factory

The second approach to code generation, more automated and probably familiar, is triggered whenever the stunt factory method `Stunt.Of<T>` is invoked:

```csharp
var cloneable = Stunt.Of<ICloneable>();
```

The `Of<T>` method has several overloads to also specify a base class and multiple interfaces as needed:

```csharp
var cloneable = Stunt.Of<MyBase, ICloneable, IDisposable>();
```

An analyzer will offer to implement the missing stunt in this case:

![Missing Member CodeFix](https://raw.githubusercontent.com/moq/moq/docs/docs/img/Stunts_FactoryCodeFix.png)

The factory-driven generated stunts will (by convention) end up in the `Stunts` folder and namespace. 

Just like in custom stunts, if an a generated stunt becomes outdated, you will also be offered to update/fix it right from the factory method invocation:

![Factory Update CodeFix](https://raw.githubusercontent.com/moq/moq/docs/docs/img/Stunts_FactoryUpdateCodeFix.png)

### Build Time

We intend to provide built-time stunt code generation for the initial stable release. It will leverage the stunt factory approach, but automatically generate or update stunts in an intermediate folder (i.e. obj\Debug) instead of the user's project.

## Extensibility

*Stunts* supports two main extensibility scenarios that allow its usage in specialized situations while exposing a similarly intuitive programming model for consumers:

* Custom factory methods: a very simple mechanism that allows generation of custom stunts that are pre-configured with custom behaviors, requiring no knowledge of analyzers or code fixes authoring.
* Custom code generators: full code generation customization via custom analyzers and code fixes that can augment the default stunts code generation, change naming conventions, etc. 

### Custom Factory Method

This straightforward mechanism allows you to provide a custom factory method that sets up default behaviors on the created stunts. Basically any static generic method annotated with the `StuntGenerator` is treated exactly like `Stunt.Of`. In fact, the [Stunts](../src/Stunts/Stunts/contentFiles/cs/netstandard1.3/Stunt.cs) uses that exact mechanism to trigger the analyzer and corresponding code fix suggestions.

Suppose you want to provide stunts that automatically randomize returned values. You would create your own static class and factory method as follows:

```csharp
public static class Randomizer
{
    [StuntGenerator]
    public static T Of<T>()
        => Stunt.Of<T>()
            // call .AddBehavior to configure default behaviors
            ;
}
```

With that in place, you can now invoke your own stunt factory method, which will trigger the same behavior as `Stunt.Of` did:

```csharp
var ping = Randomizer.Of<IPing>();
```

For example, for design-time code generation:

![Custom Factory CodeFix](https://raw.githubusercontent.com/moq/moq/docs/docs/img/Stunts_CustomFactoryCodeFix.png)

A naive implementation that randomized only integers could be:

```csharp
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
```

Of course, your factory method can receive arbitrary parameters to configure those same behaviors you add. This is exactly how `Mock.Of<T>(MockBehavior behavior)` is implemented.

## Custom Code Generators

*Stunts* provides an SDK that allows plugging in custom code generation when stunts are generated.

This approach involves inheriting from various classes in the SDK, creating custom analyzers and code actions, etc. The best way to learn about this is to explore the [Moq.Analyzer](../src/Moq/Moq.Analyzer) project.

// TODO: add documentation.