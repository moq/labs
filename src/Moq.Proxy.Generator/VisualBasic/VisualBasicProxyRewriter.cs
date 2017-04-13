using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Editing;
using System.Linq;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;
using System;

namespace Moq.Proxy.VisualBasic
{
    [ExportLanguageService(typeof(IDocumentRewriter), LanguageNames.VisualBasic)]
    class VisualBasicProxyRewriter : VisualBasicSyntaxRewriter, IDocumentRewriter
    {
        ProxySyntaxRewriter rewriter;
        SyntaxGenerator generator;

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken)
        {
            generator = SyntaxGenerator.GetGenerator(document);
            rewriter = await ProxySyntaxRewriter.CreateAsync(document);

            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            syntax = Visit(syntax);

            // Apply fixups
            syntax = new VisualBasicParameterFixup().Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
        {
            node = (ClassBlockSyntax)rewriter.VisitClass(node);

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

            return base.VisitClassBlock(node);
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
            if (implements != null && implements.ToString() == $"{nameof(IProxy)}.{nameof(IProxy.Behaviors)}")
            {
                node = (PropertyBlockSyntax)rewriter.VisitBehaviorsProperty(
                    // Make the property private (== explicit interface implementation in C#)
                    node.WithPropertyStatement(
                        node.PropertyStatement.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)))));
            }
            else
            {
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
            }

            return base.VisitPropertyBlock(node);
        }
    }
}