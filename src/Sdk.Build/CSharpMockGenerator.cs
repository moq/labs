using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Moq.Proxy;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Moq.Sdk
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.CSharp, DocumentVisitorLayer.Fixup)]
    public class CSharpMockGenerator : CSharpSyntaxRewriter, IDocumentVisitor
    {
        SyntaxGenerator generator;

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            generator = SyntaxGenerator.GetGenerator(document);
            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            syntax = Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            => generator.AddMembers(
                base.VisitClassDeclaration(node),
                generator.FieldDeclaration(
                    "mock",
                    ParseTypeName(nameof(IMock)),
                    initializer: generator.ObjectCreationExpression(
                        ParseTypeName(nameof(MockInfo)))
                ));

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            // Make IMocked properties explicit.
            if (generator.GetName(node) == nameof(IMocked.Mock) && 
                generator.GetType(node).ToString() == nameof(IMock))
                return base.VisitPropertyDeclaration(node
                    .WithExplicitInterfaceSpecifier(
                        ExplicitInterfaceSpecifier(
                            IdentifierName(nameof(IMocked))))
                    .WithModifiers(TokenList())
                    .WithExpressionBody(ArrowExpressionClause(IdentifierName("mock")))
                );

            return base.VisitPropertyDeclaration(node);
        }
    }
}