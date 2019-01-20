using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Stunts.Processors
{
    /// <summary>
    /// Removes unnecessary imports and sorts the used ones.
    /// </summary>
    public class FixupImports : IDocumentProcessor
    {
        /// <summary>
        /// Applies to both <see cref="LanguageNames.CSharp"/> and <see cref="LanguageNames.VisualBasic"/>.
        /// </summary>
        public string[] Languages { get; } = new[] { LanguageNames.CSharp, LanguageNames.VisualBasic };

        /// <summary>
        /// Runs in the last phase of code gen, <see cref="ProcessorPhase.Fixup"/>.
        /// </summary>
        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        /// <summary>
        /// Removes and sorts namespaces.
        /// </summary>
        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default)
        {
            // This codefix is available for both C# and VB
            document = await document.ApplyCodeFixAsync(CodeFixNames.All.RemoveUnnecessaryImports);

            var generator = SyntaxGenerator.GetGenerator(document);
            var syntax = await document.GetSyntaxRootAsync();
            var imports = generator.GetNamespaceImports(syntax).Select(generator.GetName).ToArray();

            Array.Sort(imports);

            if (document.Project.Language == LanguageNames.CSharp)
                syntax = new CSharpRewriteVisitor().Visit(syntax);
            else
                syntax = new VisualBasicRewriteVisitor().Visit(syntax);

            return document.WithSyntaxRoot(generator.AddNamespaceImports(syntax,
                imports.Select(generator.NamespaceImportDeclaration)));
        }

        class CSharpRewriteVisitor : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitCompilationUnit(Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax node)
                => base.VisitCompilationUnit(node.WithUsings(
                    Microsoft.CodeAnalysis.CSharp.SyntaxFactory.List<UsingDirectiveSyntax>()));
        }

        class VisualBasicRewriteVisitor : VisualBasicSyntaxRewriter
        {
            public override SyntaxNode VisitCompilationUnit(Microsoft.CodeAnalysis.VisualBasic.Syntax.CompilationUnitSyntax node)
                => base.VisitCompilationUnit(node.WithImports(
                    Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.List<ImportsStatementSyntax>()));
        }
    }
}
