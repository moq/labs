using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Moq.Proxy.Rewrite
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, DocumentVisitorLayer.Rewrite)]
    [Shared]
    class VisualBasicProxy : VisualBasicSyntaxRewriter, IDocumentVisitor
    {
        SyntaxGenerator generator;
        ICodeAnalysisServices services;

        [ImportingConstructor]
        public VisualBasicProxy(ICodeAnalysisServices services) => this.services = services;

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            generator = SyntaxGenerator.GetGenerator(document);

            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            syntax = Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        // The namespace for the proxy should be global, just like C#
        public override SyntaxNode VisitNamespaceStatement(NamespaceStatementSyntax node)
            =>  base.VisitNamespaceStatement(node.WithName(ParseName("Global." + ProxyFactory.ProxyNamespace)))
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
                generator.IdentifierName(nameof(IProxy)));

            var field = generator.FieldDeclaration(
                    "pipeline",
                    generator.IdentifierName(nameof(BehaviorPipeline)),
                    initializer: generator.ObjectCreationExpression(generator.IdentifierName(nameof(BehaviorPipeline))));

            var property = (PropertyBlockSyntax)generator.PropertyDeclaration(
                    nameof(IProxy.Behaviors),
                    GenericName("ObservableCollection", TypeArgumentList(IdentifierName(nameof(IProxyBehavior)))),
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
                    ImplementsClause(QualifiedName(IdentifierName(nameof(IProxy)), IdentifierName(nameof(IProxy.Behaviors))))));

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