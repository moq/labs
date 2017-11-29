using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Simplification;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Stunts.Tests.GeneratorTests
{
    public class StuntGeneratorTests
    {
        ITestOutputHelper output;

        public StuntGeneratorTests(ITestOutputHelper output) => this.output = output;


        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanGenerateStuntForInterface(string language, bool trace = false)
        {
            var generator = new StuntGenerator();
            var compilation = await CreateStunt(generator, language, typeof(IFoo), trace);
            var assembly = compilation.Emit();
            var type = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(type);

            var instance = Activator.CreateInstance(type);

            Assert.IsAssignableFrom<IFoo>(instance);
            Assert.IsAssignableFrom<IStunt>(instance);

            // If no behavior is configured, invoking it throws.
            Assert.Throws<NotImplementedException>(() => ((IFoo)instance).Do());

            // When we add at least one matching behavior, invocations succeed.
            instance.AddBehavior(new DefaultValueBehavior());
            ((IFoo)instance).Do();

            // The IStunt interface is properly implemented.
            Assert.Single(((IStunt)instance).Behaviors);
        }

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanGenerateStuntForClass(string language, bool trace = false)
        {
            var generator = new StuntGenerator();
            var compilation = await CreateStunt(generator, language, typeof(Foo), trace);
            var assembly = compilation.Emit();
            var type = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(type);

            var instance = Activator.CreateInstance(type);

            Assert.IsAssignableFrom<Foo>(instance);
            Assert.IsAssignableFrom<IStunt>(instance);


            // If no behavior is configured, invoking it throws.
            Assert.Throws<NotImplementedException>(() => ((Foo)instance).Do());

            // When we add at least one matching behavior, invocations succeed.
            instance.AddBehavior(new DefaultValueBehavior());
            ((Foo)instance).Do();

            // The IStunt interface is properly implemented.
            Assert.Single(((IStunt)instance).Behaviors);
        }

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task GeneratedNameContainsAdditionalInterfaceInName(string language, bool trace = false)
        {
            var compilation = await CreateStunt(new StuntGenerator(), language, new[] { typeof(INotifyPropertyChanged), typeof(IDisposable) }, trace);
            var assembly = compilation.Emit();
            var type = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(type);
            Assert.True(typeof(IDisposable).IsAssignableFrom(type));
            Assert.True(type.FullName.Contains(nameof(IDisposable)),
                $"Generated stunt should contain the additional type {nameof(IDisposable)} in its name.");
        }

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory(Skip = "Should add CompilerGenerated to each member instead.")]
        public async Task GeneratedInterfaceHasCompilerGeneratedAttribute(string language, bool trace = false)
        {
            var compilation = await CreateStunt(new StuntGenerator(), language, typeof(INotifyPropertyChanged), trace);
            var assembly = compilation.Emit();
            var type = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(type);
            Assert.True(type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any(),
                "Generated stunt did not have the 'CompilerGeneratedAttribute' attribute applied.");
        }

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task GeneratedTypeOverridesVirtualObjectMembers(string language, bool trace = false)
        {
            var compilation = await CreateStunt(new StuntGenerator(), language, new[] { typeof(INotifyPropertyChanged), typeof(IDisposable) }, trace);
            var assembly = compilation.Emit();
            var type = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(type);

            Assert.Contains(type.GetTypeInfo().DeclaredMethods, m =>
                m.Name == nameof(object.GetHashCode) ||
                m.Name == nameof(object.ToString) ||
                m.Name == nameof(object.Equals));
        }

        [Fact]
        public Task INotifyPropertyChanged()
            => CreateStunt(new StuntGenerator(), LanguageNames.VisualBasic, typeof(INotifyPropertyChanged));

        [Fact]
        public Task ITypeGetter()
            => CreateStunt(new StuntGenerator(), LanguageNames.VisualBasic, typeof(ITypeGetter));

        [Fact]
        public Task ICustomFormatter()
            => CreateStunt(new StuntGenerator(), LanguageNames.VisualBasic, typeof(ICustomFormatter));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeHasGlobalNamespaceThenItWorks(string language)
            => CreateStunt(new StuntGenerator(), language, typeof(IGlobal));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public Task WhenTypeIsInterface(string language)
            => CreateStunt(new StuntGenerator(), language, typeof(ICalculator));

        // TODO: why does this fail for VB? 
        // [InlineData(LanguageNames.VisualBasic)]
        [InlineData(LanguageNames.CSharp)]
        [Theory]
        public Task WhenTypeIsAbstract(string language)
            => CreateStunt(new StuntGenerator(), language, typeof(CalculatorBase));

        // TODO: why does this fail for VB? 
        //[InlineData(LanguageNames.VisualBasic)]
        [InlineData(LanguageNames.CSharp)]
        [Theory]
        public Task WhenTypeHasVirtualMembers(string language)
            => CreateStunt(new StuntGenerator(), language, typeof(Calculator));

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task CanGenerateProxyWithMultipleInterfaces(string language)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language);

            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = await new StuntGenerator().GenerateDocumentAsync(project, new[]
            {
                compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName),
                compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName),
                compilation.GetTypeByMetadataName(typeof(ICalculator).FullName),
            }, TimeoutToken(5));

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

            await Assert.ThrowsAsync<ArgumentException>(() => new StuntGenerator()
                .GenerateDocumentAsync(project, types, TimeoutToken(5)));
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

            await Assert.ThrowsAsync<ArgumentException>(() => new StuntGenerator()
                .GenerateDocumentAsync(project, types, TimeoutToken(5)));
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

            await Assert.ThrowsAsync<ArgumentException>(() => new StuntGenerator()
                .GenerateDocumentAsync(project, types, TimeoutToken(5)));
        }

        [Fact]
        public async Task WhenAdditionalGeneratorSpecifiedThenAddsAnnotation()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var types = new[]
            {
                compilation.GetTypeByMetadataName(typeof(IDisposable).FullName),
            };

            var doc = await new TestGenerator().GenerateDocumentAsync(project, types, TimeoutToken(5));
            var syntax = await doc.GetSyntaxRootAsync();
            var decl = syntax.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var trivia = decl.GetLeadingTrivia();
            // If we do this only once, it randomly fails :\
            // TODO: figure out why!
            //if (trivia.Count == 0)
            //{
            //    doc = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5), types);
            //    syntax = await doc.GetSyntaxRootAsync();
            //    decl = syntax.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            //    trivia = decl.GetLeadingTrivia();
            //}

            Assert.True(trivia.Any(SyntaxKind.SingleLineCommentTrivia));
        }


        [InlineData(LanguageNames.CSharp, @"public class Foo { }")]
        [InlineData(LanguageNames.VisualBasic, @"Public Class Foo 
End Class")]
        [Theory]
        public async Task CanCreateInstance(string language, string code, bool trace = false)
        {
            var compilation = await Compile(language, code, trace);
            var assembly = compilation.Emit();
            var type = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(type);
            Assert.Equal("Foo", type.FullName);

            var instance = Activator.CreateInstance(type);

            Assert.NotNull(instance);
        }

        Task<Compilation> CreateStunt(StuntGenerator generator, string language, Type type, bool trace = false)
            => CreateStunt(generator, language, new[] { type }, trace);

        async Task<Compilation> CreateStunt(StuntGenerator generator, string language, Type[] types, bool trace = false)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language);
            project = project.AddAnalyzerReference(new AnalyzerImageReference(new DiagnosticAnalyzer[] { new OverridableMembersAnalyzer() }.ToImmutableArray()));

            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var symbols = types.Select(t => compilation.GetTypeByMetadataName(t.FullName)).ToArray();
            var document = await generator.GenerateDocumentAsync(project, symbols, TimeoutToken(5));

            var syntax = await document.GetSyntaxRootAsync();
            document = project.AddDocument("code." + (language == LanguageNames.CSharp ? "cs" : "vb"), syntax, filePath: document.FilePath);

            await AssertCode.NoErrorsAsync(document);

            if (trace)
            {
                document = await Simplifier.ReduceAsync(document);
                var root = await document.GetSyntaxRootAsync();
                output.WriteLine(root.NormalizeWhitespace().ToFullString());
            }

            return await document.Project.GetCompilationAsync();
        }

        async Task<Compilation> Compile(string language, string code, bool trace = false)
        {
            var (workspace, project) = CreateWorkspaceAndProject(language);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));

            Assert.False(compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error),
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Select(d => d.GetMessage())));

            var document = project.AddDocument("code." + (language == LanguageNames.CSharp ? "cs" : "vb"), code);

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

    public class TestGenerator : StuntGenerator
    {
        public TestGenerator()
            : base("Test", GetDefaultProcessors().Concat(new[] { new TestProcessor() }).ToArray()) { }

        class TestProcessor : CSharpSyntaxRewriter, IDocumentProcessor
        {
            public string Language => LanguageNames.CSharp;

            public ProcessorPhase Phase => ProcessorPhase.Scaffold;

            public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
            {
                var syntax = await document.GetSyntaxRootAsync(cancellationToken);
                syntax = Visit(syntax);

                return document.WithSyntaxRoot(syntax);
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
                => base.VisitClassDeclaration(node.WithLeadingTrivia(SyntaxFactory.Comment("Test")));
        }
    }

    public interface IFoo
    {
        void Do();
    }

    public abstract class Foo : IFoo
    {
        public abstract void Do();
    }

    public interface ITypeGetter
    {
        Type GetType(string assembly, string name);
    }
}

public interface IGlobal
{
    void Do();
}