using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Moq.Proxy;
using Xunit;

static class TestHelpers
{
    public static (AdhocWorkspace workspace, Project project) CreateWorkspaceAndProject(string language, string assemblyName = "Code")
    {
        var workspace = new AdhocWorkspace(ProxyGenerator.CreateHost());
        var projectInfo = CreateProjectInfo(language, assemblyName);
        var project = workspace.AddProject(projectInfo);

        return (workspace, project);
    }

    public static ProjectInfo CreateProjectInfo(string language, string assemblyName)
    {
        var suffix = language == LanguageNames.CSharp ? "CS" : "VB";
        var options = language == LanguageNames.CSharp ?
                (CompilationOptions)new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) :
                (CompilationOptions)new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionStrict: OptionStrict.On);

        return ProjectInfo.Create(
            ProjectId.CreateNewId(),
            VersionStamp.Create(),
            assemblyName + "." + suffix,
            assemblyName + "." + suffix,
            language,
            compilationOptions: options,
            metadataReferences: ReferencePaths.Paths
                .Select(path => MetadataReference.CreateFromFile(path)));
    }

    public static CancellationToken TimeoutToken(int seconds)
        => Debugger.IsAttached ?
            new CancellationTokenSource().Token :
            new CancellationTokenSource(TimeSpan.FromSeconds(seconds)).Token;

    public static Assembly Emit(this Compilation compilation)
    {
        using (var stream = new MemoryStream())
        {
            var result = compilation.Emit(stream);
            if (!result.Success)
            {
                Assert.False(true,
                    "Emit failed:\r\n" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, result.Diagnostics.Select(d => d.ToString())));
            }

            stream.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(stream.ToArray());
        }
    }
}