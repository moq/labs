using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Moq.Proxy
{
    public class CSharpClassProxyGenerator
    {
        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(
            MemberDeclarationSyntax applyTo, CSharpCompilation compilation, 
            IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var results = SyntaxFactory.List<MemberDeclarationSyntax>();

            if (applyTo is ClassDeclarationSyntax applyToClass)
                results = results.Add(applyToClass
                    .WithIdentifier(SyntaxFactory.Identifier(applyToClass.Identifier.ValueText + "Proxy")));

            return Task.FromResult(results);
        }
    }
}
