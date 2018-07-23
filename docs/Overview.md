# Overview

Moq is structured in layers that separate the various responsibilities:

* [Stunts](../src/Stunts): provides the runtime interception/proxy functionality.
* [Moq.Sdk](../src/Moq): provides a common low-level abstraction for setting up conditional proxy behaviors and a full instrospection API for both set ups and invocations
* [Moq](../src/Moq): the Moq API itself, which builds on the previous two and exposes the end-user API

## Stunts

One of the key goals of Moq is to not impose any restrictions on the target runtime to enable its use. One key restriction of previous versions has been the need to perform run-time proxy generation via Reflection.Emit, which is not universally available. So instead of a run-time proxy generation, Moq uses a design-time (or compile-time) code generation strategy instead.

> The **Stunts** name was inspired by the *Test Double* naming in the mock objects literature, which in turn comes from the *Stunt Double* concept from film making. This project allows your objects to pull arbitrary *stunts* at run-time based on your instructions.

The `src/Stunts.sln` solution is the main one for *Stunts*. 

Even though the run-time API can easily be consumed directly to configure behaviors for a proxy class, it's typically consumed by generated proxies instead, and higher-level APIs like [Moq.Sdk](../src/Moq/Moq.Sdk) and indirectly by Moq's API itself. As such, it's a low-level piece of infrastructure that shouldn't be your first stop if you're just contributing to Moq API itself.

[Read more about Stunts](Stunts.md) design, internals and code generation strategy.