using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Moq.Proxy
{
    class ProxySyntaxRewriter
    {
        SyntaxGenerator generator;
        INamedTypeSymbol proxyType;
        IPropertySymbol behaviorsProp;

        public static async Task<ProxySyntaxRewriter> CreateAsync(Document document)
        {
            var compilation = await document.Project.GetCompilationAsync();

            var proxyType = compilation.GetTypeByMetadataName(typeof(IProxy).FullName);
            var behaviorsProp = proxyType.GetMembers().OfType<IPropertySymbol>().First(prop => prop.Name == nameof(IProxy.Behaviors));

            return new ProxySyntaxRewriter(document, proxyType, behaviorsProp);
        }

        private ProxySyntaxRewriter(Document document, INamedTypeSymbol proxyType, IPropertySymbol behaviorsProp)
        {
            generator = SyntaxGenerator.GetGenerator(document);
            this.proxyType = proxyType;
            this.behaviorsProp = behaviorsProp;
        }

        public SyntaxNode VisitClass(SyntaxNode node)
            => generator.InsertMembers(node, 0,
                generator.FieldDeclaration(
                    "pipeline",
                    generator.IdentifierName(nameof(BehaviorPipeline)),
                    initializer: generator.ObjectCreationExpression(generator.IdentifierName(nameof(BehaviorPipeline)))));

        public SyntaxNode VisitBehaviorsProperty(SyntaxNode property)
            => generator.WithGetAccessorStatements(property, new[]
            {
                generator.ReturnStatement(
                    generator.MemberAccessExpression(
                        generator.IdentifierName("pipeline"), 
                        nameof(BehaviorPipeline.Behaviors)))
            });
    }
}
