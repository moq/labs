using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Moq.StaticProxy.UnitTests
{
    /// <summary>
    /// Runs all the scenarios in the Scenarios folder using the source 
    /// generator to process them.
    /// </summary>
    public class Scenarios
    {
        [Theory]
        [MemberData(nameof(GetScenarios))]
        public void Run(string path)
        {
            var (diagnostics, compilation) = GetGeneratedOutput(
                Path.IsPathRooted(path) ? path :
                Path.Combine(ThisAssembly.Project.MSBuildProjectDirectory, path));

            Assert.Empty(diagnostics);

            var assembly = compilation.Emit();
            var type = assembly.GetTypes().FirstOrDefault(t => typeof(IRunnable).IsAssignableFrom(t));

            Assert.NotNull(type);

            var runnable = (IRunnable)Activator.CreateInstance(type);
            runnable.Run();
        }

        public static IEnumerable<object[]> GetScenarios()
            => Directory.EnumerateFiles(Path.Combine(ThisAssembly.Project.MSBuildProjectDirectory, "Scenarios"), "*.cs")
                .Select(file => new object[] { Path.Combine("Scenarios", Path.GetFileName(file)) });

        static (ImmutableArray<Diagnostic>, Compilation) GetGeneratedOutput(string path)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), 
                path: new FileInfo(path).FullName, 
                encoding: Encoding.UTF8);

            foreach (var name in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                Assembly.Load(name);

            Debug.Assert(System.Threading.Tasks.Task.CompletedTask.IsCompleted);

            var references = new List<MetadataReference>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies.Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location)))
                references.Add(MetadataReference.CreateFromFile(assembly.Location));

            var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(path),
                new SyntaxTree[]
                {
                    syntaxTree,
                    CSharpSyntaxTree.ParseText(File.ReadAllText("Moq/Mock.cs"), 
                        path: new FileInfo("Moq/Mock.cs").FullName, 
                        encoding: Encoding.UTF8),
                    CSharpSyntaxTree.ParseText(File.ReadAllText("Moq/Mock.Overloads.cs"), 
                        path: new FileInfo("Moq/Mock.Overloads.cs").FullName,
                        encoding: Encoding.UTF8),
                    CSharpSyntaxTree.ParseText(File.ReadAllText("Moq/Mock.StaticFactory.cs"),
                        path: new FileInfo("Moq/Mock.StaticFactory.cs").FullName,
                        encoding: Encoding.UTF8),
                }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, 
                    nullableContextOptions: NullableContextOptions.Enable));

            var diagnostics = compilation.GetDiagnostics().RemoveAll(d => 
                d.Severity == DiagnosticSeverity.Hidden || 
                d.Severity == DiagnosticSeverity.Info || 
                // Type conflicts with referenced assembly, will happen because scenarios 
                // are also compiled in the unit test project itself, but also in the scenario 
                // file compilation, but the locally defined in surce wins.
                d.Id == "CS0436");

            if (diagnostics.Any())
                return (diagnostics, compilation);

            ISourceGenerator generator = new MockSourceGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);
            var cts = new CancellationTokenSource(10000);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out diagnostics, cts.Token);

            return (diagnostics, output);
        }
    }
}
