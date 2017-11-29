using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Moq.Proxy;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;
using System;
using System.Linq;

namespace Moq.Sdk
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, DocumentVisitorLayer.Fixup)]
    public class VisualBasicMocked : VisualBasicSyntaxRewriter, IDocumentVisitor
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
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

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