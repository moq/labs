using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Moq.Proxy.Tests
{
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
                    .Select(path => MetadataReference.CreateFromFile(path)))
                .AddMetadataReference(MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName));

            return (workspace, project);
        }

        public static CancellationToken TimeoutToken(int seconds) 
            => Debugger.IsAttached ? 
                new CancellationTokenSource().Token :
                new CancellationTokenSource(TimeSpan.FromSeconds(seconds)).Token;
    }
}
