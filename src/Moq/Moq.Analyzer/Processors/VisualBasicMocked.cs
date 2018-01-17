using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Moq.Sdk;
using Stunts;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Moq.Processors
{
    class VisualBasicMocked : IDocumentProcessor
    {
        public string[] Languages { get; } = new[] { LanguageNames.VisualBasic };

        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = new VisualBasicRewriteVisitor(SyntaxGenerator.GetGenerator(document)).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class VisualBasicRewriteVisitor : VisualBasicSyntaxRewriter
        {
            SyntaxGenerator generator;

            public VisualBasicRewriteVisitor(SyntaxGenerator generator) => this.generator = generator;

            public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
            {
                var result = base.VisitClassBlock(node);

                if (!generator.GetBaseAndInterfaceTypes(result).Any(x =>
                    x.ToString() == nameof(IMocked) ||
                    x.ToString() == typeof(IMocked).FullName))
                {
                    // Only add the base type if it isn't already there
                    result = generator.AddInterfaceType(result, generator.IdentifierName(nameof(IMocked)));
                }

                if (!generator.GetMembers(result).Any(x => generator.GetName(x) == "_mock"))
                {
                    var field = generator.FieldDeclaration("_mock", ParseTypeName(nameof(IMock)))
                        .WithTrailingTrivia(CarriageReturnLineFeed);

                    // Try to locate the _mock field following the pipeline field
                    var pipeline = generator.GetMembers(result).FirstOrDefault(x => generator.GetName(x) == "pipeline");
                    if (pipeline != null)
                    {
                        result = generator.InsertNodesAfter(result, pipeline, new[] { field });
                    }
                    else
                    {
                        result = generator.InsertMembers(result, 0, field
                            .WithLeadingTrivia(ElasticTab, ElasticTab));
                    }
                }

                if (!generator.GetMembers(result).Any(x => generator.GetName(x) == nameof(IMocked.Mock)))
                {
                    var property = (PropertyBlockSyntax)generator.PropertyDeclaration(
                    nameof(IMocked.Mock),
                    ParseTypeName(nameof(IMock)),
                    modifiers: DeclarationModifiers.ReadOnly,
                    getAccessorStatements: new[]
                    {
                        generator.ReturnStatement(
                            generator.InvocationExpression(
                                generator.MemberAccessExpression(
                                    generator.IdentifierName(nameof(LazyInitializer)),
                                    nameof(LazyInitializer.EnsureInitialized)),
                                generator.Argument(
                                    RefKind.Ref,
                                    generator.IdentifierName("_mock")),
                                ParenthesizedExpression(
                                    SingleLineFunctionLambdaExpression(
                                        FunctionLambdaHeader(List<AttributeListSyntax>(), TokenList(), ParameterList(), null),
                                        ObjectCreationExpression(
                                            List<AttributeListSyntax>(),
                                            IdentifierName(nameof(DefaultMock)),
                                            ArgumentList(SingletonSeparatedList<ArgumentSyntax>(
                                                SimpleArgument(MeExpression())
                                            )),
                                            null
                                        )
                                    )
                                )
                            )
                        )
                    });

                    property = property.WithPropertyStatement(
                        property.PropertyStatement.WithImplementsClause(
                            ImplementsClause(QualifiedName(IdentifierName(nameof(IMocked)), IdentifierName(nameof(IMocked.Mock))))));

                    // Try to locate the Mock property following the Behaviors property
                    var behaviors = generator.GetMembers(result).FirstOrDefault(x => generator.GetName(x) == nameof(IStunt.Behaviors));
                    if (behaviors != null)
                    {
                        result = generator.InsertNodesAfter(result, behaviors, new[] { property });
                    }
                    else
                    {
                        result = generator.AddMembers(result, property);
                    }
                }

                return result;
            }
        }
    }
}