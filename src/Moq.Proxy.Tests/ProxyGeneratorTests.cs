using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Xunit;
using Xunit.Abstractions;

namespace Moq.Proxy.Tests
{
    public class ProxyGeneratorTests
    {
        ITestOutputHelper output;

        public ProxyGeneratorTests(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanGenerateProxy(string language)
        {
            var workspace = new AdhocWorkspace(ProxyGenerator.DefaultHost);
            var project = workspace.AddProject("code", language)
                .WithCompilationOptions(language == LanguageNames.CSharp ?
                    (CompilationOptions)new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) :
                    (CompilationOptions)new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithMetadataReferences(Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                    .Select(name => Assembly.Load(name))
                    .Concat(new[] { typeof(IProxy).Assembly })
                    .Concat(typeof(IProxy).Assembly.GetReferencedAssemblies().Select(x => Assembly.Load(x)))
                    .Where(asm => File.Exists(asm.ManifestModule.FullyQualifiedName))
                    .Select(asm => MetadataReference.CreateFromFile(asm.ManifestModule.FullyQualifiedName)))
                .AddMetadataReference(MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName));

            var compilation = await project.GetCompilationAsync();

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project,
                new[]
                {
                    compilation.GetTypeByMetadataName(typeof(IDisposable).FullName),
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName),
                    compilation.GetTypeByMetadataName(typeof(ICalculator).FullName),
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName),
                    compilation.GetTypeByMetadataName(typeof(IFormattable).FullName),
                });

            var syntax = await document.GetSyntaxRootAsync();

            output.WriteLine(syntax.NormalizeWhitespace().ToString());

            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax);

            // Try to compile again.
            compilation = await document.Project.GetCompilationAsync();

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));
        }

    }
}
