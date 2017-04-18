using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Moq.Proxy.VisualBasic
{
    /// <summary>
    /// Fixup for: 
    /// https://developercommunity.visualstudio.com/content/problem/40204/running-implement-interface-code-action-results-in.html
    /// </summary>
    class VisualBasicParameterFixup : VisualBasicSyntaxRewriter
    {
        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            var method = node.FirstAncestorOrSelf<MethodBlockSyntax>();
            var syntax = method?.BlockStatement as MethodStatementSyntax;
            if (syntax?.Identifier.GetIdentifierText().Equals(node.Identifier.Identifier.GetIdentifierText(), StringComparison.OrdinalIgnoreCase) == true)
            {
                node = node.WithIdentifier(node.Identifier.WithIdentifier(SyntaxFactory.Identifier("_" + node.Identifier.Identifier.Text)));
            }

            return base.VisitParameter(node);
        }
    }
}
