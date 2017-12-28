using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Stunts.Processors
{
    public class CSharpCompilerGenerated : IDocumentProcessor
    {
        public string Language => LanguageNames.CSharp;

        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = new CSharpRewriteVisitor(SyntaxGenerator.GetGenerator(document)).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class CSharpRewriteVisitor : CSharpSyntaxRewriter
        {
            SyntaxGenerator generator;

            public CSharpRewriteVisitor(SyntaxGenerator generator) => this.generator = generator;

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                return base.VisitMethodDeclaration((MethodDeclarationSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                return base.VisitPropertyDeclaration((PropertyDeclarationSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }

            public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                return base.VisitIndexerDeclaration((IndexerDeclarationSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }

            public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
            {
                return base.VisitEventDeclaration((EventDeclarationSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }
        }
    }
}