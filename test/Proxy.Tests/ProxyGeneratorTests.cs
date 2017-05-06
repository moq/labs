using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManualProxies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Simplification;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Moq.Proxy.Tests
{
    public class ProxyGeneratorTests
    {
        ITestOutputHelper output;

        public ProxyGeneratorTests(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task GeneratedProxyDoesNotContainAdditionalInterfaceInName(string languageName)
        {
            var compilation = await CanGenerateProxy(languageName, typeof(INotifyPropertyChanged), typeof(IDisposable));
            var assembly = compilation.Emit();
            var proxyType = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(proxyType);
            Assert.True(typeof(IDisposable).IsAssignableFrom(proxyType));
            Assert.False(proxyType.FullName.Contains(nameof(IDisposable)),
                $"Generated proxy should not contain the additional type {nameof(IDisposable)} in its name.");
        }

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task GeneratedInterfaceHasCompilerGeneratedAttribute(string languageName)
        {
            var compilation = await CanGenerateProxy(languageName, typeof(INotifyPropertyChanged));
            var assembly = compilation.Emit();
            var proxyType = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(proxyType);
            Assert.True(proxyType.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any(),
                "Generated proxy did not have the 'CompilerGeneratedAttribute' attribute applied.");
        }

        [Fact]
        public Task INotifyPropertyChanged()
            => CanGenerateProxy(LanguageNames.VisualBasic, typeof(INotifyPropertyChanged));

        [Fact]
        public Task ITypeGetter()
            => CanGenerateProxy(LanguageNames.VisualBasic, typeof(ITypeGetter));

        [Fact]
        public Task ICustomFormatter()
            => CanGenerateProxy(LanguageNames.VisualBasic, typeof(ICustomFormatter));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeHasGlobalNamespaceThenItWorks(string language)
            => CanGenerateProxy(language, typeof(IGlobal));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeIsInterface(string language)
            => CanGenerateProxy(language, typeof(ICalculator));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeIsAbstract(string language)
            => CanGenerateProxy(language, typeof(CalculatorBase));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeHasVirtualMembers(string language)
            => CanGenerateProxy(language, typeof(Calculator));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanGenerateProxyWithMultipleInterfaces(string language)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language);

            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
                new[]
                {
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName),
                    compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName),
                    compilation.GetTypeByMetadataName(typeof(ICalculator).FullName),
                });

            var syntax = await document.GetSyntaxRootAsync();

            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax,
                filePath: Path.GetTempFileName());

            await AssertCode.NoErrorsAsync(document);
        }

        [Fact]
        public async Task WhenClassSymbolIsNotFirstThenThrows()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var types = new[]
            {
                compilation.GetTypeByMetadataName(typeof(ICalculator).FullName),
                compilation.GetTypeByMetadataName(typeof(Calculator).FullName),
            };

            await Assert.ThrowsAsync<ArgumentException>(() => new ProxyGenerator()
                .GenerateProxyAsync(workspace, project, TimeoutToken(5), types));
        }

        [Fact]
        public async Task WhenMultipleClassSymbolsThenThrows()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var types = new[]
            {
                compilation.GetTypeByMetadataName(typeof(object).FullName),
                compilation.GetTypeByMetadataName(typeof(Calculator).FullName),
            };

            await Assert.ThrowsAsync<ArgumentException>(() => new ProxyGenerator()
                .GenerateProxyAsync(workspace, project, TimeoutToken(5), types));
        }

        [Fact]
        public async Task WhenEnumSymbolIsSpecifiedThenThrows()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var types = new[]
            {
                compilation.GetTypeByMetadataName(typeof(PlatformID).FullName),
            };

            await Assert.ThrowsAsync<ArgumentException>(() => new ProxyGenerator()
                .GenerateProxyAsync(workspace, project, TimeoutToken(5), types));
        }

        [Fact]
        public async Task WhenAdditionalGeneratorSpecifiedThenAddsAnnotation()
        {
            var workspace = new AdhocWorkspace(
                ProxyGenerator.CreateHost(
                    new[] { Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName }.ToImmutableArray()));

            var projectInfo = CreateProjectInfo(LanguageNames.CSharp, "code");
            var project = workspace.AddProject(projectInfo);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var types = new[]
            {
                compilation.GetTypeByMetadataName(typeof(IDisposable).FullName),
            };

            var doc = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5), types);
            var syntax = await doc.GetSyntaxRootAsync();
            var decl = syntax.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var trivia = decl.GetLeadingTrivia();
            // If we do this only once, it randomly fails :\
            // TODO: figure out why!
            if (trivia.Count == 0)
            {
                doc = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5), types);
                syntax = await doc.GetSyntaxRootAsync();
                decl = syntax.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

                trivia = decl.GetLeadingTrivia();
            }

            Assert.True(trivia.Any(SyntaxKind.SingleLineCommentTrivia));
        }

        async Task<Compilation> CanGenerateProxy(string language, Type type, Type additionalInterface = null, bool trace = false)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language, type.FullName);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var additional = additionalInterface == null ?
                ImmutableArray<ITypeSymbol>.Empty :
                new ITypeSymbol[] { compilation.GetTypeByMetadataName(additionalInterface.FullName) }.ToImmutableArray();

            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
                new ITypeSymbol[] { compilation.GetTypeByMetadataName(type.FullName) }.ToImmutableArray(),
                additional);

            var syntax = await document.GetSyntaxRootAsync();

            document = project.AddDocument("proxy." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax, filePath: Path.GetTempFileName());

            await AssertCode.NoErrorsAsync(document);

            if (trace)
            {
                document = await Simplifier.ReduceAsync(document);
                var root = await document.GetSyntaxRootAsync();
                output.WriteLine(root.NormalizeWhitespace().ToFullString());
            }

            return await document.Project.GetCompilationAsync();
        }
    }

    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.CSharp, DocumentVisitorLayer.Scaffold)]
    public class TestGenerator : CSharpSyntaxRewriter, IDocumentVisitor
    {
        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            syntax = Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            => base.VisitClassDeclaration(node.WithLeadingTrivia(SyntaxFactory.Comment("Test")));
    }
}

public interface IGlobal
{
    void Do();
}