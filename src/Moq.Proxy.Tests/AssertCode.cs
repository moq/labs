using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Moq.Proxy.Tests
{
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
    }
}
