using System;
using System.Linq;
/// <summary>
/// Used by the InjectModuleInitializer. All code inside the Run method is ran as soon as the assembly is loaded.
/// </summary>
internal static partial class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    internal static void Run()
    {
        if (!AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "Microsoft.Build"))
        {
#pragma warning disable 0436
            Microsoft.Build.Locator.MSBuildLocator.RegisterMSBuildPath(ThisAssembly.Metadata.MSBuildBinPath);
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", ThisAssembly.Metadata.MSBuildBinPath, EnvironmentVariableTarget.Process);
#pragma warning restore 0436
        }

        OnRun();
	}

    /// <summary>
    /// Declare in another partial class to extend what is run when the assembly is 
    /// loaded.
    /// </summary>
    static partial void OnRun();
}