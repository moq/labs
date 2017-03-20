using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq.Proxy.Properties;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Moq.Proxy.CSharp.ProxySyntaxFactory;

namespace Moq.Proxy.CSharp
{
    public class CSharpInterfaceProxyGenerator
    {
        public CSharpInterfaceProxyGenerator() { }

        public Task<SyntaxNode> GenerateAsync(
            CSharpCompilation compilation,
            IProgress<Diagnostic> progress, 
            CancellationToken cancellationToken,
            params INamedTypeSymbol[] interfaces)
        {
            //progress.Report(Diagnostic.Create("MG1000", "Generator",
            //    new LocalizableResourceString(nameof(Resources.MG1000), Resources.ResourceManager, typeof(Resources)),
            //    DiagnosticSeverity.Info, DiagnosticSeverity.Info, true, 1));

            var proxy = ProxyClass(ProxyClassIdentifier(interfaces))
                .AddProxiedTypes(interfaces)
                .AddImplementation(new ProxySyntaxInfo(interfaces));

            return Task.FromResult<SyntaxNode>(proxy);
        }
    }
}
