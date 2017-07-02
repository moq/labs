using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Moq.Proxy;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Moq.Sdk
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, DocumentVisitorLayer.Fixup)]
    public class VisualBasicFixup : VisualBasicSyntaxRewriter, IDocumentVisitor
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
            => base.VisitCompilationUnit(node.AddImports(
                ImportsStatement(SingletonSeparatedList<ImportsClauseSyntax>(
                    SimpleImportsClause(IdentifierName(typeof(LazyInitializer).Namespace))))));

        public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
            => generator.InsertMembers(
                base.VisitClassBlock(node),
                1,
                generator.FieldDeclaration(
                    "_mock",
                    ParseTypeName(nameof(IMock))
                ));

        public override SyntaxNode VisitPropertyBlock(PropertyBlockSyntax node)
        {
            var type = (TypeSyntax)generator.GetType(node);
            var name = generator.GetName(node);

            if (type.ToString() == nameof(IMock) &&
                name == nameof(IMocked.Mock))
            {
                node = (PropertyBlockSyntax)generator.WithGetAccessorStatements(node, new[]
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
                                        IdentifierName(nameof(MockInfo)),
                                        ArgumentList(SingletonSeparatedList<ArgumentSyntax>(
                                            SimpleArgument(
                                                SimpleMemberAccessExpression(
                                                    IdentifierName("pipeline"),
                                                    IdentifierName(nameof(BehaviorPipeline.Behaviors))
                                                )
                                            )
                                        )),
                                        null
                                    )
                                )
                            )
                        )
                    )
                            //generator.Argument(
                            //    RefKind.None
                            //    generator.ValueReturningLambdaExpression(
                            //        new []
                            //        {
                            //            generator.ObjectCreationExpression(
                            //                generator.IdentifierName(nameof(MockInfo)),
                            //                generator.MemberAccessExpression(
                            //                    generator.IdentifierName("pipeline"),
                            //                    nameof(BehaviorPipeline.Behaviors)))
                            //        }
                            //    )
                            //)
                });
            }

            return base.VisitPropertyBlock(node);
        }
    }
}