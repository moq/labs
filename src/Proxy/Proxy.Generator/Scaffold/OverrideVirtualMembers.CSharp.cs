using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.CSharp, DocumentVisitorLayer.Scaffold)]
    [Shared]
    class CSharpOverrideVirtualMembers : OverrideVirtualMembers
    {
        [ImportingConstructor]
        public CSharpOverrideVirtualMembers(ICodeAnalysisServices services)
            : base(services) { }

        protected override SyntaxNode AddEvent(SyntaxGenerator generator, SyntaxNode syntax, IEventSymbol symbol, SyntaxNode @event)
            => base.AddEvent(generator, syntax, symbol, 
                ((EventFieldDeclarationSyntax)@event).AddModifiers(Token(SyntaxKind.OverrideKeyword)));
    }
}
