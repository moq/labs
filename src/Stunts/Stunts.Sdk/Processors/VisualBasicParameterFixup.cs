using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Stunts.Processors
{
    /// <summary>
    /// Fixup for: 
    /// https://developercommunity.visualstudio.com/content/problem/40204/running-implement-interface-code-action-results-in.html
    /// </summary>
    class VisualBasicParameterFixup : IDocumentProcessor
    {

        public string Language => LanguageNames.VisualBasic;

        public ProcessorPhase Phase => ProcessorPhase.Fixup;

        public async Task<Document> ProcessAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var syntax = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            syntax = new VisualBasicParameterFixupVisitor(SyntaxGenerator.GetGenerator(document)).Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

        class VisualBasicParameterFixupVisitor : VisualBasicSyntaxRewriter
        {
            Dictionary<string, string> renamedParameters = new Dictionary<string, string>();
            SyntaxGenerator generator;

            public VisualBasicParameterFixupVisitor(SyntaxGenerator generator) => this.generator = generator;

            public override SyntaxNode VisitParameterList(ParameterListSyntax node)
            {
                renamedParameters = new Dictionary<string, string>();
                return base.VisitParameterList(node);
            }

            public override SyntaxNode VisitParameter(ParameterSyntax node)
            {
                var method = node.FirstAncestorOrSelf<MethodBlockSyntax>();
                var syntax = method?.BlockStatement as MethodStatementSyntax;
                if (syntax?.Identifier.GetIdentifierText().Equals(node.Identifier.Identifier.GetIdentifierText(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    renamedParameters[node.Identifier.Identifier.Text] = "_" + node.Identifier.Identifier.Text;
                    node = node.WithIdentifier(node.Identifier.WithIdentifier(Identifier("_" + node.Identifier.Identifier.Text)));
                }

                return base.VisitParameter(node);
            }

            public override SyntaxNode VisitSimpleArgument(SimpleArgumentSyntax node)
            {
                var name = node.ToString();
                if (renamedParameters.ContainsKey(name))
                    return base.VisitSimpleArgument(SimpleArgument(IdentifierName(renamedParameters[name])));

                return base.VisitSimpleArgument(node);
            }
        }
    }
}
