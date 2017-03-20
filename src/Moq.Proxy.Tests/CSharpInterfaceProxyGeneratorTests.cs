using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq.Proxy.CSharp;
using Xunit;
using Xunit.Abstractions;
using static Moq.Proxy.CSharp.ProxySyntaxFactory;

namespace Moq.Proxy.Tests
{
    public class CSharpInterfaceProxyGeneratorTests
    {
        ITestOutputHelper output;

        public CSharpInterfaceProxyGeneratorTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void BasicProxyCompiles()
        {
            var proxy = ProxyClass(ValidIdentifier("ProxyOfFoo"));

            AssertCode.Compiles(proxy);
            output.WriteLine(proxy.NormalizeWhitespace().ToFullString());
        }

        [Fact]
        public void SyntaxInfo()
        {
            var type = typeof(ICalculator);
            var compilation = CSharpCompilation.Create("codegen")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(asm => File.Exists(asm.ManifestModule.FullyQualifiedName))
                    .Select(asm => MetadataReference.CreateFromFile(asm.ManifestModule.FullyQualifiedName)));

            var symbol = compilation.GetTypeByMetadataName(type.FullName);

            foreach (var member in symbol.GetMembers())
            {
                switch (member)
                {
                    case IEventSymbol @event:
                        break;
                    case IMethodSymbol method:
                        if (method.MethodKind == MethodKind.PropertyGet || 
                            method.MethodKind == MethodKind.PropertySet)
                        {
                            var property = (IPropertySymbol)method.AssociatedSymbol;

                        }
                        else if (method.MethodKind == MethodKind.EventAdd || 
                            method.MethodKind == MethodKind.EventRemove)
                        {
                            var @event = (IEventSymbol)method.AssociatedSymbol;

                        }
                        else
                        {

                        }
                        break;
                    case IPropertySymbol property:
                        break;
                    default:
                        break;
                }


                output.WriteLine(member.Name);
            }

            //var info = new ProxySyntaxInfo(symbol);
        }

        [Theory]
        [InlineData(typeof(ICalculator), "ICalculator")]
        //[InlineData(typeof(int?), "int?")]
        public void FullNameReturnsNamepaces(Type type, string fullName)
        {
            var compilation = CSharpCompilation.Create("codegen")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(asm => File.Exists(asm.ManifestModule.FullyQualifiedName))
                    .Select(asm => MetadataReference.CreateFromFile(asm.ManifestModule.FullyQualifiedName)));

            var symbol = compilation.GetTypeByMetadataName(type.FullName);
            Assert.NotNull(symbol);

            var name = symbol.ToSyntax();

            Assert.Equal(fullName, name.ToFullString());
        }

        [Fact]
        public async void GenerateInterfaceProxy()
        {
            var type = typeof(ICalculator);
            var compilation = CSharpCompilation.Create("codegen")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(asm => File.Exists(asm.ManifestModule.FullyQualifiedName))
                    .Select(asm => MetadataReference.CreateFromFile(asm.ManifestModule.FullyQualifiedName)));

            var symbol = compilation.GetTypeByMetadataName(type.FullName);
            Assert.NotNull(symbol);

            var progress = new Progress<Diagnostic>(d => output.WriteLine(d.GetMessage()));
            var syntax = await new CSharpInterfaceProxyGenerator().GenerateAsync(compilation, progress, CancellationToken.None, symbol);

            AssertCode.Compiles(syntax);
            output.WriteLine(syntax.NormalizeWhitespace().ToFullString());
        }

        [Fact]
        public async void CustomType()
        {
            var type = typeof(System.Security.IPermission);
            var compilation = CSharpCompilation.Create("codegen")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(asm => File.Exists(asm.ManifestModule.FullyQualifiedName))
                    .Select(asm => MetadataReference.CreateFromFile(asm.ManifestModule.FullyQualifiedName)));

            var symbol = compilation.GetTypeByMetadataName(type.FullName);
            Assert.NotNull(symbol);

            var progress = new Progress<Diagnostic>(d => output.WriteLine(d.GetMessage()));
            var syntax = await new CSharpInterfaceProxyGenerator().GenerateAsync(compilation, progress, CancellationToken.None, symbol);

            AssertCode.Compiles(syntax);
            output.WriteLine(syntax.NormalizeWhitespace().ToFullString());
        }

        [Theory(Skip = "Some interfaces are failing.")]
        [MemberData("AllInterfaces")]
        public async void CanGenerateAllProxies(CSharpCompilation compilation, INamedTypeSymbol symbol)
        {
            var progress = new Progress<Diagnostic>(d => output.WriteLine(d.GetMessage()));
            var syntax = await new CSharpInterfaceProxyGenerator().GenerateAsync(compilation, progress, CancellationToken.None, symbol);

            AssertCode.Compiles(syntax);
        }

        public static IEnumerable<object[]> AllInterfaces
        {
            get
            {
                var compilation = CSharpCompilation.Create("codegen")
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Select(asm => MetadataReference.CreateFromFile(asm.ManifestModule.FullyQualifiedName)));

                return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetLoadedTypes)
                    .Where(t => t.IsInterface && !t.IsGenericTypeDefinition)
                    .Select(t => compilation.GetTypeByMetadataName(t.FullName))
                    .Where(s => s != null)
                    .Select(s => (new object[] { compilation, s }))
                    .Take(50);
            }
        }

        static IEnumerable<Type> GetLoadedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (Exception)
            {
                Enumerable.Empty<Type>();
            }

            return Enumerable.Empty<Type>();
        }
    }
}
