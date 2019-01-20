using System.IO;

partial class ModuleInitializer
{
    static partial void OnRun()
    {
        foreach (var msbuild in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "Microsoft.Build.*", SearchOption.TopDirectoryOnly))
        {
            try
            {
                File.Delete(msbuild);
            }
            catch { }
        }
    }
}