using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Moq.Proxy.Rewrite
{
    //[ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, GeneratorLayer.Rewrite)]
    class VisualBasicShadowsFixup : VisualBasicSyntaxRewriter, IDocumentVisitor
    {
        ICodeAnalysisServices services;

        [ImportingConstructor]
        public VisualBasicShadowsFixup(ICodeAnalysisServices services) => this.services = services;

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
            => document.WithSyntaxRoot(Visit(await document.GetSyntaxRootAsync(cancellationToken)));

        public override SyntaxNode VisitClassBlock(ClassBlockSyntax node) 
            => node.WithLeadingTrivia(CommentTrivia("#Disable Warning BC40005 ' Member shadows an overridable method in the base type"))
                   .WithTrailingTrivia(CommentTrivia("#Enable Warning BC40005 ' Member shadows an overridable method in the base type"));
    }
}
