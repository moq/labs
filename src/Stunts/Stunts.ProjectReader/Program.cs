using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Mono.Options;

namespace Stunts
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var help = false;
            var instance = default(VisualStudioInstance);
            var msbuild = default(string);
            var wait = false;
            var project = default(string);
            var properties = new Dictionary<string, string>();
            var configuration = default(string);
            var platform = default(string);
            var output = default(string);

            var options = new OptionSet
            {
                { "p|project=", "MSBuild project file to read", p => project = p },
                { "i|installation:", "Visual Studio installation directory", v => instance =
                    MSBuildLocator.QueryVisualStudioInstances(new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.VisualStudioSetup })
                    .FirstOrDefault(x => x.VisualStudioRootPath.Equals(v, StringComparison.OrdinalIgnoreCase)) ??
                    throw new ArgumentException($"Did not find a Visual Studio installation at {v}.") },
                { "m|msbuild:", "MSBuild root directory.", m => msbuild = m },
                { "c|configuration=", "Project configuration (i.e. Debug or Release)", c => configuration = c },
                { "t|platform=", "Project target platform (i.e. AnyCPU)", plat => platform = plat },
                { "o|output:", "Optional output file to emit. Emits to standard output if not specified", o => output = o },
                { "<>", "Additional MSBuild global properties to set", v =>
                    {
                        var values = v.Split(new[] { '=', ':' }, 2);
                        properties[values[0].Trim()] = values[1].Trim().Trim('\"');
                    }
                },
                new ResponseFileSource(),
                { "d|debug", "Attach a debugger to the process", d => Debugger.Launch() },
                { "w|wait", "Wait before exiting the process", w => wait = w != null },
                { "h|?|help", "Show this message and exit", h => help = true },
            };

            List<string> extra;
            var appName = Path.GetFileName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);

            try
            {
                extra = options.Parse(args);

                if (help ||
                    string.IsNullOrEmpty(project) ||
                    (instance == null && msbuild == null))
                {
                    var writer = help ? Console.Out : Console.Error;

                    // show some app description message
                    writer.WriteLine($"Usage: {appName} [OPTIONS]+");
                    writer.WriteLine();

                    writer.WriteLine("Options:");
                    options.WriteOptionDescriptions(writer);

                    return -1;
                }

                // TODO: Mac support
                if (instance != null)
                    MSBuildLocator.RegisterInstance(instance);
                else if (msbuild != null)
                    MSBuildLocator.RegisterMSBuildPath(msbuild);

                var metadata = await ReadProject(project, properties, output != null);
                if (string.IsNullOrEmpty(output))
                    metadata.Save(Console.Out);
                else
                    metadata.Save(output);

                if (wait)
                    Console.ReadLine();

                return 0;
            }
            catch (OptionException e)
            {
                Console.Error.WriteLine($"{appName}: {e.Message}");
                Console.Error.WriteLine($"Try '{appName} -?' for more information.");

                if (wait)
                    Console.ReadLine();

                return -1;
            }
            catch (Exception e)
            {
#if DEBUG
                if (!Debugger.IsAttached)
                    Debugger.Launch();
#endif

                // TODO: should we render something different in this case? (non-options exception?)
                Console.Error.WriteLine($"{appName}: {e.Message}");

                if (wait)
                    Console.ReadLine();

                return -1;
            }
        }

        /// <summary>
        /// Read the given project file with the specified global 
        /// properties.
        /// </summary>
        private static async Task<XElement> ReadProject(string projectFile, Dictionary<string, string> properties, bool consoleProgress)
        {
            var workspace = MSBuildWorkspace.Create(properties);
            var project = await workspace.OpenProjectAsync(projectFile, consoleProgress ? new ConsoleProgressReporter() : null);
            var references = project.MetadataReferences.OfType<PortableExecutableReference>().ToList();

            return new XElement("Project",
                new XAttribute("Id", project.Id.Id),
                new XAttribute("Name", project.Name),
                new XAttribute("AssemblyName", project.AssemblyName),
                // We may support other languages than C# in our visitors down the road.
                new XAttribute("Language", project.Language),
                new XAttribute("FilePath", project.FilePath),
                new XAttribute("OutputFilePath", project.OutputFilePath),
                new XElement("CompilationOptions",
                    new XAttribute("OutputKind", project.CompilationOptions.OutputKind.ToString()),
                    new XAttribute("Platform", project.CompilationOptions.Platform.ToString())),
                new XElement("ProjectReferences", project.ProjectReferences
                    .Where(x => workspace.CurrentSolution.Projects.Any(p => p.Id == x.ProjectId))
                    .Select(x => new XElement("ProjectReference",
                        new XAttribute("FilePath", workspace.CurrentSolution.Projects.First(p => p.Id == x.ProjectId).FilePath)))),
                new XElement("MetadataReferences", references.Select(x =>
                    new XElement("MetadataReference", new XAttribute("FilePath", x.FilePath)))),
                new XElement("Documents", project.Documents.Select(x =>
                    new XElement("Document",
                        new XAttribute("FilePath", x.FilePath),
                        new XAttribute("Folders", string.Join(Path.DirectorySeparatorChar.ToString(), x.Folders))))),
                new XElement("AdditionalDocuments", project.AdditionalDocuments.Select(x =>
                    new XElement("Document",
                        new XAttribute("FilePath", x.FilePath),
                        new XAttribute("Folders", string.Join(Path.DirectorySeparatorChar.ToString(), x.Folders)))))
            );
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}
