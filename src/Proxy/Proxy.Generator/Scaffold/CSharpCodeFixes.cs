using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.CSharp, DocumentVisitorLayer.Scaffold)]
    [Shared]

    class CSharpCodeFixes : CSharpSyntaxRewriter, IDocumentVisitor
    {
        static readonly string[] codeFixNames = new[]
        {
            CodeFixNames.CSharp.ImplementAbstractClass,
            CodeFixNames.CSharp.ImplementInterface,
            "OverrideAllMembersCodeFix",
        };

        ICodeAnalysisServices analysisServices;
        SyntaxGenerator generator;
        CancellationToken cancellationToken;

        [ImportingConstructor]
        public CSharpCodeFixes(ICodeAnalysisServices services) => analysisServices = services;

        public async Task<Document> VisitAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.cancellationToken = cancellationToken;
            generator = SyntaxGenerator.GetGenerator(document);
            var codeFixService = analysisServices.GetWorkspaceService<ICodeFixService>();

            foreach (var codeFixName in codeFixNames)
            {
                document = await codeFixService.ApplyAsync(codeFixName, document, cancellationToken);
            }

            return document;
        }
    }
}
