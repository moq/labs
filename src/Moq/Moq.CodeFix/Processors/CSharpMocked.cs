using System.Linq;
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
        public string[] Languages { get; } = new[] { LanguageNames.CSharp };

        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default)
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = new CSharpRewriteVisitor(SyntaxGenerator.GetGenerator(document)).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class CSharpRewriteVisitor : CSharpSyntaxRewriter
        {
            SyntaxGenerator generator;

            public CSharpRewriteVisitor(SyntaxGenerator generator) => this.generator = generator;

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

                if (!generator.GetBaseAndInterfaceTypes(node).Any(x =>
                    x.ToString() == nameof(IMocked) ||
                    x.ToString() == typeof(IMocked).FullName))
                {
                    // Only add the base type if it isn't already there
                    node = node.AddBaseListTypes(SimpleBaseType(IdentifierName(nameof(IMocked))));
                }

                if (!generator.GetMembers(node).Any(x => generator.GetName(x) == "mock"))
                {
                    var field = FieldDeclaration(
                        VariableDeclaration(IdentifierName(Identifier(nameof(IMock))))
                            .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("mock"))))
                        );

                    // Try to locate the mock field following the pipeline field
                    var pipeline = generator.GetMembers(node).FirstOrDefault(x => generator.GetName(x) == "pipeline");
                    if (pipeline != null)
                    {
                        node = (ClassDeclarationSyntax)generator.InsertNodesAfter(node, pipeline,
                            new[] { field.WithLeadingTrivia(pipeline.GetLeadingTrivia()) });
                    }
                    else
                    {
                        node = (ClassDeclarationSyntax)generator.InsertMembers(node, 0, field
                            .WithLeadingTrivia(ElasticTab, ElasticTab)
                            .NormalizeWhitespace()
                            .WithTrailingTrivia(CarriageReturnLineFeed, CarriageReturnLineFeed));
                    }
                }

                if (!generator.GetMembers(node).Any(x => generator.GetName(x) == nameof(IMocked.Mock)))
                {
                    var property = PropertyDeclaration(IdentifierName(nameof(IMock)), nameof(IMocked.Mock))
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
                                    IdentifierName(nameof(DefaultMock)))
                                .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(
                                    ThisExpression()
                                ))))
                            ))
                                }))
                            )
                        ))
                      .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

                    // Try to locate the Mock property following the Behaviors property
                    var behaviors = generator.GetMembers(node).FirstOrDefault(x => generator.GetName(x) == nameof(IStunt.Behaviors));
                    if (behaviors != null)
                    {
                        node = (ClassDeclarationSyntax)generator.InsertNodesAfter(node, behaviors, new[] { property });
                    }
                    else
                    {
                        node = (ClassDeclarationSyntax)generator.AddMembers(node, property);
                    }
                }

                return node;
            }
        }
    }
}