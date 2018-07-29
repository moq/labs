using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Stunts.Processors
{
    /// <summary>
    /// Adds the <see cref="IStunt"/> interface implementation.
    /// </summary>
    public class CSharpStunt : IDocumentProcessor
    {
        /// <summary>
        /// Applies to <see cref="LanguageNames.CSharp"/> only.
        /// </summary>
        public string[] Languages { get; } = new[] { LanguageNames.CSharp };

        /// <summary>
        /// Runs in the final phase of codegen, <see cref="ProcessorPhase.Fixup"/>.
        /// </summary>
        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        /// <summary>
        /// Adds the <see cref="IStunt"/> interface implementation to the document.
        /// </summary>
        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default)
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = new CSharpStuntVisitor(document).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class CSharpStuntVisitor : CSharpSyntaxRewriter
        {
            SyntaxGenerator generator;
            Document document;

            public CSharpStuntVisitor(Document document)
            {
                this.document = document;
                generator = SyntaxGenerator.GetGenerator(document);
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

                if (!generator.GetBaseAndInterfaceTypes(node).Any(x => 
                    x.ToString() == nameof(IStunt) ||
                    x.ToString() == typeof(IStunt).FullName))
                {
                    // Only add the base type if it isn't already there
                    node = node.AddBaseListTypes(SimpleBaseType(IdentifierName(nameof(IStunt))));
                }

                if (!generator.GetMembers(node).Any(x => generator.GetName(x) == nameof(IStunt.Behaviors)))
                {
                    node = (ClassDeclarationSyntax)generator.InsertMembers(node, 0,
                        PropertyDeclaration(
                            GenericName(
                                Identifier("ObservableCollection"),
                                TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(nameof(IStuntBehavior))))),
                            Identifier(nameof(IStunt.Behaviors)))
                            .WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifier(IdentifierName(nameof(IStunt))))
                            .WithExpressionBody(ArrowExpressionClause(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("pipeline"),
                                    IdentifierName("Behaviors"))))
                             .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                            .NormalizeWhitespace()
                            .WithTrailingTrivia(CarriageReturnLineFeed, CarriageReturnLineFeed)
                        );
                }

                if (!generator.GetMembers(node).Any(x => generator.GetName(x) == "pipeline"))
                {
                    node = (ClassDeclarationSyntax)generator.InsertMembers(node, 0,
                        FieldDeclaration(
                            VariableDeclaration(IdentifierName(Identifier(nameof(BehaviorPipeline))))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier("pipeline"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            ObjectCreationExpression(IdentifierName(nameof(BehaviorPipeline)))
                                            .WithArgumentList(ArgumentList())))))
                            .NormalizeWhitespace()
                        ).WithModifiers(TokenList(Token(SyntaxKind.ReadOnlyKeyword))));
                }

                return node;
            }
        }
    }
}