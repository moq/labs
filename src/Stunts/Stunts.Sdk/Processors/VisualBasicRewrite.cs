using System;
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
    public class VisualBasicRewrite : IDocumentProcessor
    {
        public string Language => LanguageNames.VisualBasic;

        public ProcessorPhase Phase => ProcessorPhase.Rewrite;

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

            // The namespace for the proxy should be global, just like C#
            public override SyntaxNode VisitNamespaceStatement(NamespaceStatementSyntax node)
                => base.VisitNamespaceStatement(node.WithName(ParseName("Global." + node.Name)))
                        .WithTrailingTrivia(CarriageReturnLineFeed);

            public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
            {
                // Turn event fields into event declarations.
                var events = node.ChildNodes().OfType<EventStatementSyntax>().ToArray();
                node = node.RemoveNodes(events, SyntaxRemoveOptions.KeepNoTrivia);

                foreach (var e in events)
                {
                    var valueParam = ParameterList().AddParameters(Parameter(ModifiedIdentifier("value")).WithAsClause(e.AsClause));
                    var statements = List<StatementSyntax>(new[]
                    {
                        ExpressionStatement((ExpressionSyntax)generator.ExecutePipeline(null, valueParam.Parameters))
                    });

                    node = node.AddMembers(new[]
                    {
                        EventBlock(e.WithCustomKeyword(Token(SyntaxKind.CustomKeyword)), List(new[]
                        {
                            AddHandlerAccessorBlock(
                                AccessorStatement(SyntaxKind.AddHandlerAccessorStatement, Token(SyntaxKind.AddHandlerKeyword))
                                    .WithParameterList(valueParam))
                                    .WithStatements(statements),
                            RemoveHandlerAccessorBlock(
                                AccessorStatement(SyntaxKind.RemoveHandlerAccessorStatement, Token(SyntaxKind.RemoveHandlerKeyword))
                                    .WithParameterList(valueParam))
                                    .WithStatements(statements),
                            RaiseEventAccessorBlock(
                                AccessorStatement(SyntaxKind.RaiseEventAccessorStatement, Token(SyntaxKind.RaiseEventKeyword))
                                    .WithParameterList(ParameterList().AddParameters(
                                        Parameter(ModifiedIdentifier("sender")).WithAsClause(SimpleAsClause(PredefinedType(Token(SyntaxKind.ObjectKeyword)))),
                                        Parameter(ModifiedIdentifier("args")).WithAsClause(SimpleAsClause(IdentifierName(nameof(EventArgs)))))))
                        }))
                    });
                }

                node = (ClassBlockSyntax)generator.AddInterfaceType(
                    base.VisitClassBlock(node),
                    generator.IdentifierName(nameof(IStunt)));

                var field = generator.FieldDeclaration(
                    "pipeline",
                    generator.IdentifierName(nameof(BehaviorPipeline)),
                    modifiers: DeclarationModifiers.ReadOnly,
                    initializer: generator.ObjectCreationExpression(generator.IdentifierName(nameof(BehaviorPipeline))));

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

                return generator.InsertMembers(node, 0, field, property);
            }

            public override SyntaxNode VisitMethodBlock(MethodBlockSyntax node)
            {
                var outParams = node.BlockStatement.ParameterList.Parameters.Where(x => x.Modifiers.Any(SyntaxKind.OutKeyword)).ToArray();
                var refParams = node.BlockStatement.ParameterList.Parameters.Where(x => x.Modifiers.Any(SyntaxKind.ByRefKeyword)).ToArray();

                if (outParams.Length != 0 || refParams.Length != 0)
                    node = (MethodBlockSyntax)generator.ImplementMethod(node, generator.GetType(node), outParams, refParams);
                else
                    node = (MethodBlockSyntax)generator.ImplementMethod(node, generator.GetType(node));

                return base.VisitMethodBlock(node);
            }

            public override SyntaxNode VisitPropertyBlock(PropertyBlockSyntax node)
            {
                var implements = node.PropertyStatement?.ImplementsClause?.InterfaceMembers.FirstOrDefault();
                (var canRead, var canWrite) = generator.InspectProperty(node);
                var type = (TypeSyntax)generator.GetType(node);
                if (canRead)
                {
                    node = (PropertyBlockSyntax)generator.WithGetAccessorStatements(node, new[]
                    {
                        generator.ReturnStatement(generator.ExecutePipeline(type, generator.GetParameters(node)))
                    });
                }
                if (canWrite)
                {
                    node = (PropertyBlockSyntax)generator.WithSetAccessorStatements(node, new[]
                    {
                        generator.ExecutePipeline(null, generator
                            .GetParameters(node)
                            .Concat(new [] { Parameter(ModifiedIdentifier("value")).WithAsClause(SimpleAsClause(type)) }))
                    });
                }

                return base.VisitPropertyBlock(node);
            }
        }
    }
}