# moq v5

The most popular and friendly mocking framework for .NET

[![Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/Moq/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json)
[![Status](https://github.com/moq/moq/workflows/build/badge.svg?branch=main)](https://github.com/moq/moq/actions?query=branch%3Amain+workflow%3Abuild+)
[![License](https://img.shields.io/github/license/moq/moq.svg)](https://github.com/moq/moq/blob/master/LICENSE)
[![Discord Chat](https://img.shields.io/badge/chat-on%20discord-7289DA.svg)](https://discord.gg/8PtpGdu)
[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/moq/moq)


> **IMPORTANT**: this repository is for the *upcoming* version of Moq. Issues and source for the current stable Moq v4.x are at https://github.com/moq/moq4

CI package feed: https://pkg.kzu.io/index.json

## Building the repository

```
dotnet msbuild
```

Running tests:

```
dotnet test
```

## Testing built packages locally

You can either build from command line or explicitly Pack (from the context menu) the *Moq.Package* project.

Packages are generated in the `bin` folder in the repository root. To test these packages you can just add a package source 
pointing to it. You can also just place a `NuGet.Config` like the following anywhere above the directory with the 
test solution(s):

```xml
<configuration>
	<packageSources>
		<add key="moq" value="[cloned repo dir]\bin" />
  </packageSources>
</configuration>
```

You can also do use project properties (or a *Directory.Build.props* to affect an entire folder hierarchy) with:

```xml
<Project>
  <PropertyGroup>
    <RestoreSources>https://api.nuget.org/v3/index.json;$(RestoreSources)</RestoreSources>
    <RestoreSources Condition="Exists('[cloned repo dir]\bin')">[cloned repo dir]\bin;$(RestoreSources)</RestoreSources>
  </PropertyGroup>
<Project>
```

Every time the packages are produced, the local nuget cache is cleared, so that a subsequent restore in VS will 
automatically cause the updated version to be unpacked again. The locally built version will always have the version [42.42.42](https://en.wikipedia.org/wiki/42_(number)#The_Hitchhiker's_Guide_to_the_Galaxy).
