using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, GeneratorLayer.Scaffold)]
    [Shared]
    class VisualBasicVirtualMembersScaffold : VirtualMembersScaffold
    {
        // VB can't override virtual events :\
        protected override SyntaxNode AddEvent(SyntaxGenerator generator, SyntaxNode syntax, IEventSymbol symbol, SyntaxNode @event) 
            => syntax.WithLeadingTrivia(syntax.GetLeadingTrivia().Add(
                CommentTrivia($"' NOTE: overriding virtual events is not supported by VB.NET. Skipping {generator.GetName(@event)}. See https://github.com/dotnet/vblang/issues/63")));

        protected override SyntaxNode AddProperty(SyntaxGenerator generator, SyntaxNode syntax, IPropertySymbol symbol, SyntaxNode property)
        {
            var modifiers = generator.GetModifiers(property);
            if (symbol.GetMethod == null)
                modifiers = modifiers.WithIsWriteOnly(true);
            if (symbol.SetMethod == null)
                modifiers = modifiers.WithIsReadOnly(true);

            property = generator.WithModifiers(property, modifiers);

            if (symbol.IsIndexer)
            {
                property = generator.AddParameters(property, symbol.Parameters.Select(x => generator.ParameterDeclaration(x)));
                var block = (PropertyBlockSyntax)property;

                property = block.WithPropertyStatement(block.PropertyStatement
                    .AddModifiers(Token(SyntaxKind.DefaultKeyword))
                    .AddParameterListParameters(symbol.Parameters.Select(x => generator.ParameterDeclaration(x)).OfType<ParameterSyntax>().ToArray()));
            }

            return base.AddProperty(generator, syntax, symbol, property);
        }
    }
}
