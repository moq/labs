To more quickly iterate on the extension, after a first "normal" deploy, you can 
instead run the following from the command line:

For Visual Studio 2017:

```
msbuild /t:QuickDeploy /p:TargetFramework=net462 && devenv /updateConfiguration /rootSuffix Moq && devenv /rootSuffix Moq
```