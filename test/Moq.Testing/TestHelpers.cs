using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Moq.Proxy;

static class TestHelpers
{
    public static (Workspace workspace, Project project) CreateWorkspaceAndProject(string language)
    {
        var workspace = new AdhocWorkspace(ProxyGenerator.CreateHost());
        var project = workspace.AddProject("code", language)
            .WithCompilationOptions(language == LanguageNames.CSharp ?
                (CompilationOptions)new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) :
                (CompilationOptions)new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithMetadataReferences(ReferencePaths.Paths
                .Select(path => MetadataReference.CreateFromFile(path)));

        return (workspace, project);
    }

    public static CancellationToken TimeoutToken(int seconds)
        => Debugger.IsAttached ?
            new CancellationTokenSource().Token :
            new CancellationTokenSource(TimeSpan.FromSeconds(seconds)).Token;
}