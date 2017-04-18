using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static TestHelpers;

public static class AssertCode
{
    public static void Compiles(SyntaxNode proxyCode)
    {
        // TODO: see how the VB side can be done
        var compilation = CSharpCompilation.Create("codegen")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => File.Exists(asm.ManifestModule.FullyQualifiedName))
                .Select(asm => MetadataReference.CreateFromFile(asm.ManifestModule.FullyQualifiedName)))
            .AddSyntaxTrees(CompilationUnit()
                .WithUsings(
                    List(
                        new UsingDirectiveSyntax[]
                        {
                                UsingDirective(IdentifierName("System")),
                                UsingDirective(
                                    QualifiedName(
                                        QualifiedName(
                                            IdentifierName("System"),
                                            IdentifierName("Collections")),
                                        IdentifierName("Generic"))),
                                UsingDirective(
                                    QualifiedName(
                                        IdentifierName("System"),
                                        IdentifierName("Reflection"))),
                                UsingDirective(
                                    QualifiedName(
                                        IdentifierName("Moq"),
                                        IdentifierName("Proxy"))),
                        }))
                .AddMembers((MemberDeclarationSyntax)proxyCode).SyntaxTree);

        var result = compilation.Emit("codegen.dll");
        if (!result.Success)
        {
            var error = new StringBuilder();
            error.AppendLine("Code failed to compile. Errors:");
            foreach (var diag in result.Diagnostics)
            {
                error.Append("\t").AppendLine(diag.GetMessage());
            }

            error.AppendLine("Source: ");
            error.AppendLine(proxyCode.NormalizeWhitespace().ToFullString());

            Assert.False(true, error.ToString());
        }
    }

    public static async Task NoErrorsAsync(Document document)
    {
        var compilation = await document.Project.GetCompilationAsync(TimeoutToken(2));
        var syntax = await document.GetSyntaxRootAsync(TimeoutToken(1));

        if (compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            try
            {
                // Attempt to normalize whitespace and get the errors again, so the code is more legible
                syntax = syntax.NormalizeWhitespace();
                document = document.WithSyntaxRoot(syntax);
                compilation = await document.Project.GetCompilationAsync(TimeoutToken(2));
            }
            catch (OperationCanceledException) { }

            var indexOffset = document.Project.Language == LanguageNames.VisualBasic ? 1 : 0;

            Assert.False(true,
                Environment.NewLine +
                string.Join(Environment.NewLine, compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d
                    => $"'{syntax.GetText().GetSubText(d.Location.SourceSpan).ToString()}' : {d.ToString()}")) +
                Environment.NewLine +
                string.Join(Environment.NewLine,
                    syntax.ToString()
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select((line, index) => $"{(index + indexOffset).ToString().PadLeft(3, ' ')}| {line}")));
        }
    }
}