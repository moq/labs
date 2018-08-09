using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Mono.Options;
using Newtonsoft.Json;
using StreamJsonRpc;

namespace Stunts
{
    internal class Program
    {
        private static ManualResetEventSlim exit = new ManualResetEventSlim();
        private static bool ready;
        private static Task initializer = Task.CompletedTask;

        public static async Task<int> Main(string[] args)
        {
            var help = false;
            var msbuild = default(string);
            var wait = false;
            var parent = default(Process);
            var project = default(string);
            var properties = new Dictionary<string, string>();
            var output = default(string);
            var parentId = 0;
            var parentNotFound = false;

            var options = new OptionSet
            {
                { "m|msbuild=", "MSBuild root directory.", m => msbuild = m },
                { "p|project:", "MSBuild project file to read", s => project = s },
                { "o|output:", "Output file to emit. Emits to standard output if not specified", o => output = o },
                { "<>", "Additional MSBuild global properties to set", v =>
                    {
                        var values = v.Split(new[] { '=', ':' }, 2);
                        properties[values[0].Trim()] = values[1].Trim().Trim('\"');
                    }
                },
                new ResponseFileSource(),
                { "parent:", "Parent process ID to monitor to automatically exit", (int p) =>
                    {
                        try
                        {
                            parentId = p;
                            parent = Process.GetProcessById(p);
                        }
                        catch (ArgumentException)
                        {
                            parentNotFound = true;
                        }
                    }
                },
                { "w|wait", "Wait before exiting when a source project or solution is specified", w => wait = true },
                { "d|debug", "Attach a debugger to the process", d => Debugger.Launch() },
                { "h|?|help", "Show this message and exit", h => help = true },
            };

            List<string> extra;
            var appName = Path.GetFileName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);

            try
            {
                extra = options.Parse(args);
                if (parentNotFound)
                {
                    Console.Error.WriteLine($"Specified parent process ID {parentId} is not running. Exiting.");
                    return -1;
                }

                if (help || string.IsNullOrEmpty(msbuild))
                {
                    var writer = help ? Console.Out : Console.Error;

                    // show some app description message
                    writer.WriteLine($"Usage: {appName} [OPTIONS]+");
                    writer.WriteLine();

                    writer.WriteLine("Options:");
                    options.WriteOptionDescriptions(writer);

                    return -1;
                }

                MSBuildLocator.RegisterMSBuildPath(msbuild);

                if (parent != null)
                {
                    if (parent.HasExited)
                        return 0;

                    parent.EnableRaisingEvents = true;
                    parent.Exited += (_, __) => exit.Set();
                }

                if (!string.IsNullOrEmpty(project))
                {
                    // The intent is to operate in standalone commandline, 
                    // so no JsonRpc will be set up.
                    var program = new Program();

                    if (properties.Count == 0)
                        Console.WriteLine($@"Creating workspace...");
                    else
                        Console.WriteLine($@"Creating workspace with properties: 
{properties.Select(pair => $"\t{pair.Key}={pair.Value}")}");

                    await program.CreateWorkspaceAsync(properties);
                    Console.WriteLine($@"Opening project {Path.GetFileName(project)}...");
                    var metadata = await program.OpenProjectAsync(project);
                    if (string.IsNullOrEmpty(output))
                        Console.WriteLine(JsonConvert.SerializeObject(metadata, Formatting.Indented, new EnumConverter()));
                    else
                        File.WriteAllText(output, JsonConvert.SerializeObject(metadata, Formatting.Indented, new EnumConverter()));

                    Console.WriteLine("Done");

                    if (wait)
                        Console.ReadLine();

                    return 0;
                }
                else
                {
                    // When no source is passed in, we assume the mode will be RPC and start 
                    // listening
                    var program = new Program();
                    var rpc = new JsonRpc(Console.OpenStandardOutput(), Console.OpenStandardInput(), program);
                    rpc.StartListening();

                    // Force resolving right-away.
                    initializer = Task.Run(Init);
                    exit.Wait();

                    program.workspace?.Dispose();

                    return 0;
                }
            }
            catch (OptionException e)
            {
                Console.Error.WriteLine($"{appName}: {e.Message}");
                Console.Error.WriteLine($"Try '{appName} -?' for more information.");

                if (wait)
                {
                    Console.ReadLine();
                }

                return -1;
            }
            catch (ReflectionTypeLoadException re)
            {
#if DEBUG
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
#endif

                // TODO: should we render something different in this case? (non-options exception?)
                Console.Error.WriteLine($"{appName}: {string.Join(Environment.NewLine, re.LoaderExceptions.Select(e => e.ToString()))}");

                if (wait)
                {
                    Console.ReadLine();
                }

                return -1;
            }
            catch (Exception e)
            {
#if DEBUG
                if (!Debugger.IsAttached)
                {
                    Debugger.Launch();
                }
#endif

                // TODO: should we render something different in this case? (non-options exception?)
                Console.Error.WriteLine($"{appName}: {e.Message}");

                if (wait)
                {
                    Console.ReadLine();
                }

                return -1;
            }
        }

        private static async Task Init()
        {
            var workspace = MSBuildWorkspace.Create();
            await workspace.OpenProjectAsync(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName), "Empty.csproj"));
            await workspace.OpenProjectAsync(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName), "Empty.vbproj"));
            ready = true;
        }

        private MSBuildWorkspace workspace;

        public void Debug() => Debugger.Launch();

        public void Exit() => Task.Delay(200).ContinueWith(_ => exit.Set());

        public bool Ping() => ready;

        public async Task CreateWorkspaceAsync(Dictionary<string, string> properties)
        {
            await initializer;
            workspace?.Dispose();
            workspace = MSBuildWorkspace.Create(properties);
            workspace.SkipUnrecognizedProjects = true;
        }

        public async Task CloseWorkspaceAsync()
        {
            await initializer;
            workspace?.Dispose();
        }

        public async Task<object> OpenProjectAsync(string projectFile)
        {
            if (workspace == null)
            {
                throw new InvalidOperationException($"No active workspace. Invoke {nameof(CreateWorkspaceAsync)} first.");
            }

            var projectFullPath = new FileInfo(projectFile).FullName;
            var project = workspace.CurrentSolution.Projects.FirstOrDefault(p => p.FilePath == projectFullPath);
            if (project == null)
            {
                project = await workspace.OpenProjectAsync(projectFullPath);
            }

            return ProjectToMetadata(project);
        }

        private object ProjectToMetadata(Project project)
        {
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
                    .Select(x => ProjectToMetadata(workspace.CurrentSolution.Projects.First(p => p.Id == x.ProjectId)))
                    .ToArray(),
                MetadataReferences = project.MetadataReferences.OfType<PortableExecutableReference>()
                    .Select(x => x.FilePath).ToArray(),
                Documents = project.Documents
                    .Select(x => new Document
                    {
                        FilePath = x.FilePath,
                        Folders = x.Folders.ToArray()
                    })
                    .ToArray(),
                AdditionalDocuments = project.AdditionalDocuments
                    .Select(x => new Document
                    {
                        FilePath = x.FilePath,
                        Folders = x.Folders.ToArray()
                    })
                    .ToArray()
            };
        }

        public class Document
        {
            public string FilePath { get; set; }
            public string[] Folders { get; set; }
        }

        public class EnumConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
                => objectType.IsEnum;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                => throw new NotImplementedException();

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                => writer.WriteValue(value.ToString());
        }
    }
}
