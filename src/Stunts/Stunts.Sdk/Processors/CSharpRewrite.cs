using System.Collections.Generic;
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
    /// Rewrites all members so they are implemented through 
    /// the <see cref="BehaviorPipeline"/> field added to the 
    /// class by the <see cref="CSharpStunt"/>.
    /// </summary>
    public class CSharpRewrite : IDocumentProcessor
    {
        /// <summary>
        /// Applies to <see cref="LanguageNames.CSharp"/> only.
        /// </summary>
        public string[] Languages { get; } = new[] { LanguageNames.CSharp };

        /// <summary>
        /// Runs in the third phase of codegen, <see cref="ProcessorPhase.Rewrite"/>.
        /// </summary>
        public ProcessorPhase Phase => ProcessorPhase.Rewrite;

        /// <summary>
        /// Rewrites all members in the document.
        /// </summary>
        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default)
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = new CSharpRewriteVisitor(document).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class CSharpRewriteVisitor : CSharpSyntaxRewriter
        {
            SyntaxGenerator generator;
            Document document;

            public CSharpRewriteVisitor(Document document)
            {
                this.document = document;
                generator = SyntaxGenerator.GetGenerator(document);
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // Turn event fields into event declarations.
                var events = node.ChildNodes().OfType<EventFieldDeclarationSyntax>().ToArray();
                node = node.RemoveNodes(events, SyntaxRemoveOptions.KeepNoTrivia);

                node = node.AddMembers(events
                    .Select(x => EventDeclaration(x.Declaration.Type, x.Declaration.Variables.First().Identifier)
                        .WithModifiers(x.Modifiers))
                    .ToArray());

                return node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (generator.GetAttributes(node).Any(attr => generator.GetName(attr) == "CompilerGenerated"))
                    return base.VisitMethodDeclaration(node);

                var outParams = node.ParameterList.Parameters.Where(x => x.Modifiers.Any(SyntaxKind.OutKeyword)).ToArray();
                var refOutParams = node.ParameterList.Parameters.Where(x => x.Modifiers.Any(SyntaxKind.RefKeyword) || x.Modifiers.Any(SyntaxKind.OutKeyword)).ToArray();

                if (outParams.Length != 0 || refOutParams.Length != 0)
                {
                    node = (MethodDeclarationSyntax)generator.ImplementMethod(node, node.ReturnType.IsVoid() ? null : node.ReturnType, outParams, refOutParams);
                }
                else
                {
                    if (node.Body != null)
                        node = node.RemoveNodes(new SyntaxNode[] { node.Body }, SyntaxRemoveOptions.KeepNoTrivia);

                    node = node.WithExpressionBody(
                            ArrowExpressionClause(ExecutePipeline(node.ReturnType, node.ParameterList.Parameters)))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }

                return base.VisitMethodDeclaration(node);
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (generator.GetAttributes(node).Any(attr => generator.GetName(attr) == "CompilerGenerated"))
                    return base.VisitPropertyDeclaration(node);

                (var canRead, var canWrite) = generator.InspectProperty(node);
                canRead = canRead || node.ExpressionBody != null;

                if (node.ExpressionBody != null)
                    node = node.RemoveNode(node.ExpressionBody, SyntaxRemoveOptions.KeepNoTrivia);

                node = node.WithAccessorList(null);

                if (canRead && !canWrite)
                {
                    node = node
                        .WithExpressionBody(ArrowExpressionClause(ExecutePipeline(node.Type, Enumerable.Empty<ParameterSyntax>())))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }
                else
                {
                    if (canRead)
                    {
                        node = node.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithExpressionBody(ArrowExpressionClause(ExecutePipeline(node.Type, Enumerable.Empty<ParameterSyntax>())))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
                    }
                    if (canWrite)
                    {
                        node = node.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithExpressionBody(ArrowExpressionClause(
                                // NOTE: we always append the implicit "value" parameter for setters.
                                ExecutePipeline(null, new[] { Parameter(Identifier("value")).WithType(node.Type) })))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
                    }
                }

                return base.VisitPropertyDeclaration(node);
            }

            public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                if (generator.GetAttributes(node).Any(attr => generator.GetName(attr) == "CompilerGenerated"))
                    return base.VisitIndexerDeclaration(node);

                var trivia = node.GetTrailingTrivia();

                // NOTE: Most of this code could be shared with VisitPropertyDeclaration but the mutating With* 
                // and props like ExpressionBody aren't available in the shared base BasePropertyDeclarationSyntax type :(
                var canRead = (node.ExpressionBody != null || node.AccessorList?.Accessors.Any(x => x.IsKind(SyntaxKind.GetAccessorDeclaration)) == true);
                var canWrite = node.AccessorList?.Accessors.Any(x => x.IsKind(SyntaxKind.SetAccessorDeclaration)) == true;
                if (node.ExpressionBody != null)
                    node = node.RemoveNode(node.ExpressionBody, SyntaxRemoveOptions.KeepNoTrivia);
                if (node.AccessorList?.Accessors.Any() == true)
                    node = node.RemoveNodes(node.AccessorList.Accessors, SyntaxRemoveOptions.KeepNoTrivia);

                if (canRead && !canWrite)
                {
                    node = node.WithExpressionBody(
                        ArrowExpressionClause(ExecutePipeline(node.Type, node.ParameterList.Parameters)));
                }
                else
                {
                    if (canRead)
                    {
                        node = node.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithExpressionBody(ArrowExpressionClause(ExecutePipeline(node.Type, node.ParameterList.Parameters)))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
                    }
                    if (canWrite)
                    {
                        node = node.AddAccessorListAccessors(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithExpressionBody(ArrowExpressionClause(
                                ExecutePipeline(null, node.ParameterList.Parameters.Concat(new[] { Parameter(Identifier("value")).WithType(node.Type) }))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
                    }
                }

                return base.VisitIndexerDeclaration(node.WithTrailingTrivia(trivia));
            }

            public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
            {
                if (generator.GetAttributes(node).Any(attr => generator.GetName(attr) == "CompilerGenerated"))
                    return base.VisitEventDeclaration(node);

                var parameters = new[] { Parameter(Identifier("value")).WithType(node.Type) };
                node = node.WithAccessorList(
                    AccessorList(
                        List(new AccessorDeclarationSyntax[]
                        {
                            AccessorDeclaration(SyntaxKind.AddAccessorDeclaration)
                                .WithExpressionBody(
                                    ArrowExpressionClause(ExecutePipeline(node.Type, parameters)))
                                .WithSemicolon(),
                            AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration)
                                .WithExpressionBody(
                                    ArrowExpressionClause(ExecutePipeline(node.Type, parameters)))
                                .WithSemicolon()
                        })));

                return base.VisitEventDeclaration(node);
            }

            ExpressionSyntax ExecutePipeline(TypeSyntax returnType, IEnumerable<SyntaxNode> parameters)
                => (ExpressionSyntax)generator.ExecutePipeline(returnType.IsVoid() ? null : returnType, parameters);
        }
    }
}