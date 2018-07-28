# Overview

Moq is structured in layers that separate the various responsibilities:

* [Stunts](../src/Stunts): provides the runtime interception/proxy functionality.
* [Moq.Sdk](../src/Moq): provides a common low-level abstraction for setting up conditional proxy behaviors and a full instrospection API for both set ups and invocations
* [Moq](../src/Moq): the Moq API itself, which builds on the previous two and exposes the end-user API

## Stunts

One of the key goals of Moq is to not impose any restrictions on the target runtime to enable its use. One important restriction of previous versions has been the need to perform run-time proxy generation via Reflection.Emit, which is not universally available. So instead of a run-time proxy generation, Moq uses a design-time (or compile-time) code generation strategy instead.

> The **Stunts** name was inspired by the *Test Double* naming in the mock objects literature, which in turn comes from the *Stunt Double* concept from film making. This project allows your objects to pull arbitrary *stunts* at run-time based on your instructions.

The `src/Stunts.sln` solution is the main one for *Stunts*. 

Even though the run-time API can easily be consumed directly to configure behaviors for a proxy class, it's typically consumed by generated proxies instead, and higher-level APIs like [Moq.Sdk](../src/Moq/Moq.Sdk) and indirectly by Moq's API itself. As such, it's a low-level piece of infrastructure that shouldn't be your first stop if you're just contributing to the Moq API itself.

[Read more about Stunts](Stunts.md) design, internals and code generation strategy.

## Moq SDK

A second goal of Moq is to provide a common low-level API that allows rich mocking APIs to be built on top. Moq itself is of course the primary example and driver of the SDK features, but they are not tied to any particular setup or verification API. In particular, it provides method invocation matching/setup and introspection, invocation recording and instrospection, and a general-purpose state bag associated with every mock object for use in custom behaviors.

Between *Stunts* and *Moq SDK*, the intention is that developers exploring new API designs and domain-specific extensions to mocking can quickly get full fledged libraries without having to implement the whole stack, but rather focus on their added value on top of a common core. Ultimately, we'd like to provide reimplementations of all the major mocking APIs on top of this SDK.

The *Moq SDK*, like *Stunts*, is also a `netstandard` library (version 1.4) to ensure broad compatiblity.

[Read more about Moq SDK](MoqSdk.md) design, internals and code generation strategy.