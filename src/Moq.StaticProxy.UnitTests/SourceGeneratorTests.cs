using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public void GeneratesRecursiveMockForProperties()
        {
            var code = @"
using Moq;

namespace UnitTests
{
    public class Test
    {
        public void Do()
        {
            var mock = Mock.Of<IFoo>();
            
            mock.Bar.Baz.Name.Returns(""hi"");
        }
    }

    public interface IFoo 
    {        
        IBar Bar { get; }
    }
    
    public interface IBar 
    {
        IBaz Baz { get; }
    }

    public interface IBaz 
    {
        string Name { get; }
    }
}";

            var (diagnostics, compilation) = GetGeneratedOutput(code);

            Assert.Empty(diagnostics);

            var assembly = compilation.Emit();

            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IFoo" + MockNaming.DefaultSuffix));
            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IBar" + MockNaming.DefaultSuffix));
            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IBaz" + MockNaming.DefaultSuffix));
        }

        [Fact]
        public void GeneratesRecursiveMockForMethods()
        {
            var code = @"
using Moq;

namespace UnitTests
{
    public class Test
    {
        public void Do()
        {
            var mock = Mock.Of<IFoo>();
            
            mock.GetBar().GetBaz().Name.Returns(""hi"");
        }
    }

    public interface IFoo 
    {        
        IBar GetBar();
    }
    
    public interface IBar 
    {
        IBaz GetBaz();
    }

    public interface IBaz 
    {
        string Name { get; }
    }
}";

            var (diagnostics, compilation) = GetGeneratedOutput(code);

            Assert.Empty(diagnostics);

            var assembly = compilation.Emit();

            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IFoo" + MockNaming.DefaultSuffix));
            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IBar" + MockNaming.DefaultSuffix));
            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IBaz" + MockNaming.DefaultSuffix));
        }

        [Fact]
        public void GeneratesRecursiveMockSetup()
        {
            var code = @"
using Moq;

namespace UnitTests
{
    public class Test
    {
        public void Do()
        {
            var mock = Mock.Of<IFoo>();

            mock.Setup(x => x.GetBar().Baz.Name).Returns(""hi"");            
        }
    }

    public interface IFoo 
    {        
        IBar GetBar();
    }
    
    public interface IBar 
    {
        IBaz Baz { get; }
    }

    public interface IBaz 
    {
        string Name { get; }
    }
}";

            var (diagnostics, compilation) = GetGeneratedOutput(code);

            Assert.Empty(diagnostics);

            var assembly = compilation.Emit();

            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IFoo" + MockNaming.DefaultSuffix));
            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IBar" + MockNaming.DefaultSuffix));
            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IBaz" + MockNaming.DefaultSuffix));
        }

        [Fact]
        public void GeneratesRecursiveMockSetupScope()
        {
            var code = @"
using Moq;

namespace UnitTests
{
    public class Test
    {
        public void Do()
        {
            var mock = Mock.Of<IFoo>();
            using (new SetupScope())
            {
                mock.GetBar().Baz.Name.Returns(""hi"");
            }
        }
    }

    public interface IFoo 
    {        
        IBar GetBar();
    }
    
    public interface IBar 
    {
        IBaz Baz { get; }
    }

    public interface IBaz 
    {
        string Name { get; }
    }
}";

            var (diagnostics, compilation) = GetGeneratedOutput(code);

            Assert.Empty(diagnostics);

            var assembly = compilation.Emit();

            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IFoo" + MockNaming.DefaultSuffix));
            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IBar" + MockNaming.DefaultSuffix));
            Assert.NotNull(assembly.GetType(MockNaming.DefaultRootNamespace + ".UnitTests.IBaz" + MockNaming.DefaultSuffix));
        }

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

            var diagnostics = compilation.GetDiagnostics().RemoveAll(d => d.Severity == DiagnosticSeverity.Hidden || d.Severity == DiagnosticSeverity.Info);
            if (diagnostics.Any())
                return (diagnostics, compilation);

            ISourceGenerator generator = new MockSourceGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics);

            return (diagnostics, output);
        }
    }
}
