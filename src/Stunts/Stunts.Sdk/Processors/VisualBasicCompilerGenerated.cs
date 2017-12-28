using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Stunts.Processors
{
    public class VisualBasicCompilerGenerated : IDocumentProcessor
    {
        public string Language => LanguageNames.VisualBasic;

        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = new VisualBasicRewriteVisitor(SyntaxGenerator.GetGenerator(document)).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class VisualBasicRewriteVisitor : VisualBasicSyntaxRewriter
        {
            SyntaxGenerator generator;

            public VisualBasicRewriteVisitor(SyntaxGenerator generator) => this.generator = generator;

            public override SyntaxNode VisitMethodBlock(MethodBlockSyntax node)
            {
                return base.VisitMethodBlock((MethodBlockSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }

            public override SyntaxNode VisitPropertyBlock(PropertyBlockSyntax node)
            {
                return base.VisitPropertyBlock((PropertyBlockSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }

            public override SyntaxNode VisitEventBlock(EventBlockSyntax node)
            {
                return base.VisitEventBlock((EventBlockSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }
        }
    }
}