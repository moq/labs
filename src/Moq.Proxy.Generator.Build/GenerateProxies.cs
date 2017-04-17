using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using Tasks = System.Threading.Tasks;

namespace Moq.Proxy
{
    /// <summary>
    /// Task that generates the proxies.
    /// </summary>
    public class GenerateProxies : ToolTask
    {
        List<ITaskItem> proxies = new List<ITaskItem>();

        public string FileExtension { get; set; }

        [Required]
        public string LanguageName { get; set; }

        [Required]
        public string OutputPath { get; set; }

        public ITaskItem[] References { get; set; }

        public ITaskItem[] Sources { get; set; }

        public ITaskItem[] AdditionalInterfaces { get; set; }

        // TODO: collect from tool output
        [Output]
        public ITaskItem[] Proxies => proxies.ToArray();

        protected override string ToolName => "pgen.exe";

        // NOTE: this allows the Mono or .NETCore targets to override 'pgen.exe' by setting ToolExe differently 
        // (i.e. 'mono pgen.exe' or 'dotnet pgen.dll').
        protected override string GenerateFullPathToTool() => Path.Combine(ToolPath, ToolExe ?? ToolName);

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            // The pgen tool writes to standard output the proxy files it writes out.
            if (File.Exists(singleLine))
                proxies.Add(new TaskItem(singleLine));

            base.LogEventsFromTextOutput(singleLine, messageImportance);
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
            // which the pgen tool reads from the file.
            var builder = new StringBuilder();

            if (AdditionalInterfaces != null)
            {
                foreach (var additionalInterface in AdditionalInterfaces)
                {
                    builder.AppendLine("-a")
                        .AppendLine(additionalInterface.ItemSpec);
                }
            }

            foreach (var reference in References)
            {
                builder.AppendLine("-r")
                    .AppendLine(reference.GetMetadata("FullPath"));
            }

            foreach (var source in Sources)
            {
                builder.AppendLine("-s")
                    .AppendLine(source.GetMetadata("FullPath"));
            }

            return builder.ToString();
        }
    }
}
