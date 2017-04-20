using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Moq.Proxy.Scaffold
{
    abstract class OverrideVirtualMembers : IDocumentVisitor
    {
        public async Task<Document> VisitAsync(ILanguageServices services, Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var project = document.Project;
            var generator = SyntaxGenerator.GetGenerator(project);

            var tree = await document.GetSyntaxTreeAsync(cancellationToken);
            var root = tree.GetRoot(cancellationToken);
            var originalClass = root.DescendantNodes().First(node => generator.GetDeclarationKind(node) == DeclarationKind.Class);
            // Preserve the parent compilation unit since otherwise, the updated class syntax doesn't have any of the original usings.
            var compilationUnit = originalClass.Parent;
            var baseType = generator.GetBaseAndInterfaceTypes(originalClass).FirstOrDefault();
            var semantic = await document.GetSemanticModelAsync(cancellationToken);
            var symbol = semantic.GetSymbolInfo(baseType);

            // If the first symbol isn't a base class, there's nothing to do.
            if (symbol.Symbol == null || symbol.Symbol.Kind != SymbolKind.NamedType ||
                ((INamedTypeSymbol)symbol.Symbol).TypeKind != TypeKind.Class)
                return document;

            var newSyntax = originalClass;
            foreach (var member in ((INamedTypeSymbol)symbol.Symbol).GetMembers().Where(x => x.IsVirtual && x.CanBeReferencedByName))
            {
                if (member.Kind == SymbolKind.Event)
                {
                    newSyntax = DeclareEvent(generator, newSyntax, (IEventSymbol)member);
                }
                else if (member.Kind == SymbolKind.Property)
                {
                    newSyntax = DeclareProperty(generator, newSyntax, (IPropertySymbol)member);
                }
                else if (member.Kind == SymbolKind.Method)
                {
                    newSyntax = DeclareMethod(generator, newSyntax, (IMethodSymbol)member);
                }
            }

            document = document.WithSyntaxRoot(generator.ReplaceNode(root, originalClass, newSyntax));

            return document;
        }

        SyntaxNode DeclareEvent(SyntaxGenerator generator, SyntaxNode syntax, IEventSymbol symbol)
            => AddEvent(generator, syntax, symbol, generator.WithModifiers(generator.EventDeclaration(symbol), DeclarationModifiers.Override));

        protected virtual SyntaxNode AddEvent(SyntaxGenerator generator, SyntaxNode syntax, IEventSymbol symbol, SyntaxNode @event)
            => generator.AddMembers(syntax, @event);

        SyntaxNode DeclareProperty(SyntaxGenerator generator, SyntaxNode syntax, IPropertySymbol symbol) =>
            AddProperty(generator, syntax, symbol, generator.WithModifiers(
                generator.PropertyDeclaration(
                    symbol,
                    new[]
                    {
                        generator.ThrowStatement(
                            generator.ObjectCreationExpression(generator.IdentifierName(nameof(NotImplementedException))))
                    },
                    new[]
                    {
                        generator.ThrowStatement(
                            generator.ObjectCreationExpression(generator.IdentifierName(nameof(NotImplementedException))))
                    }
                ),
                DeclarationModifiers.Override
            )
        );

        protected virtual SyntaxNode AddProperty(SyntaxGenerator generator, SyntaxNode syntax, IPropertySymbol symbol, SyntaxNode property)
            => generator.AddMembers(syntax, property);

        SyntaxNode DeclareMethod(SyntaxGenerator generator, SyntaxNode syntax, IMethodSymbol symbol) 
            => generator.AddMembers(
                syntax,
                generator.WithModifiers(
                    generator.MethodDeclaration(
                        symbol,
                        new[]
                        {
                            generator.ThrowStatement(
                                generator.ObjectCreationExpression(generator.IdentifierName(nameof(NotImplementedException))))
                        }
                    ),
                    DeclarationModifiers.Override
                )
              );
    }
}
