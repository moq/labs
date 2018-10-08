# moq v5

The most popular and friendly mocking framework for .NET

[![CoreBuild Standard](https://img.shields.io/badge/√_corebuild-standard-blue.svg)](http://www.corebuild.io)
[![Build status](https://ci.appveyor.com/api/projects/status/cm308wecf8ekyv33/branch/master?svg=true)](https://ci.appveyor.com/project/MobileEssentials/moq/branch/master)
[![License](https://img.shields.io/github/license/moq/moq.svg)](https://github.com/moq/moq/blob/master/LICENSE)
[![Join the chat at https://gitter.im/moq/moq](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/moq/moq?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Follow on Twitter](https://img.shields.io/twitter/follow/moqthis.svg?style=social&label=Follow)](http://twitter.com/intent/user?screen_name=moqthis)

This repository supports [corebuild](http://www.corebuild.io) for configure/build/test from `msbuild`.

> **IMPORTANT**: this repository is for the *upcoming* version of Moq. Issues and source for the current stable Moq v4.x are at https://github.com/moq/moq4

## Building the repository

```
msbuild /t:configure
msbuild
```

The default target is `Help`, which will render the documentation for the build itself and what targets are available. Since this is a [corebuild](http://www.corebuild.io) standard repository, you can run:

```
msbuild /t:configure
msbuild /t:build
msbuild /t:test
```

## Dependencies

The packaging projects are [NuGetizer](https://github.com/NuGet/NuGet.Build.Packaging/) projects, and require the [VS2017 extension](http://bit.ly/nugetizer-2017) to load in Visual Studio. The extension is not necessary to build the packages from MSBuild, however.
