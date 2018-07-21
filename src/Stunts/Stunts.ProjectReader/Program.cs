using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using StreamJsonRpc;

namespace Stunts
{
    internal class Program
    {
        private static ManualResetEventSlim exit = new ManualResetEventSlim();
        private static bool ready;

        public static int Main(string[] args)
        {
            if (args.Any(s => s.Equals("-d")) || args.Any(s => s.Equals("/d")))
                Debugger.Launch();

            if (args.Length != 0 && Directory.Exists(args[0]))
                // If we receive a path as a first argument, we use that for MSBuild
                MSBuildLocator.RegisterMSBuildPath(args[0]);
            else
                // Otherwise, just use the current directory.
                MSBuildLocator.RegisterMSBuildPath(Directory.GetCurrentDirectory());

            var rpc = new JsonRpc(Console.OpenStandardOutput(), Console.OpenStandardInput(), new Program());
            rpc.StartListening();

            // Force resolving right-away.
            Task.Run(Init);

            exit.Wait();

            return 0;
        }

        private static async Task Init()
        {
            var workspace = MSBuildWorkspace.Create();
            await workspace.OpenProjectAsync(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName), "Empty.csproj"));
            ready = true;
        }

        private MSBuildWorkspace workspace;

        public void Debug() => Debugger.Launch();

        public void Exit() => exit.Set();

        public bool Ping() => ready;

        public void CreateWorkspace(Dictionary<string, string> properties)
        {
            workspace?.Dispose();
            workspace = MSBuildWorkspace.Create(properties);
        }

        public void CloseWorkspace() => workspace?.Dispose();

        public async Task<object> OpenProject(string projectFile)
        {
            if (workspace == null)
                CreateWorkspace(new Dictionary<string, string>());

            var project = workspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == projectFile) ??
                await workspace.OpenProjectAsync(projectFile);
            var references = project.MetadataReferences.OfType<PortableExecutableReference>().ToList();

            return new
            {
                project.Id.Id,
                project.Name,
                project.AssemblyName,
                project.Language,
                project.FilePath,
                project.OutputFilePath,
                CompilationOptions = new
                {
                    project.CompilationOptions.OutputKind,
                    project.CompilationOptions.Platform
                },
                ProjectReferences = project.ProjectReferences
                    .Where(x => workspace.CurrentSolution.ProjectIds.Contains(x.ProjectId))
                    .Select(x => workspace.CurrentSolution.Projects.First(p => p.Id == x.ProjectId).FilePath)
                    .ToArray(),
                MetadataReferences = references.Select(x => x.FilePath).ToArray(),
                Documents = project.Documents
                    .Select(x => new
                    {
                        x.FilePath,
                        x.Folders
                    })
                    .ToArray(),
                AdditionalDocuments = project.AdditionalDocuments
                    .Select(x => new
                    {
                        x.FilePath,
                        x.Folders
                    })
                    .ToArray()
            };
        }
    }
}
