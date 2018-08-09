using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using StreamJsonRpc;

namespace Stunts.Tasks
{
    internal class ProjectReader : IDisposable
    {
        public static ProjectReader GetProjectReader(IBuildConfiguration configuration)
        {
            //var lifetime = configuration.BuildingInsideVisualStudio ?
            //    RegisteredTaskObjectLifetime.AppDomain :
            //    RegisteredTaskObjectLifetime.Build;
            // TODO: When we get project sync, enable ^
            var lifetime = RegisteredTaskObjectLifetime.Build;
            var key = typeof(ProjectReader).FullName;
            if (!(configuration.BuildEngine4.GetRegisteredTaskObject(key, lifetime) is ProjectReader reader))
            {
                reader = new ProjectReader(configuration.MSBuildBinPath, configuration.ToolsPath, configuration.DebugConsole);
                configuration.BuildEngine4.RegisterTaskObject(key, reader, lifetime, false);
            }

            return reader;
        }

        private readonly string msBuildBinPath;
        private readonly string toolsPath;
        private readonly bool debugConsole;
        private readonly string readerExe;
        private Process process;
        private JsonRpc rpc;

        public ProjectReader(string msBuildBinPath, string toolsPath, bool debugConsole)
        {
            this.msBuildBinPath = msBuildBinPath;
            this.toolsPath = toolsPath;
            this.debugConsole = debugConsole;

            readerExe = new FileInfo(Path.Combine(toolsPath, "Stunts.ProjectReader.exe")).FullName;

            if (!File.Exists(readerExe))
                throw new FileNotFoundException($"Did not find project reader tool at '{readerExe}'.", readerExe);

            EnsureRunning();
        }

        private void EnsureRunning()
        {
            if (process == null || process.HasExited)
            {
                var args = "-parent=" + Process.GetCurrentProcess().Id +
                    // We pass down the -d flag so that the external process can also launch a debugger 
                    // for easy troubleshooting.
                    (debugConsole ? " -d " : " ") +
                    "-m=\"" + msBuildBinPath + "\"";

                process = Process.Start(new ProcessStartInfo(readerExe, args)
                {
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                });

                rpc = new JsonRpc(process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
                rpc.StartListening();
            }
        }

        public void Dispose()
        {
            if (process != null && !process.HasExited)
                Task.Run(async () => await rpc.InvokeAsync("Exit"));

            process.WaitForExit();
            process = null;
        }

        public async Task Debug()
        {
            EnsureRunning();
            await rpc.InvokeAsync(nameof(Debug));
        }

        public async Task Exit()
        {
            if (process != null && !process.HasExited)
            {
                await rpc.InvokeAsync(nameof(Exit));
                process.WaitForExit();
            }

            process = null;
        }

        public async Task<bool> Ping()
        {
            EnsureRunning();
            return await rpc.InvokeAsync<bool>(nameof(Ping));
        }

        public async Task CreateWorkspaceAsync(IDictionary<string, string> globalProperties)
        {
            EnsureRunning();
            // We never do codegen in the remote workspace
            await rpc.InvokeAsync(nameof(CreateWorkspaceAsync), new Dictionary<string, string>(globalProperties)
            {
                ["NoCodeGen"] = "true"
            });
        }

        public async Task CloseWorkspaceAsync()
        {
            EnsureRunning();
            await rpc.InvokeAsync(nameof(CloseWorkspaceAsync));
        }

        public async Task<dynamic> OpenProjectAsync(string projectFullPath)
        {
            EnsureRunning();
            return await rpc.InvokeAsync<dynamic>(nameof(OpenProjectAsync), projectFullPath);
        }
    }
}