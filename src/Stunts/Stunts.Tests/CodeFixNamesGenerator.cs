using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Stunts
{
    // Automatic generator for the CodeFixNames.g.cs
    class CodeFixNamesGenerator
    {
        // Re-run this method with TD.NET AdHoc runner to regenerate CodeFixNames.g.cs as needed.
        public void GenerateCodeFixNames()
        {
            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies.Concat(new[] { typeof(CodeFixNamesGenerator).Assembly }));
            var providers = host.GetExports<CodeFixProvider, IDictionary<string, object>>();

            var allFixes = new HashSet<string>();
            var codeFixes = new Dictionary<string, HashSet<string>>
            {
                { "All", allFixes }
            };

            foreach (var provider in providers.Where(x => 
                x.Metadata.TryGetValue("Name", out var value) && 
                value is string name &&
                !string.IsNullOrEmpty(name) && 
                x.Metadata.TryGetValue("Languages", out value) && 
                value is string[]))
            {
                foreach (var language in (string[])provider.Metadata["Languages"])
                {
                    if (!codeFixes.ContainsKey(language))
                        codeFixes.Add(language, new HashSet<string>());

                    codeFixes[language].Add((string)provider.Metadata["Name"]);
                    allFixes.Add((string)provider.Metadata["Name"]);
                }
            }

            var ns = NamespaceDeclaration(ParseName("Stunts.Processors"))
                .AddMembers(ClassDeclaration("CodeFixNames")
                    .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                    .WithMembers(List<MemberDeclarationSyntax>(codeFixes.Select(lang
                        => ClassDeclaration(lang.Key.Replace(" ", "").Replace("#", "Sharp"))
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.PartialKeyword)))
                        .WithMembers(List<MemberDeclarationSyntax>(lang.Value.OrderBy(x => x).Select(fix
                            => FieldDeclaration(VariableDeclaration(
                                PredefinedType(Token(SyntaxKind.StringKeyword)),
                                SeparatedList(new[] {
                                    VariableDeclarator(fix.Replace(" ", ""))
                                    .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(fix))))
                                })
                               ))
                               .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ConstKeyword)))
                            )
                        ))
                      ))
                    )
                );

            using (var output = new StreamWriter(@"..\..\..\Stunts.Sdk\Processors\CodeFixNames.g.cs", false))
            {
                output.WriteLine("#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member");
                ns.NormalizeWhitespace().WriteTo(output);
                output.WriteLine();
                output.WriteLine("#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member");
            }
        }
    }
}
