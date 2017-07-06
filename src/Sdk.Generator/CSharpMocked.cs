using System;
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
                    Whitespace(Environment.NewLine),
                    Trivia(RegionDirectiveTrivia(false).WithEndOfDirectiveToken(
                        Token(TriviaList(PreprocessingMessage(nameof(IMocked))),
                        SyntaxKind.EndOfDirectiveToken,
                        TriviaList()))),
                    Whitespace(Environment.NewLine)));

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
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("pipeline"),
                                        IdentifierName(nameof(BehaviorPipeline.Behaviors))
                                    )
                                ))))
                            ))
                        }))
                    )
                ))
              // ; #endregion
              .WithSemicolonToken(Token(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(
                  Whitespace(Environment.NewLine),
                  Trivia(EndRegionDirectiveTrivia(false)))));

            result = generator.AddMembers(result, prop);

            return result;
        }
    }
}