using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Moq.Proxy;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Moq.Sdk
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.CSharp, DocumentVisitorLayer.Fixup)]
    public class CSharpMocked : CSharpSyntaxRewriter, IDocumentVisitor
    {
        SyntaxGenerator generator;

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var hasSdk = document.Project.MetadataReferences.Any(x => x.Display.EndsWith("Moq.Sdk.dll")) ||
                document.Project.ProjectReferences.Select(x => document.Project.Solution.GetProject(x.ProjectId).AssemblyName).Any(x => x == "Moq.Sdk");

            // Only apply the Moq visitor if the project actually contains a Moq.Sdk reference.
            if (!hasSdk)
                return document;

            generator = SyntaxGenerator.GetGenerator(document);
            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            syntax = Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
            => base.VisitCompilationUnit(node.AddUsings(
                UsingDirective(IdentifierName(typeof(LazyInitializer).Namespace)),
                UsingDirective(IdentifierName(typeof(IMocked).Namespace))));

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var result = generator.AddInterfaceType(
                base.VisitClassDeclaration(node), 
                generator.IdentifierName(nameof(IMocked)));

            result = generator.AddMembers(result, 
                generator.FieldDeclaration("mock", ParseTypeName(nameof(IMock)))
                // #region IMocked
                .WithLeadingTrivia(
                    CarriageReturnLineFeed,
                    Trivia(RegionDirectiveTrivia(true)
                        .WithRegionKeyword(Token(
                            TriviaList(),
                            SyntaxKind.RegionKeyword,
                            TriviaList(Space)))
                        .WithEndOfDirectiveToken(Token(
                            TriviaList(PreprocessingMessage(nameof(IMocked))),
                            SyntaxKind.EndOfDirectiveToken,
                            TriviaList(CarriageReturnLineFeed))
                        )
                    )
                )
            );

            var prop = PropertyDeclaration(IdentifierName(nameof(IMock)), nameof(IMocked.Mock))
                // Make IMocked properties explicit.
                .WithExplicitInterfaceSpecifier(
                    ExplicitInterfaceSpecifier(
                        IdentifierName(nameof(IMocked))))
                .WithModifiers(TokenList())
                // => LazyInitializer.EnsureInitialized(ref mock, () => new MockInfo(pipeline.Behaviors));
                .WithExpressionBody(ArrowExpressionClause(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(nameof(LazyInitializer)),
                            IdentifierName(nameof(LazyInitializer.EnsureInitialized))),
                        ArgumentList(SeparatedList(new ArgumentSyntax[]
                        {
                            Argument(RefExpression(IdentifierName("mock"))),
                            Argument(ParenthesizedLambdaExpression(
                                ObjectCreationExpression(
                                    IdentifierName(nameof(MockInfo)))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(
                                    ThisExpression()
                                ))))
                            ))
                        }))
                    )
                ))
              .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
              // #endregion
              .WithTrailingTrivia(
                CarriageReturnLineFeed,
                Trivia(EndRegionDirectiveTrivia(false)),
                CarriageReturnLineFeed);

            result = generator.AddMembers(result, prop);

            return result;
        }
    }
}