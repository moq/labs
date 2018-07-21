using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using StreamJsonRpc;
using Xunit;
using Xunit.Abstractions;

namespace Stunts.Tests
{
    public class ProjectReaderTests : IDisposable
    {
#if DEBUG
        private const string Configuration = "Debug";
#else
        const string Configuration = "Release";
#endif
        private const string ExePath = @"..\..\..\Stunts.ProjectReader\bin\" + Configuration + @"\Stunts.ProjectReader.exe";

        // Generated from MSBuild with <AssemblyAttribute Include="System.Reflection.AssemblyMetadata">
        static readonly string MSBuildBinPath = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(x => x.Key == "MSBuildBinPath")
            .Select(x => x.Value)
            .First();

        ITestOutputHelper output;
        Process process;

        public ProjectReaderTests(ITestOutputHelper output)
        {
            this.output = output;
            StartProcess();
        }

        [Fact]
        public async Task WhenRunningReader_ThenCanShutdown()
        {
            var rpc = new JsonRpc(process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
            rpc.StartListening();

            await rpc.InvokeAsync("Ping");
            await rpc.InvokeAsync("Exit");

            process.WaitForExit();
        }

        [Fact]
        public async Task WhenReadingProject_ThenReturnsMetadata()
        {
            var rpc = new JsonRpc(process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
            rpc.StartListening();

            await rpc.InvokeAsync("CreateWorkspace", new Dictionary<string, string>
                {
                    { "foo", "bar" }
                });

            var result = await rpc.InvokeAsync<dynamic>("OpenProject",
                new FileInfo(@"..\..\Stunts.Tests.csproj").FullName);

            await rpc.InvokeAsync("Exit");

            process.WaitForExit();

            Assert.NotNull(result);

            if (Debugger.IsAttached)
                output.WriteLine(result.ToString());

            Assert.Equal("Stunts.Tests", (string)result.Name);
        }

        [Fact]
        public async Task WhenReadingAllProjects_ThenReturnsMetadata()
        {
            var rpc = new JsonRpc(process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
            rpc.StartListening();

            while (!await rpc.InvokeAsync<bool>("Ping"))
            {
                Thread.Sleep(100);
            }

            await ReadAllProjects(rpc);
            await ReadAllProjects(rpc);

            await rpc.InvokeAsync("Exit");

            process.WaitForExit();
        }

        public async Task ReadAllProjects(JsonRpc rpc)
        {
            var total = Stopwatch.StartNew();
            var watch = Stopwatch.StartNew();

            output.WriteLine("Creating workspace");
            await rpc.InvokeAsync("CreateWorkspace", new Dictionary<string, string>
                {
                    { "foo", "bar" }
                });
            output.WriteLine($"Created in {watch.Elapsed.TotalSeconds} seconds.");

            watch.Restart();
            output.WriteLine("Reading Stunts.csproj");
            var result = await rpc.InvokeAsync<dynamic>("OpenProject",
                new FileInfo(@"..\..\..\Stunts\Stunts.csproj").FullName);

            output.WriteLine($"Read in {watch.Elapsed.TotalSeconds} seconds.");

            watch.Restart();
            output.WriteLine("Reading Stunts.Tests.csproj");
            result = await rpc.InvokeAsync<dynamic>("OpenProject",
                new FileInfo(@"..\..\Stunts.Tests.csproj").FullName);

            output.WriteLine($"Read in {watch.Elapsed.TotalSeconds} seconds.");

            foreach (string reference in result.ProjectReferences)
            {
                watch.Restart();
                output.WriteLine($"Reading {Path.GetFileName(reference)}");
                await rpc.InvokeAsync<dynamic>("OpenProject", reference);
                output.WriteLine($"Read in {watch.Elapsed.TotalSeconds} seconds.");
            }

            await rpc.InvokeAsync("CloseWorkspace");

            watch.Stop();
            total.Stop();
            output.WriteLine($"Read all in {total.Elapsed.TotalSeconds} seconds.");
        }

        public void Dispose()
        {
            if (!process.HasExited)
                process.Kill();
        }

        private void StartProcess()
            => process = Process.Start(new ProcessStartInfo(
                new FileInfo(ExePath).FullName, (Debugger.IsAttached ? " -d" : ""))
                {
                    CreateNoWindow = true,
                    WorkingDirectory = MSBuildBinPath,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                });
    }
}
