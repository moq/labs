using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Moq.Proxy.Rewrite
{
    /// <summary>
    /// Fixup for: 
    /// https://developercommunity.visualstudio.com/content/problem/40204/running-implement-interface-code-action-results-in.html
    /// </summary>
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, GeneratorLayer.Rewrite)]
    class VisualBasicParameterFixup : VisualBasicSyntaxRewriter, IDocumentVisitor
    {
        Dictionary<string, string> renamedParameters = new Dictionary<string, string>();
        SyntaxGenerator generator;

        public async Task<Document> VisitAsync(ILanguageServices services, Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            generator = SyntaxGenerator.GetGenerator(document);

            var syntax = await document.GetSyntaxRootAsync(cancellationToken);
            syntax = Visit(syntax);

            return document.WithSyntaxRoot(syntax);
        }

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
