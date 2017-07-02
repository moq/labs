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
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, DocumentVisitorLayer.Prepare)]
    public class VisualBasicPrepare : VisualBasicSyntaxRewriter, IDocumentVisitor
    {
        SyntaxGenerator generator;

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            generator = SyntaxGenerator.GetGenerator(document);
            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            syntax = Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
            => generator.AddNamespaceImports(base.VisitCompilationUnit(node), generator.IdentifierName(typeof(IMocked).Namespace));

        public override SyntaxNode VisitClassBlock(ClassBlockSyntax node)
            => generator.AddInterfaceType(base.VisitClassBlock(node), generator.IdentifierName(nameof(IMocked)));
    }
}