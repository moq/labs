using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;

namespace Stunts.Tasks
{
    public class AssemblyResolver : IDisposable
    {
        readonly Action<MessageImportance, string> logMessage;

        static AssemblyResolver()
            => AppDomain.CurrentDomain.AssemblyResolve += OnInitialAssemblyResolve;

        public static void Init() { }

        private static Assembly OnInitialAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var directory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            var assemblyFile = Path.Combine(directory, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyFile))
                return Assembly.LoadFrom(assemblyFile);

            return null;
        }

        /// <summary>
        /// Gets or sets the list of paths to reference assemblies.
        /// </summary>
        public ITaskItem[] ReferencePath { get; }

        /// <summary>
        /// Gets or sets the paths to directories to search for dependencies.
        /// </summary>
        public ITaskItem[] AssemblySearchPath { get; }

        public AssemblyResolver(ITaskItem[] assemblySearchPath, Action<MessageImportance, string> logMessage)
        {
            ReferencePath = new ITaskItem[0];
            AssemblySearchPath = assemblySearchPath ?? new ITaskItem[0];
            this.logMessage = logMessage;

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            // Once we're successfully loaded, don't resolve copy-local naively anymore.
            AppDomain.CurrentDomain.AssemblyResolve -= OnInitialAssemblyResolve;
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            logMessage(MessageImportance.Low, $"Resolving {args.Name}");

            return LoadAssembly(new AssemblyName(args.Name));
        }

        private Assembly LoadAssembly(AssemblyName assemblyName)
        {
            var referencedAssemblies = from pathItem in ReferencePath
                                       let refPath = pathItem.GetMetadata("FullPath")
                                       where Path.GetFileNameWithoutExtension(refPath).Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase)
                                       select AssemblyName.GetAssemblyName(refPath);
            var searchedAssemblies = from pathItem in AssemblySearchPath
                                     let path = pathItem.GetMetadata("FullPath")
                                     where Directory.Exists(path)
                                     from file in Directory.EnumerateFiles(path, $"{assemblyName.Name}.dll", SearchOption.TopDirectoryOnly)
                                     select AssemblyName.GetAssemblyName(file);

            return referencedAssemblies
                .Concat(searchedAssemblies)
                //.Where(name => name.Version >= assemblyName.Version)
                // Be more strict and just allow same-major?
                .Where(name =>
                    (name.Name == "Newtonsoft.Json" && name.Version >= assemblyName.Version) || 
                    (name.Version.Major == assemblyName.Version.Major &&
                     name.Version.Minor >= assemblyName.Version.Minor))
                .Select(name =>
                {
                    logMessage(MessageImportance.Low, $"Loading {name.Name} from {new Uri(name.CodeBase).LocalPath}");
                    return Assembly.LoadFrom(new Uri(name.CodeBase).LocalPath);
                })
                .FirstOrDefault();
        }

        public void Dispose()
            => AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
    }
}
