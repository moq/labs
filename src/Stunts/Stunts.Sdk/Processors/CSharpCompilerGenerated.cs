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
    /// Adds a <see cref="System.Runtime.CompilerServices.CompilerGeneratedAttribute"/> 
    /// attribute to all generated members, so that it's possible to distinguish user-authored 
    /// members in a partial class from the generated code.
    /// </summary>
    public class CSharpCompilerGenerated : IDocumentProcessor
    {
        /// <summary>
        /// Applies to <see cref="LanguageNames.CSharp"/> only.
        /// </summary>
        public string[] Languages { get; } = new[] { LanguageNames.CSharp };

        /// <summary>
        /// Runs in the final phase of codegen, <see cref="ProcessorPhase.Fixup"/>.
        /// </summary>
        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        /// <summary>
        /// Applies the attribute to all members in the document.
        /// </summary>
        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default)
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
                if (generator.GetAttributes(node).Any(attr => generator.GetName(attr) == "CompilerGenerated"))
                    return base.VisitMethodDeclaration(node);

                return base.VisitMethodDeclaration((MethodDeclarationSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }

            public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (generator.GetAttributes(node).Any(attr => generator.GetName(attr) == "CompilerGenerated"))
                    return base.VisitPropertyDeclaration(node);

                return base.VisitPropertyDeclaration((PropertyDeclarationSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }

            public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
            {
                if (generator.GetAttributes(node).Any(attr => generator.GetName(attr) == "CompilerGenerated"))
                    return base.VisitIndexerDeclaration(node);

                return base.VisitIndexerDeclaration((IndexerDeclarationSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }

            public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
            {
                if (generator.GetAttributes(node).Any(attr => generator.GetName(attr) == "CompilerGenerated"))
                    return base.VisitEventDeclaration(node);

                return base.VisitEventDeclaration((EventDeclarationSyntax)
                    generator.AddAttributes(node, Attribute(IdentifierName("CompilerGenerated"))));
            }
        }
    }
}