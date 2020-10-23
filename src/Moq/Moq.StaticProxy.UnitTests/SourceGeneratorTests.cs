using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using Moq.Sdk;
using TypeNameFormatter;
using Xunit;

namespace Stunts.UnitTests
{
    public class SourceGeneratorTests
    {
        [Fact]
        public void GeneratesOneMockPerType()
        {
            var code = @"
using System;
using Moq;

namespace UnitTests
{
    public class Test
    {
        public void Do()
        {
            var mock = Mock.Of<IDisposable>();
            var services = Mock.Of<IServiceProvider>();

            Console.WriteLine(mock.ToString());
        }
        
        public void DoToo()
        {
            var other = Mock.Of<IDisposable>();
            var sp = Mock.Of<IServiceProvider>();
        }
    }
}";

            var (diagnostics, compilation) = GetGeneratedOutput(code);

            Assert.Empty(diagnostics);
            
            var assembly = compilation.Emit();

            Assert.NotNull(assembly.GetType(MockNaming.GetFullName(typeof(IDisposable))));
            Assert.NotNull(assembly.GetType(MockNaming.GetFullName(typeof(IServiceProvider))));
        }

        [InlineData(typeof(IDisposable), typeof(IServiceProvider), typeof(IFormatProvider))]
        [InlineData(typeof(ICollection<string>), typeof(IDisposable))]
        [InlineData(typeof(IDictionary<IReadOnlyCollection<string>, IReadOnlyList<int>>), typeof(IDisposable))]
        [InlineData(typeof(IDisposable))]
        [Theory]
        public void GenerateCode(params Type[] types)
        {
            var code = @"
using System;
using Moq;

namespace UnitTests
{
    public class Test
    {
        public void Do()
        {
            var mock = Mock.Of<$$>();
            Console.WriteLine(mock.ToString());
        }
    }
}".Replace("$$", string.Join(", ", types.Select(t =>
                     t.GetFormattedName(TypeNameFormatOptions.Namespaces))));

            var (diagnostics, compilation) = GetGeneratedOutput(code);

            Assert.Empty(diagnostics);

            var assembly = compilation.Emit();

            var name = MockNaming.GetFullName(types.First(), types.Skip(1).ToArray());
            var type = assembly.GetType(name);

            Assert.NotNull(type);

            var stunt = Activator.CreateInstance(type!);

            foreach (var iface in types)
            {
                Assert.IsAssignableFrom(type, stunt);
            }
        }

        static (ImmutableArray<Diagnostic>, Compilation) GetGeneratedOutput(string source, [CallerMemberName] string? test = null)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source, path: test + ".cs");

            var references = new List<MetadataReference>();
            // Force-load Moq.dll, Mock.Sdk and Stunts
            Debug.WriteLine(MockBehavior.Default);
            Debug.WriteLine(typeof(IMockBehavior));
            Debug.WriteLine(typeof(IStunt));

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }

            var compilation = CSharpCompilation.Create(test,
                new SyntaxTree[]
                {
                    syntaxTree,
                    CSharpSyntaxTree.ParseText(File.ReadAllText("Moq/Mock.cs"), path: "Mock.cs"),
                    CSharpSyntaxTree.ParseText(File.ReadAllText("Moq/Mock.Overloads.cs"), path: "Mock.Overloads.cs"),
                }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Any())
                return (diagnostics, compilation);

            ISourceGenerator generator = new MockSourceGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics);

            return (diagnostics, output);
        }
    }
}
