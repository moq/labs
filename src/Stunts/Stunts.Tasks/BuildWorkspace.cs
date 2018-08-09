using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Stunts.Tasks
{
    internal class BuildWorkspace : Workspace
    {
        public static BuildWorkspace GetWorkspace(IBuildConfiguration configuration)
        {
            // TODO: could we keep the workspace alive while building inside VS?
            // Loading everything during build takes a constant 21'' for out stunts 
            // solution, which is unacceptable for every build, unless it's a 
            // command line build.
            var lifetime = RegisteredTaskObjectLifetime.Build;
            var key = typeof(BuildWorkspace).FullName;
            if (!(configuration.BuildEngine4.GetRegisteredTaskObject(key, lifetime) is BuildWorkspace workspace))
            {
                workspace = new BuildWorkspace(configuration);
                configuration.BuildEngine4.RegisterTaskObject(key, workspace, lifetime, false);
            }
            return workspace;
        }

        private readonly IBuildConfiguration configuration;
        private readonly ProjectReader reader;
        private readonly Task initializer;

        public BuildWorkspace(IBuildConfiguration configuration)
            : base(MefHostServices.Create(
                MefHostServices.DefaultAssemblies.Concat(
                        configuration.Analyzers.Select(x => Assembly.LoadFrom(x.GetMetadata("FullPath"))))), 
                "BuildWorkspace")
        {
            this.configuration = configuration;
            // Signals to code fixers that this is a build-time workspace, and therefore 
            // generated code should go into the intermediate output path instead of the 
            // project path.
            Options = Options.WithChangedOption(StuntOptions.IsBuildTime, true);

            reader = configuration.GetProjectReader();

            // Initialize the reader's workspace with the global properties.
            initializer = Task.Run(async () => 
                await reader.CreateWorkspaceAsync(this.configuration.GlobalProperties));
        }

        protected override void Dispose(bool finalize)
        {
            base.Dispose(finalize);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            reader.CloseWorkspaceAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public override bool CanApplyChange(ApplyChangesKind feature)
        {
            switch (feature)
            {
                case ApplyChangesKind.AddProject:
                case ApplyChangesKind.AddProjectReference:
                case ApplyChangesKind.AddDocument:
                case ApplyChangesKind.ChangeDocument:
                    return true;
            }

            return false;
        }

        protected override void ApplyDocumentAdded(DocumentInfo info, SourceText text)
        {
            base.ApplyDocumentAdded(info, text);
            Directory.CreateDirectory(Path.GetDirectoryName(info.FilePath));
            using (var writer = new StreamWriter(info.FilePath, false, text.Encoding ?? Encoding.UTF8))
            {
                text.Write(writer);
            }
            configuration.BuildEngine4.LogMessageEvent(new BuildMessageEventArgs($"{nameof(BuildWorkspace)}.{nameof(ApplyDocumentAdded)}: {info.FilePath}", "", nameof(BuildWorkspace), MessageImportance.Low));
        }

        protected override void ApplyDocumentTextChanged(DocumentId id, SourceText text)
        {
            base.ApplyDocumentTextChanged(id, text);
            var document = CurrentSolution.Projects
                .FirstOrDefault(p => p.Id == id.ProjectId)?
                .GetDocument(id);

            if (document != null)
            {
                using (var writer = new StreamWriter(document.FilePath, false, text.Encoding ?? Encoding.UTF8))
                {
                    text.Write(writer);
                }
                configuration.BuildEngine4.LogMessageEvent(new BuildMessageEventArgs($"{nameof(BuildWorkspace)}.{nameof(ApplyDocumentTextChanged)}: {document.FilePath}", "", nameof(BuildWorkspace), MessageImportance.Low));
            }
        }

        public async Task<Project> GetOrAddProjectAsync(string projectFullPath, Action<MessageImportance, string> log, CancellationToken cancellation)
        {
            projectFullPath = new FileInfo(projectFullPath).FullName;
            // Ensure full project paths always.
            if (!File.Exists(projectFullPath))
                throw new FileNotFoundException("Project file not found.", projectFullPath);

            // We load projects only once.
            var project = FindProjectByPath(projectFullPath);
            if (project != null)
            {
                log(MessageImportance.Low, $"Found existing loaded project for {Path.GetFileName(projectFullPath)}.");
                return project;
            }

            project = await AddProjectAsync(projectFullPath, log, cancellation);

            TryApplyChanges(CurrentSolution);

            return project;
        }

        private async Task<Project> AddProjectAsync(string projectFullPath, Action<MessageImportance, string> log, CancellationToken cancellation)
        {
            await initializer;

            var watch = Stopwatch.StartNew();

            var metadata = await reader.OpenProjectAsync(projectFullPath);
            log(MessageImportance.Low, $"Read '{Path.GetFileName(projectFullPath)}' metadata in {watch.Elapsed.TotalSeconds} seconds.");

            var project = await AddProjectFromMetadataAsync(metadata, log, cancellation);

            return project;
        }

        private async Task<Project> AddProjectFromMetadataAsync(dynamic projectMetadata, Action<MessageImportance, string> log, CancellationToken cancellation)
        {
            var watch = Stopwatch.StartNew();

            var projectFile = (string)projectMetadata.FilePath;
            var language = (string)projectMetadata.Language;
            var output = (OutputKind)Enum.Parse(typeof(OutputKind), (string)projectMetadata.CompilationOptions.OutputKind);
            var platform = (Platform)Enum.Parse(typeof(Platform), (string)projectMetadata.CompilationOptions.Platform);

            // Iterate the references of the msbuild project
            var referencesToAdd = new List<ProjectReference>();
            foreach (var projectReference in projectMetadata.ProjectReferences)
            {
                var referencedProject = FindProjectByPath((string)projectReference.FilePath);
                if (referencedProject != null)
                {
                    log(MessageImportance.Low, $"Found existing loaded project for {Path.GetFileName((string)projectReference.FilePath)}.");
                } 
                else
                {
                    referencedProject = await AddProjectFromMetadataAsync(projectReference, log, cancellation);
                }

                referencesToAdd.Add(new ProjectReference(referencedProject.Id));
            }

            OnProjectAdded(
                ProjectInfo.Create(
                    ProjectId.CreateFromSerialized(new Guid((string)projectMetadata.Id)),
                    VersionStamp.Default,
                    (string)projectMetadata.Name,
                    (string)projectMetadata.AssemblyName,
                    language,
                    projectFile,
                    outputFilePath: (string)projectMetadata.OutputFilePath,
                    metadataReferences: ((IEnumerable)projectMetadata.MetadataReferences)
                        .Cast<object>()
                        .Select(x => MetadataReference.CreateFromFile(x.ToString())),
                    // Switch compilation options depending on language.
                    compilationOptions: language == LanguageNames.CSharp 
                        ? (CompilationOptions)new CSharpCompilationOptions(output, platform: platform)
                        : (CompilationOptions)new VisualBasicCompilationOptions(output, platform: platform), 
                    parseOptions: (language == LanguageNames.CSharp
                        ? (ParseOptions)new CSharpParseOptions(documentationMode: DocumentationMode.None)
                        : (ParseOptions)new VisualBasicParseOptions(documentationMode: DocumentationMode.None))
                        // \o/: Allows code fixes to know the intermediate output path for generated code.
                        .WithFeatures(new[] { new KeyValuePair<string, string>("IntermediateOutputPath", (string)projectMetadata.IntermediateOutputPath) })
                )
            );

            cancellation.ThrowIfCancellationRequested();

            // Add the documents to the workspace
            foreach (var document in projectMetadata.Documents)
            {
                AddDocument(projectFile, document, isAdditionalDocument: false);
                cancellation.ThrowIfCancellationRequested();
            }

            foreach (var document in projectMetadata.AdditionalDocuments)
            {
                AddDocument(projectFile, document, isAdditionalDocument: true);
                cancellation.ThrowIfCancellationRequested();
            }

            // Fix references

            cancellation.ThrowIfCancellationRequested();

            if (referencesToAdd.Count > 0)
            {
                var addedProject = FindProjectByPath(projectFile);

                TryApplyChanges(CurrentSolution.WithProjectReferences(addedProject.Id, referencesToAdd));
            }

            log(MessageImportance.Low, $"Added '{projectMetadata.Name}' to workspace in {watch.Elapsed.TotalSeconds} seconds.");

            return FindProjectByPath(projectFile);
        }

        private void AddDocument(string projectPath, dynamic document, bool isAdditionalDocument)
        {
            var documentPath = (string)document.FilePath;
            var project = FindProjectByPath(projectPath);
            SourceText text;
            using (var reader = new StreamReader(documentPath))
            {
                text = SourceText.From(reader.BaseStream);
            }

            var documentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                Path.GetFileName(documentPath),
                loader: TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create(), documentPath)),
                folders: ((IEnumerable)document.Folders).OfType<object>().Select(x => x.ToString()),
                filePath: documentPath);

            if (isAdditionalDocument)
            {
                OnAdditionalDocumentAdded(documentInfo);
            }
            else
            {
                OnDocumentAdded(documentInfo);
            }
        }

        private Project FindProjectByPath(string projectPath)
            => CurrentSolution.Projects
                .Where(x => string.Equals(x.FilePath, projectPath, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
    }
}
