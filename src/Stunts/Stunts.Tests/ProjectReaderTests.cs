using System;
using System.Collections;
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

        // NOTE: this should have FAILED, but didn't, 
        // Since Stunts.Sdk.csproj = Debug|AnyCPU in the solution configuration, 
        // yet the assert that *all* project references contain `Release` should 
        // have failed.
        [Fact]
        public async Task WhenReadingProjectWithSolutionInformation_ThenReturnsProperConfiguration()
        {
            var rpc = new JsonRpc(process.StandardInput.BaseStream, process.StandardOutput.BaseStream);
            rpc.StartListening();

            await rpc.InvokeAsync("CreateWorkspace", new Dictionary<string, string>
                {
                    { "CurrentSolutionConfigurationContents", @"<SolutionConfiguration>
  <ProjectConfiguration Project='{F15D4684-C048-4349-9EFA-D2C036C43E9C}' AbsolutePath='C:\Code\Personal\moq\src\Stunts\Stunts\Stunts.csproj' BuildProjectInSolution='True'>Release|AnyCPU</ProjectConfiguration>
  <ProjectConfiguration Project='{6AD80F39-063B-4F37-9624-7955EC0E6008}' AbsolutePath='C:\Code\Personal\moq\src\Stunts\Stunts.Internal\Stunts.Internal.csproj' BuildProjectInSolution='True'>Release|AnyCPU</ProjectConfiguration>
  <ProjectConfiguration Project='{80B6A279-ED30-4069-8510-EAF3A139EA7D}' AbsolutePath='C:\Code\Personal\moq\src\Stunts\Stunts.Sdk\Stunts.Sdk.csproj' BuildProjectInSolution='True'>Debug|AnyCPU</ProjectConfiguration>
  <ProjectConfiguration Project='{1320496D-6348-42B0-95F5-BA7891EF0096}' AbsolutePath='C:\Code\Personal\moq\src\Stunts\Stunts.Tests\Stunts.Tests.csproj' BuildProjectInSolution='True'>Release|AnyCPU</ProjectConfiguration>
  <ProjectConfiguration Project='{3C2F7D4C-D10A-439C-B986-FF51664CD895}' AbsolutePath='C:\Code\Personal\moq\src\Stunts\Stunts.Analyzer\Stunts.Analyzer.csproj' BuildProjectInSolution='True'>Release|AnyCPU</ProjectConfiguration>
  <ProjectConfiguration Project='{F801B6E7-70DA-46FE-937D-199BB00E7647}' AbsolutePath='C:\Code\Personal\moq\src\Samples\Sample\Sample.csproj' BuildProjectInSolution='True'>Release|AnyCPU</ProjectConfiguration>
  <ProjectConfiguration Project='{91F9A526-E62C-491B-8774-88A61BDF1E1A}' AbsolutePath='C:\Code\Personal\moq\src\Stunts\Stunts.Package\Stunts.Package.nuproj' BuildProjectInSolution='True'>Release|AnyCPU</ProjectConfiguration>
  <ProjectConfiguration Project='{F033B778-A0F8-43E6-85B9-E184DFFCF347}' AbsolutePath='C:\Code\Personal\moq\src\Stunts\Stunts.Tasks\Stunts.Tasks.csproj' BuildProjectInSolution='True'>Release|AnyCPU</ProjectConfiguration>
  <ProjectConfiguration Project='{D4BCD0FD-946F-4A43-A6B0-6FE3832C2B10}' AbsolutePath='C:\Code\Personal\moq\src\Stunts\Stunts.ProjectReader\Stunts.ProjectReader.csproj' BuildProjectInSolution='True'>Release|AnyCPU</ProjectConfiguration>
</SolutionConfiguration>"},
                    { "Configuration", "Release" },
                    { "Platform", "AnyCPU" }
                });

            var result = await rpc.InvokeAsync<dynamic>("OpenProject",
                new FileInfo(@"..\..\Stunts.Tests.csproj").FullName);

            await rpc.InvokeAsync("Exit");

            process.WaitForExit();

            Assert.NotNull(result);

            if (Debugger.IsAttached)
                output.WriteLine(result.ToString());

            Assert.Equal("Stunts.Tests", (string)result.Name);
            Assert.True(((string)result.OutputFilePath).Contains("Release"));
            Assert.All(((IEnumerable)result.ProjectReferences)
                .Cast<dynamic>().Select(d => (string)d.OutputFilePath),
                s => s.Contains("Release"));
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

            //foreach (string reference in result.ProjectReferences)
            //{
            //    watch.Restart();
            //    output.WriteLine($"Reading {Path.GetFileName(reference)}");
            //    await rpc.InvokeAsync<dynamic>("OpenProject", reference);
            //    output.WriteLine($"Read in {watch.Elapsed.TotalSeconds} seconds.");
            //}

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
                new FileInfo(ExePath).FullName, $"-m=\"{MSBuildBinPath}\"" + (Debugger.IsAttached ? " -d" : ""))
                {
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                });
    }
}
