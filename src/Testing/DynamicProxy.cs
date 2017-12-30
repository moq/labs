using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Stunts;
using Xunit;
using static TestHelpers;

static class DynamicProxy
{
    static AdhocWorkspace workspace = new AdhocWorkspace();

    public static Task<T> Of<T>(string language, bool save = false)
        => Of<T>(language, new Type[0], save);

    public static Task<T> Of<T>(string language, Type extraInterface, bool save = false)
        => Of<T>(language, new[] { extraInterface }, save);

    public static async Task<T> Of<T>(string language, Type[] interfaces, bool save = false)
    {
        var type = typeof(T);
        var projectInfo = CreateProjectInfo(language, type.FullName + "_Source");
        var project = workspace.AddProject(projectInfo);
        var compilation = await project.GetCompilationAsync(TimeoutToken(5));
        var document = await new ProxyGenerator().GenerateProxyAsync(workspace, project, TimeoutToken(5),
            new[]
            {
                compilation.GetTypeByMetadataName(type.FullName),
            }
            .Concat(
                interfaces.Select(i => compilation.GetTypeByMetadataName(i.FullName))
            )
            .ToArray());

        project = document.Project;
        var filePath = default(string);
        if (save)
        {
            filePath = typeof(T).FullName + (language == LanguageNames.CSharp ? ".cs" : ".vb");
            var root = await document.GetSyntaxRootAsync();
            var code = root.NormalizeWhitespace().ToFullString();
            File.WriteAllText(filePath, code);
        }

        // Just in case, start from a brand-new project.
        document = workspace
            .AddProject(CreateProjectInfo(language, type.FullName))
            .AddDocument(type.FullName,
                await document.GetSyntaxRootAsync(),
                filePath: filePath);

        compilation = await document.Project.GetCompilationAsync(TimeoutToken(5));
        Assembly assembly;

        if (save)
        {
            var assemblyFile = type.FullName + ".dll";
            var result = compilation.Emit(assemblyFile);
            result.AssertSuccess();
            assembly = Assembly.LoadFrom(assemblyFile);
        }
        else
        {
            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                result.AssertSuccess();
                stream.Seek(0, SeekOrigin.Begin);
                assembly = Assembly.Load(stream.ToArray());
            }
        }

        var proxyType = assembly.GetExportedTypes().FirstOrDefault();
        Assert.NotNull(proxyType);

        var proxy = Activator.CreateInstance(proxyType);
        return (T)proxy;
    }
}