using System;
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
            var composition = new ContainerConfiguration()
                .WithAssemblies(MefHostServices
                    .DefaultAssemblies
                    .Add(typeof(CodeFixNamesGenerator).Assembly))
                .CreateContainer();

            var providers = composition.GetExports<Lazy<CodeFixProvider, CodeChangeProviderMetadata>>();

            var allFixes = new HashSet<string>();
            var codeFixes = new Dictionary<string, HashSet<string>>
            {
                { "All", allFixes }
            };

            foreach (var provider in providers.Where(x => !string.IsNullOrEmpty(x.Metadata.Name)))
            {
                foreach (var language in provider.Metadata.Languages)
                {
                    if (!codeFixes.ContainsKey(language))
                        codeFixes.Add(language, new HashSet<string>());

                    codeFixes[language].Add(provider.Metadata.Name);
                    allFixes.Add(provider.Metadata.Name);
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
                ns.NormalizeWhitespace().WriteTo(output);
            }
        }
    }
}
