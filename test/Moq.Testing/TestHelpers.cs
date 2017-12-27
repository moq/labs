using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.VisualBasic;
using Xunit;

static partial class TestHelpers
{
    public static (AdhocWorkspace workspace, Project project) CreateWorkspaceAndProject(string language, string assemblyName = "Code", bool includeStuntApi = true, bool includeMockApi = false)
    {
        var workspace = new AdhocWorkspace();
        var projectInfo = CreateProjectInfo(language, assemblyName, includeStuntApi, includeMockApi);
        var project = workspace.AddProject(projectInfo);

        return (workspace, project);
    }

    public static ProjectInfo CreateProjectInfo(string language, string assemblyName, bool includeStuntApi = true, bool includeMockApi = false)
    {
        var suffix = language == LanguageNames.CSharp ? "CS" : "VB";
        var options = language == LanguageNames.CSharp ?
                (CompilationOptions)new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default) :
                (CompilationOptions)new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optionStrict: OptionStrict.On, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default);
        var parse = language == LanguageNames.CSharp ?
                (ParseOptions)new CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest) :
                (ParseOptions)new VisualBasicParseOptions(Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.Latest);

        //The location of the .NET assemblies
        var frameworkPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        var netstandardPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".nuget\packages\NETStandard.Library\2.0.0\build\netstandard2.0\ref");
        if (!Directory.Exists(netstandardPath))
            netstandardPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref");
        if (!Directory.Exists(netstandardPath))
            netstandardPath = @"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref";

        if (!Directory.Exists(netstandardPath))
            throw new InvalidOperationException("Failed to find location of .NETStandard 2.0 reference assemblies");

        //var referencePaths = new[]
        //{
        //    //typeof(object).Assembly.Location,
        //    //typeof(Enumerable).Assembly.Location,
        //    typeof(Stunts.IStunt).Assembly.Location,
        //    //typeof(Moq.Sdk.IMock).Assembly.Location,
        //    Assembly.GetExecutingAssembly().Location,
        //    //typeof(LazyInitializer).Assembly.Location,

        //    typeof(System.Object).Assembly.Location,                                     //mscorlib
        //    typeof(System.Composition.ExportAttribute).Assembly.Location,                //System.Composition.AttributeModel
        //    typeof(System.Composition.Convention.ConventionBuilder).Assembly.Location,   //System.Composition.Convention
        //    typeof(System.Composition.Hosting.CompositionHost).Assembly.Location,        //System.Composition.Hosting
        //    typeof(System.Composition.CompositionContext).Assembly.Location,             //System.Composition.Runtime
        //    typeof(System.Composition.CompositionContextExtensions).Assembly.Location,   //System.Composition.TypedParts
        //    typeof(System.CodeDom.Compiler.CodeCompiler).Assembly.Location,              //System.CodeDom.Compiler

        //    Path.Combine(frameworkPath, "System.dll"),
        //    Path.Combine(frameworkPath, "System.Core.dll"),
        //    Path.Combine(frameworkPath, "System.Runtime.dll"),

        //    @"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref\netstandard.dll",
        //    @"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref\System.Runtime.dll",
        //    @"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref\System.Reflection.dll",
        //    //@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref\System.Threading.dll",
        //    //@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref\System.Threading.Tasks.dll",
        //};

        var referencePaths =
            Directory.EnumerateFiles(netstandardPath, "*.dll")
            .Concat(ReferencePaths.Paths)
            //ReferencePaths.Paths
            //.Concat(Directory.EnumerateFiles(netstandardPath, "*.dll"))
            .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path))
            .Distinct(FileNameEqualityComparer.Default);

        var projectId = ProjectId.CreateNewId();
        var documents = new List<DocumentInfo>();

        if (includeStuntApi)
        {
            var stuntApi = language == LanguageNames.CSharp ?
                new FileInfo(@"..\..\..\..\src\Stunts\Stunts\contentFiles\cs\netstandard1.3\Stunt.cs").FullName :
                new FileInfo(@"..\..\..\..\src\Stunts\Stunts\contentFiles\vb\netstandard1.3\Stunt.vb").FullName;

            Assert.True(File.Exists(stuntApi));
            documents.Add(DocumentInfo.Create(
                DocumentId.CreateNewId(projectId, Path.GetFileName(stuntApi)),
                Path.GetFileName(stuntApi),
                loader: new FileTextLoader(stuntApi, Encoding.Default),
                filePath: stuntApi));
        }

        if (includeMockApi)
        {
            var mockApi = language == LanguageNames.CSharp ?
                new FileInfo(@"..\..\..\..\src\Moq\Moq\contentFiles\cs\netstandard1.3\Mock.cs").FullName :
                new FileInfo(@"..\..\..\..\src\Moq\Moq\contentFiles\vb\netstandard1.3\Mock.vb").FullName;

            Assert.True(File.Exists(mockApi));
            documents.Add(DocumentInfo.Create(
                DocumentId.CreateNewId(projectId, Path.GetFileName(mockApi)),
                Path.GetFileName(mockApi),
                loader: new FileTextLoader(mockApi, Encoding.Default),
                filePath: mockApi));
        }

        return ProjectInfo.Create(
            projectId,
            VersionStamp.Create(),  
            assemblyName + "." + suffix,
            assemblyName + "." + suffix,
            language,
            compilationOptions: options,
            parseOptions: parse,
            metadataReferences: referencePaths
                .Select(path => MetadataReference.CreateFromFile(path)),
            documents: documents.ToArray());
    }

    public static CancellationToken TimeoutToken(int seconds)
        => Debugger.IsAttached ?
            new CancellationTokenSource().Token :
            new CancellationTokenSource(TimeSpan.FromSeconds(seconds)).Token;

    public static Assembly Emit(this Compilation compilation)
    {
        using (var stream = new MemoryStream())
        {
            var result = compilation.Emit(stream);
            result.AssertSuccess();

            stream.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(stream.ToArray());
        }
    }

    public static void AssertSuccess(this EmitResult result)
    {
        if (!result.Success)
        {
            Assert.False(true,
                "Emit failed:\r\n" +
                Environment.NewLine +
                string.Join(Environment.NewLine, result.Diagnostics.Select(d => d.ToString())));
        }
    }

    public class FileNameEqualityComparer : IEqualityComparer<string>
    {
        public static IEqualityComparer<string> Default { get; } = new FileNameEqualityComparer();

        FileNameEqualityComparer() { }

        public bool Equals(string x, string y) => Path.GetFileName(x).Equals(Path.GetFileName(y));

        public int GetHashCode(string obj) => Path.GetFileName(obj).GetHashCode();
    }
}