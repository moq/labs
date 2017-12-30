using System;
using System.Collections.Generic;
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
        var compilation = await document.Project.GetCompilationAsync(TimeoutToken(5));
        var noWarn = new HashSet<string>
        {
            //"CS1701", // fusion reference mismatch, binding redirect required.
        };
        var diagnostics = compilation.GetDiagnostics(TimeoutToken(5)).Where(d => !noWarn.Contains(d.Id)).ToArray();
        if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning))
        {
            SyntaxNode syntax;
            try
            {
                // Attempt to normalize whitespace and get the errors again, so the code is more legible
                syntax = await document.GetSyntaxRootAsync(TimeoutToken(1));
                syntax = syntax.NormalizeWhitespace();
                document = document.WithSyntaxRoot(syntax);
                compilation = await document.Project.GetCompilationAsync(TimeoutToken(5));
            }
            catch
            {
                syntax = await document.GetSyntaxRootAsync(TimeoutToken(1));
            }

            if (!string.IsNullOrEmpty(document.FilePath))
                File.WriteAllText(document.FilePath, (await document.GetTextAsync()).ToString());

            var indexOffset = document.Project.Language == LanguageNames.VisualBasic ? 1 : 0;

            Assert.False(true,
                Environment.NewLine +
                "Errors:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning).Select(d
                        => $"  '{d.Location.SourceTree.GetText().GetSubText(d.Location.SourceSpan).ToString()}' : {d.ToString()}")) +
                    Environment.NewLine +
                "Source:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine,
                        syntax.ToString()
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .Select((line, index) => $"{(index + indexOffset).ToString().PadLeft(3, ' ')}| {line}")) +
                    Environment.NewLine +
                "References:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine,
                        document.Project.MetadataReferences
                        .OfType<PortableExecutableReference>()
                        .Select(x => "  - " + x.FilePath)) +
                    Environment.NewLine);
        }
    }
}