using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xamarin.Build;

namespace Stunts.Tasks
{
    public class GenerateStunts : AsyncTask, IBuildConfiguration
    {
        [Required]
        public string ProjectFullPath { get; set; }

        /// <summary>
        /// Gets or sets the paths to directories to search for dependencies.
        /// </summary>
        [Required]
        public ITaskItem[] AssemblySearchPath { get; set; } 

        [Required]
        public string MSBuildBinPath { get; set; }

        [Required]
        public string ToolsPath { get; set; }

        [Required]
        public ITaskItem[] FixableDiagnosticIds { get; set; }

        //[Required]
        public ITaskItem[] Analyzers { get; set; }

        public bool BuildingInsideVisualStudio { get; set; }

        /// <summary>
        /// Whether to debug debug the task for troubleshooting purposes.
        /// </summary>
        public bool DebugTask { get; set; }

        /// <summary>
        /// Whether to cause the console program to launch a debugger on run 
        /// for troubleshooting purposes.
        /// </summary>
        public bool DebugConsole { get; set; }

        [Output]
        public ITaskItem[] GeneratedFiles { get; set; }

        IDictionary<string, string> IBuildConfiguration.GlobalProperties => BuildEngine.GetGlobalProperties();

        public override bool Execute()
        {
            using (var resolver = new AssemblyResolver(AssemblySearchPath, (i, m) => Log.LogMessage(i, m)))
            {
                Task.Run(ExecuteAsync).ConfigureAwait(false);
                return base.Execute();
            }
        }

        private async Task ExecuteAsync()
        {
            if (DebugTask)
            {
                Debugger.Launch();
            }

            try
            {
                var watch = Stopwatch.StartNew();
                var workspace = this.GetWorkspace();
                var project = await workspace.GetOrAddProjectAsync(ProjectFullPath, (i, m) => LogMessage(m, i), Token);
                watch.Stop();
                
                LogMessage($"Loaded {project.Name} in {watch.Elapsed.TotalSeconds} seconds");

                var compilation = await project.GetCompilationAsync(Token);
                var analyzers = Analyzers
                    .Select(x => Assembly.LoadFrom(x.GetMetadata("FullPath")))
                    .SelectMany(x => x.GetExportedTypes())
                    .Where(t => !t.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
                    .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                    .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                    .ToImmutableArray();

                var analyzed = compilation.WithAnalyzers(analyzers);
                var diagnostics = await analyzed.GetAnalyzerDiagnosticsAsync(Token);
                // By grouping by target type name, we avoid running the same generators multiple times.
                var group = diagnostics.Where(d => d.Properties.ContainsKey("TargetFullName")).GroupBy(d => d.Properties["TargetFullName"]);
                // We filter all available codefix providers to only those that support the project language and can 
                // fix any of the generator diagnostic codes we received. We don't use the providers from this 
                // retrieval and instead just let the DocumentExtensions.ApplyCodeFixAsync retrieve it by name 
                // since that code is also used elsewhere for the design-time codegen, so it was better to just 
                // reuse that.
                var providers = (from provider in project.Solution.Workspace.Services.HostServices.GetExports<CodeFixProvider, IDictionary<string, object>>()
                                 let languages = (string[])provider.Metadata["Languages"]
                                 let name = provider.Metadata.TryGetValue("Name", out var value) ? (string)value : ""
                                 where 
                                    !string.IsNullOrEmpty(name) && 
                                    languages.Contains(project.Language) && 
                                    FixableDiagnosticIds.Any(id => provider.Value.FixableDiagnosticIds.Contains(id.ItemSpec))
                                 select new { Name = name, Provider = provider.Value }
                                ).ToArray();

                foreach (var diag in group)
                {
                    var diagnostic = diag.First();
                    // Re-fetching the project and the document since previous TryApplyChanges to the 
                    // workspace will make the outdated immutable projects invalid for further TryApplyChanges
                    project = workspace.CurrentSolution.GetProject(project.Id);
                    // Even though the diagnostics was retrieved for a previous "version" of the solution/project, 
                    // retrieving the document from its SourceTree still works.
                    var document = project.GetDocument(diagnostic.Location.SourceTree);
                    Debug.Assert(document != null, string.Format("Failed to locate the corresponding document at {0} for diagnostics {1}:{2}.", 
                        diagnostic.Location.SourceTree.FilePath, diagnostic.Id, diagnostic.GetMessage()));

                    // There should be just one for each diagnostic id, but maybe not?
                    foreach (var provider in providers.Where(p => p.Provider.FixableDiagnosticIds.Contains(diagnostic.Id)))
                    {
                        document = await document.ApplyCodeFixAsync(provider.Name, analyzers, Token);
                    }

                    // After each document is modified by the generators, we want to apply the changes 
                    // so that the workspace writes the files to the right locations before moving on.
                    Debug.Assert(document.Project.Solution.Workspace.TryApplyChanges(document.Project.Solution), "Failed to apply changes to the workspace.");
                }
            }
            catch (Exception e)
            {
                LogErrorFromException(e);
            }
            finally
            {
                Complete();
            }
        }
    }
}
