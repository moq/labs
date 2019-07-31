# moq v5

The most popular and friendly mocking framework for .NET

[![CoreBuild Standard](https://img.shields.io/badge/√_corebuild-standard-blue.svg)](http://www.corebuild.io)
[![Build Status](https://dev.azure.com/kzu/oss/_apis/build/status/moq?branchName=master)](https://dev.azure.com/kzu/oss/_build/latest?definitionId=20&branchName=master)
[![Tests](https://img.shields.io/azure-devops/tests/kzu/oss/20.svg?compact_message&logo=azure-pipelines)](https://dev.azure.com/kzu/oss/_build/latest?definitionId=20&branchName=master&view=results)
[![License](https://img.shields.io/github/license/moq/moq.svg)](https://github.com/moq/moq/blob/master/LICENSE)
[![Join the chat at https://gitter.im/moq/moq](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/moq/moq?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Follow on Twitter](https://img.shields.io/twitter/follow/moqthis.svg?style=social&label=Follow)](http://twitter.com/intent/user?screen_name=moqthis)

This repository supports [corebuild](http://www.corebuild.io) for configure/build/test from `msbuild`.

> **IMPORTANT**: this repository is for the *upcoming* version of Moq. Issues and source for the current stable Moq v4.x are at https://github.com/moq/moq4

CI package feed: http://kzu.nuget.cloud/index.json (CDN) or https://kzu.blob.core.windows.net/nuget/index.json (blob).

## Building the repository

```
msbuild /t:configure
msbuild
```

The default target is `Help`, which will render the documentation for the build itself and what targets are available. 
Since this is a [corebuild](http://www.corebuild.io) standard repository, you can run:

```
msbuild /t:configure
msbuild /t:build
msbuild /t:test
```

## Testing built packages locally

Release builds will produce packages. In Debug builds, you will need to right-click and `Pack` the relevant project, 
such as `Moq.Package` (which will also pack its dependencies like `Stunts.Package`). These packages will be dropped 
in the `out` folder in the repository root directory. To test these packages you can just add a package source 
pointing to it. You can also just place a `NuGet.Config` like the following anywhere above the directory with the 
test solution(s):

```xml
<configuration>
	<packageSources>
		<add key="moq" value="[cloned repo dir]\out" />
  </packageSources>
</configuration>
```

Every time the packages are produced, the local nuget cache is cleared, so that a subsequent restore in VS will 
automatically cause the updated version to be unpacked again. If versions change between package builds, you can 
just reference them with a wildcard so the latest will automatically be chosen, such as:

```xml
<ItemGroup>
  <PackageReference Include="Moq" Version="5.0.0-alpha.*"/>
</ItemGroup>
```
