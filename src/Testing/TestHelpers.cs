using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
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
        var netstandardPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @".nuget\packages\NETStandard.Library\2.0.0\build\netstandard2.0\ref");
        if (!Directory.Exists(netstandardPath))
            netstandardPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref");
        if (!Directory.Exists(netstandardPath))
            netstandardPath = @"C:\Program Files\dotnet\sdk\NuGetFallbackFolder\netstandard.library\2.0.0\build\netstandard2.0\ref";

        if (!Directory.Exists(netstandardPath))
            throw new InvalidOperationException("Failed to find location of .NETStandard 2.0 reference assemblies");

        var referencePaths =
            Directory.EnumerateFiles(netstandardPath, "*.dll")
#pragma warning disable CS0436 // Type conflicts with imported type
            .Concat(ThisAssembly.Metadata.ReferencePaths.Split('|'))
#pragma warning restore CS0436 // Type conflicts with imported type
            .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path))
            .Distinct(FileNameEqualityComparer.Default);

        var projectId = ProjectId.CreateNewId();
        var documents = new List<DocumentInfo>();

        if (includeStuntApi)
        {
            var stuntApi = language == LanguageNames.CSharp ?
                new FileInfo(@"contentFiles\cs\netstandard1.3\Stunts\Stunt.cs").FullName :
                new FileInfo(@"contentFiles\vb\netstandard1.3\Stunts\Stunt.vb").FullName;

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
                new FileInfo(@"contentFiles\cs\netstandard2.0\Mocks\Mock.cs").FullName :
                new FileInfo(@"contentFiles\vb\netstandard2.0\Mocks\Mock.vb").FullName;
            var mockApiOverloads = Path.ChangeExtension(mockApi, ".Overloads") + Path.GetExtension(mockApi);

            Assert.True(File.Exists(mockApi));

            documents.Add(DocumentInfo.Create(
                DocumentId.CreateNewId(projectId, Path.GetFileName(mockApi)),
                Path.GetFileName(mockApi),
                loader: new FileTextLoader(mockApi, Encoding.Default),
                filePath: mockApi));
            documents.Add(DocumentInfo.Create(
                DocumentId.CreateNewId(projectId, Path.GetFileName(mockApiOverloads)),
                Path.GetFileName(mockApiOverloads),
                loader: new FileTextLoader(mockApiOverloads, Encoding.Default),
                filePath: mockApiOverloads));
        }

        var projectDir = Path.Combine(Path.GetTempPath(), "Mock", projectId.Id.ToString());

        return ProjectInfo.Create(
            projectId, 
            VersionStamp.Create(),  
            assemblyName + "." + suffix,
            assemblyName + "." + suffix,
            language,
            filePath: language == LanguageNames.CSharp 
                ? Path.Combine(projectDir, "code.csproj") 
                : Path.Combine(projectDir, "code.vbproj"),
            compilationOptions: options,
            parseOptions: parse,
            metadataReferences: referencePaths
                .Select(path => MetadataReference.CreateFromFile(path)),
            documents: documents.ToArray());
    }

    public static CancellationToken TimeoutToken(int seconds)
        => Debugger.IsAttached ?
            CancellationToken.None :
            new CancellationTokenSource(TimeSpan.FromSeconds(seconds)).Token;

    public static Document AddDocument(this AdhocWorkspace workspace, Project project, string content, string fileName = "code.cs")
        => workspace.AddDocument(DocumentInfo.Create(
            DocumentId.CreateNewId(project.Id),
            "code.cs",
            loader: TextLoader.From(TextAndVersion.Create(SourceText.From(content), VersionStamp.Create()))));

    public static async Task<Document> ApplyCodeFixAsync<TDiagnosticAnalyzer, TCodeFixProvider>(this Workspace workspace, Document document, string diagnosticId)
        where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFixProvider : CodeFixProvider, new()
    {
        var compilation = await document.Project.GetCompilationAsync(TimeoutToken(5));
        var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new TDiagnosticAnalyzer()));
        var diagnostic = (await withAnalyzers.GetAnalyzerDiagnosticsAsync())
            .Where(d => d.Id == diagnosticId)
            .OrderBy(d => d.Location.SourceSpan.Start)
            .FirstOrDefault();

        Assert.NotNull(diagnostic);

        var provider = new TCodeFixProvider();
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), TimeoutToken(5));

        await provider.RegisterCodeFixesAsync(context);

        var applyChanges = actions
            .SelectMany(x => x.GetOperationsAsync(TimeoutToken(10)).Result)
            .OfType<ApplyChangesOperation>()
            .FirstOrDefault();

        Assert.NotNull(applyChanges);

        applyChanges.Apply(workspace, TimeoutToken(5));

        // According to https://github.com/DotNetAnalyzers/StyleCopAnalyzers/pull/935 and 
        // https://github.com/dotnet/roslyn-sdk/issues/140, Sam Harwell mentioned that we should 
        // be forcing a re-parse of the document syntax tree at this point. 
        return await workspace.CurrentSolution.GetDocument(document.Id).RecreateDocumentAsync(TimeoutToken(2));
    }

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