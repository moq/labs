using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Moq.Sdk;
using Stunts;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Moq.Processors
{
    class CSharpMocked : IDocumentProcessor
    {
        public string Language => LanguageNames.CSharp;

        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = new CSharpRewriteVisitor(SyntaxGenerator.GetGenerator(document)).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class CSharpRewriteVisitor : CSharpSyntaxRewriter
        {
            SyntaxGenerator generator;

            public CSharpRewriteVisitor(SyntaxGenerator generator) => this.generator = generator;

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
}