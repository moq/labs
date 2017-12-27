using System;
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
        public string Language => LanguageNames.VisualBasic;

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

            public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
                => base.VisitCompilationUnit(node.AddImports(
                    ImportsStatement(SingletonSeparatedList<ImportsClauseSyntax>(SimpleImportsClause(IdentifierName(typeof(LazyInitializer).Namespace)))),
                    ImportsStatement(SingletonSeparatedList<ImportsClauseSyntax>(SimpleImportsClause(IdentifierName(typeof(IMocked).Namespace))))));

            public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
            {
                var result = generator.AddInterfaceType(
                    base.VisitClassBlock(node),
                    generator.IdentifierName(nameof(IMocked)));

                result = generator.AddMembers(result,
                    generator.FieldDeclaration("_mock", ParseTypeName(nameof(IMock)))
                        .WithLeadingTrivia(Whitespace(Environment.NewLine)));

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
                                        IdentifierName(nameof(MockInfo)),
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

                result = generator.AddMembers(result, property);

                return result;
            }
        }
    }
}