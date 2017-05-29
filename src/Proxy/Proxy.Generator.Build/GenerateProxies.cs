using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Moq.Proxy
{
    /// <summary>
    /// Task that generates the proxies.
    /// </summary>
    public class GenerateProxies : ToolTask
    {
        List<ITaskItem> proxies = new List<ITaskItem>();

        /// <summary>
        /// The extension of the generated proxy files.
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Name of the target language, which must be one known to Rosly. 
        /// <see cref="Microsoft.CodeAnalysis.LanguageNames"/>.
        /// </summary>
        [Required]
        public string LanguageName { get; set; }

        /// <summary>
        /// The target directory where generated proxies should be written to.
        /// </summary>
        [Required]
        public string OutputPath { get; set; }

        /// <summary>
        /// Optional working directory to run the tool from.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Assembly references for the code generation.
        /// </summary>
        public ITaskItem[] References { get; set; }

        /// <summary>
        /// Source files to inspect to discover types that need proxies, denoted 
        /// by invocations to methods that are annotated with <c>Moq.Proxy.ProxyGeneratorAttribute</c>.
        /// </summary>
        public ITaskItem[] Sources { get; set; }

        /// <summary>
        /// Additional interfaces (by full type name) that should be implemented by generated 
        /// proxies.
        /// </summary>
        public ITaskItem[] AdditionalInterfaces { get; set; }

        /// <summary>
        /// Additional types (by full type name) that should be proxied.
        /// </summary>
        public ITaskItem[] AdditionalProxies { get; set; }

        /// <summary>
        /// Additional generator assemblies that participate in the code generation composition.
        /// </summary>
        public ITaskItem[] AdditionalGenerators { get; set; }

        [Output]
        public ITaskItem[] Proxies => proxies.ToArray();

        protected override bool HandleTaskExecutionErrors()
        {
            var responseFile = Path.Combine(OutputPath, "pgen.rsp");

            // When we fail, we create a pgen.rsp file for easy repro'ing of the 
            // failure if necessary.
            Log.LogWarning($@"Proxy generation failed. To re-run and debug the generation, please execute the following command:
""{Path.GetFullPath(GenerateFullPathToTool())}"" @{responseFile} -debug");

            Directory.CreateDirectory(OutputPath);
            File.WriteAllText(responseFile, GenerateCommandLineCommands() + Environment.NewLine + GenerateResponseFileCommands());

            return base.HandleTaskExecutionErrors();
        }

        protected override string ToolName => "pgen.exe";

        // NOTE: this allows the Mono or .NETCore targets to override 'pgen.exe' by setting ToolExe differently 
        // (i.e. 'mono pgen.exe' or 'dotnet pgen.dll').
        protected override string GenerateFullPathToTool() => Path.Combine(ToolPath, ToolExe ?? ToolName);

        protected override string GetWorkingDirectory() => WorkingDirectory ?? ToolPath;

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            // The pgen tool writes to standard output the proxy files it writes out.
            if (File.Exists(singleLine))
                proxies.Add(new TaskItem(singleLine));

            base.Log.LogMessageFromText(singleLine, messageImportance);
        }

        protected override void LogToolCommand(string message)
        {
            Log.LogCommandLine(MessageImportance.Low, message);
        }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            // Command line arguments that are short and single, we pass them directly 
            // as a command line.
            if (!string.IsNullOrEmpty(FileExtension))
            {
                builder.AppendSwitch("-e");
                builder.AppendSwitch(FileExtension);
            }

            builder.AppendSwitch("-l");
            builder.AppendFileNameIfNotNull(LanguageName);

            builder.AppendSwitch("-o");
            builder.AppendFileNameIfNotNull(OutputPath);

            return builder.ToString();
        }

        protected override string GenerateResponseFileCommands()
        {
            // Arguments that can potentially be many and go over the 
            // command line args length are passed via a response file, 
            // which the pgen tool reads from the file automatically via 
            // Mono.Options
            var builder = new StringBuilder();

            if (AdditionalInterfaces != null)
            {
                foreach (var additionalInterface in AdditionalInterfaces)
                {
                    builder.AppendLine("-i")
                        .AppendLine(additionalInterface.ItemSpec);
                }
            }

            if (AdditionalProxies != null)
            {
                foreach (var additionalProxy in AdditionalProxies)
                {
                    builder.AppendLine("-p")
                        .AppendLine(additionalProxy.ItemSpec);
                }
            }

            if (AdditionalGenerators != null)
            {
                foreach (var additionalGenerator in AdditionalGenerators)
                {
                    builder.AppendLine("-g")
                        .AppendLine(additionalGenerator.ItemSpec);
                }
            }

            if (References != null)
            {
                foreach (var reference in References)
                {
                    builder.AppendLine("-r")
                        .AppendLine("\"" + reference.GetMetadata("FullPath") + "\"");
                }
            }

            if (Sources != null)
            {
                foreach (var source in Sources)
                {
                    builder.AppendLine("-s")
                        .AppendLine("\"" + source.GetMetadata("FullPath") + "\"");
                }
            }

            return builder.ToString();
        }
    }
}