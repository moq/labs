using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Moq.Sdk;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Moq.Proxy.Tests
{
    class ProxyGeneratorEndToEnd
    {
        ITestOutputHelper output;

        public ProxyGeneratorEndToEnd(ITestOutputHelper output) => this.output = output;

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task WhenTypeIsInterface(string language)
        {
            var target = await GenerateProxy<ICalculator>(language);
            var intercepted = false;

            target.AddBehavior((method, next) => { intercepted = true; return next()(method, next); });
            target.AddBehavior(new DefaultValueBehavior());

            target.Add(1, 1);
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            target.Clear("foo");
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            Assert.False(target.IsOn);
            Assert.True(intercepted, "Failed to intercept property getter");

            intercepted = false;
            Assert.Equal(default(CalculatorMode), target.Mode);
            Assert.True(intercepted, "Failed to intercept property getter");

            intercepted = false;
            target.Mode = CalculatorMode.Scientific;
            Assert.True(intercepted, "Failed to intercept property setter");

            intercepted = false;
            target.Recall("foo");
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            target.Store("foo", 1);
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept regular method");

            var x = 0;
            var y = 0;
            var z = 0;

            intercepted = false;
            target.TryAdd(ref x, ref y, out z);
            Assert.True(intercepted);
            Assert.True(intercepted, "Failed to intercept ref/out method");

            intercepted = false;
            target.TurnedOn += (s, e) => { };
            Assert.True(intercepted, "Failed to intercept event add");

            intercepted = false;
            target.TurnedOn -= (s, e) => { };
            Assert.True(intercepted, "Failed to intercept event remove");

            intercepted = false;
            target.TurnOn();
            Assert.True(intercepted, "Failed to intercept regular method");
        }

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task WhenTypeIsAbstract(string language)
        {
            var target = await GenerateProxy<CalculatorBase>(language);
            var intercepted = false;

            target.AddBehavior((method, next) => { intercepted = true; return next()(method, next); });
            target.AddBehavior(new DefaultValueBehavior());

            intercepted = false;
            Assert.False(target.IsOn);
            Assert.True(intercepted);

            intercepted = false;
            target.TurnOn();
            Assert.True(intercepted);

            intercepted = false;
            target.TurnedOn += (s, e) => { };
            if (language == LanguageNames.VisualBasic)
                // Intercepting events doesn't work in VB
                Assert.False(intercepted, "Visual Basic can't intercept virtual events");
            else
                Assert.True(intercepted);

            intercepted = false;
            target.TurnedOn -= (s, e) => { };
            if (language == LanguageNames.VisualBasic)
                // Intercepting events doesn't work in VB
                Assert.False(intercepted, "Visual Basic can't intercept virtual events");
            else
                Assert.True(intercepted);
        }

        [InlineData(LanguageNames.CSharp)]
        //[InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public async Task WhenTypeHasVirtualMembers(string language)
        {
            var target = await GenerateProxy<Calculator>(language).ConfigureAwait(false);
            var intercepted = false;

            target.AddBehavior((method, next) => { intercepted = true; return next()(method, next); });
            target.AddBehavior(new DefaultValueBehavior());

            target.Add(1, 1);
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            target.Clear("foo");
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            Assert.False(target.IsOn);
            Assert.True(intercepted, "Failed to intercept property getter");

            intercepted = false;
            target.Mode = CalculatorMode.Scientific;
            Assert.True(intercepted, "Failed to intercept property setter");

            intercepted = false;
            Assert.Equal(default(CalculatorMode), target.Mode);
            Assert.True(intercepted, "Failed to intercept property getter");

            intercepted = false;
            target.Recall("foo");
            Assert.True(intercepted, "Failed to intercept regular method");

            intercepted = false;
            target.Store("foo", 1);
            Assert.True(intercepted, "Failed to intercept regular method");

            var x = 0;
            var y = 0;
            var z = 0;

            intercepted = false;
            target.TryAdd(ref x, ref y, out z);
            Assert.True(intercepted, "Failed to intercept ref/out method");

            intercepted = false;
            target.TurnedOn += (s, e) => { };
            if (language == LanguageNames.VisualBasic)
                // Intercepting events doesn't work in VB
                Assert.False(intercepted, "Visual Basic can't intercept virtual events");
            else
                Assert.True(intercepted, "Failed to intercept event add");

            intercepted = false;
            target.TurnedOn -= (s, e) => { };
            if (language == LanguageNames.VisualBasic)
                // Intercepting events doesn't work in VB
                Assert.False(intercepted, "Visual Basic can't intercept virtual events");
            else
                Assert.True(intercepted, "Failed to intercept event remove");

            intercepted = false;
            target.TurnOn();
            Assert.True(intercepted, "Failed to intercept regular method");
        }

        async Task<T> GenerateProxy<T>(string language, bool trace = false)
        {
            var type = typeof(T);
            var (workspace, project) = CreateWorkspaceAndProject(language);
            var compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
                new[]
                {
                    compilation.GetTypeByMetadataName(type.FullName),
                });

            var filePath = Path.GetTempFileName();
            project = document.Project;
            var root = await document.GetSyntaxRootAsync();
            var code = root.NormalizeWhitespace().ToFullString();
            File.WriteAllText(filePath, code);

#if DEBUG
            output.WriteLine(filePath);
#endif

            var assemblyName = typeof(T).FullName;

            // Just in case, start from a brand-new workspace and project.
            workspace = new AdhocWorkspace();
            project = workspace.AddProject(CreateProjectInfo(language, assemblyName));
            document = project.AddDocument(typeof(T).FullName + (language == LanguageNames.CSharp ? ".cs" : ".vb"),
                code,
                filePath: filePath);

            project = document.Project;
            compilation = await project.GetCompilationAsync(TimeoutToken(5));
            var assembly = compilation.Emit();
            var proxyType = assembly.GetExportedTypes().FirstOrDefault();

            Assert.NotNull(proxyType);

            var proxy = Activator.CreateInstance(proxyType);

            return (T)proxy;
        }
    }
}
