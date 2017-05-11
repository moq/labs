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
    public class VisualBasicMockGenerator : VisualBasicSyntaxRewriter, IDocumentVisitor
    {
        SyntaxGenerator generator;

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            generator = SyntaxGenerator.GetGenerator(document);
            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            syntax = Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
            => generator.AddMembers(
                base.VisitClassBlock(node),
                generator.FieldDeclaration(
                    "_mock",
                    ParseTypeName(nameof(IMock)),
                    initializer: generator.ObjectCreationExpression(
                        ParseTypeName(nameof(MockInfo)))
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
                    generator.ReturnStatement(IdentifierName("_mock"))
                });
            }

            return base.VisitPropertyBlock(node);
        }
    }
}