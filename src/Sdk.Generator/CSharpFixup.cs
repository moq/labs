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
    public class CSharpFixup : CSharpSyntaxRewriter, IDocumentVisitor
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
            => base.VisitCompilationUnit(node.AddUsings(UsingDirective(IdentifierName(typeof(LazyInitializer).Namespace))));

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            => generator.InsertMembers(
                base.VisitClassDeclaration(node),
                1,
                generator.FieldDeclaration(
                    "mock",
                    ParseTypeName(nameof(IMock))
                ));

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (generator.GetName(node) == nameof(IMocked.Mock) &&
                generator.GetType(node).ToString() == nameof(IMock))
                return base.VisitPropertyDeclaration(node
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
                );

            return base.VisitPropertyDeclaration(node);
        }
    }
}