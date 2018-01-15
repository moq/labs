using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Stunts.Processors
{
    /// <summary>
    /// Usability overloads for the C# syntax.
    /// </summary>
    static class CSharpSyntaxExtensions
    {
        public static bool IsVoid(this TypeSyntax typeSyntax)
        {
            return typeSyntax.IsKind(SyntaxKind.PredefinedType) &&
                ((PredefinedTypeSyntax)typeSyntax).Keyword.IsKind(SyntaxKind.VoidKeyword);
        }

        public static bool IsKind(this SyntaxToken token, SyntaxKind kind)
        {
            return token.RawKind == (int)kind;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind)
        {
            return node?.RawKind == (int)kind;
        }

        public static AccessorDeclarationSyntax WithSemicolon(this AccessorDeclarationSyntax syntax) =>
            syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }
}
