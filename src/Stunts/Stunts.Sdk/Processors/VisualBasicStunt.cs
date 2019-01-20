using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Stunts.Processors
{
    /// <summary>
    /// Adds the <see cref="IStunt"/> interface implementation.
    /// </summary>
    public class VisualBasicStunt : IDocumentProcessor
    {
        /// <summary>
        /// Applies to <see cref="LanguageNames.VisualBasic"/> only.
        /// </summary>
        public string[] Languages { get; } = new[] { LanguageNames.VisualBasic };

        /// <summary>
        /// Runs in the final phase of codegen, <see cref="ProcessorPhase.Fixup"/>.
        /// </summary>
        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        /// <summary>
        /// Adds the <see cref="IStunt"/> interface implementation to the document.
        /// </summary>
        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default)
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            
            syntax = new VisualBasicStuntVisitor(SyntaxGenerator.GetGenerator(document)).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class VisualBasicStuntVisitor : VisualBasicSyntaxRewriter
        {
            SyntaxGenerator generator;

            public VisualBasicStuntVisitor(SyntaxGenerator generator) => this.generator = generator;

            public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
            {
                if (!node.Options.Any(opt => !opt.ChildTokens().Any(t => t.Kind() == SyntaxKind.StrictKeyword)))
                    node = node.AddOptions(OptionStatement(Token(SyntaxKind.StrictKeyword), Token(SyntaxKind.OnKeyword)));

                return base.VisitCompilationUnit(node);
            }

            public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
            {
                var result = base.VisitClassBlock(node);

                if (!generator.GetBaseAndInterfaceTypes(result).Any(x =>
                    x.ToString() == nameof(IStunt) ||
                    x.ToString() == typeof(IStunt).FullName))
                {
                    // Only add the base type if it isn't already there
                    result = generator.AddInterfaceType(
                        result,
                        generator.IdentifierName(nameof(IStunt)));
                }

                if (!generator.GetMembers(result).Any(x => generator.GetName(x) == nameof(IStunt.Behaviors)))
                {
                    var property = (PropertyBlockSyntax)generator.PropertyDeclaration(
                        nameof(IStunt.Behaviors),
                        GenericName("ObservableCollection", TypeArgumentList(IdentifierName(nameof(IStuntBehavior)))),
                        modifiers: DeclarationModifiers.ReadOnly,
                        getAccessorStatements: new[]
                        {
                            generator.ReturnStatement(
                                generator.MemberAccessExpression(
                                    IdentifierName("pipeline"),
                                    nameof(BehaviorPipeline.Behaviors)))
                        });

                    property = property.WithPropertyStatement(
                        property.PropertyStatement.WithImplementsClause(
                            ImplementsClause(QualifiedName(IdentifierName(nameof(IStunt)), IdentifierName(nameof(IStunt.Behaviors))))));

                    result = generator.InsertMembers(result, 0, property);
                }

                if (!generator.GetMembers(result).Any(x => generator.GetName(x) == "pipeline"))
                {
                    var field = generator.FieldDeclaration(
                        "pipeline",
                        generator.IdentifierName(nameof(BehaviorPipeline)),
                        modifiers: DeclarationModifiers.ReadOnly,
                        initializer: generator.ObjectCreationExpression(generator.IdentifierName(nameof(BehaviorPipeline))));

                    result = generator.InsertMembers(result, 0, field);
                }

                return result;
            }
        }
    }
}