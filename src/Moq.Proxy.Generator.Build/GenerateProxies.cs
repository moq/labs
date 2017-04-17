using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using Tasks = System.Threading.Tasks;

namespace Moq.Proxy
{
    public class GenerateProxies : ToolTask
    {
        public string FileExtension { get; set; }

        [Required]
        public string LanguageName { get; set; }

        [Required]
        public string OutputPath { get; set; }


        public ITaskItem[] References { get; set; }

        public ITaskItem[] Sources { get; set; }

        [Output]
        public ITaskItem[] Proxies { get; set; }

        protected override string ToolName => "pgen.exe";

        // NOTE: this allows the Mono or .NETCore targets to override 'pgen.exe' by setting ToolExe differently 
        // (i.e. 'mono pgen.exe' or 'dotnet pgen.dll').
        protected override string GenerateFullPathToTool() => Path.Combine(ToolPath, ToolExe ?? ToolName);

        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            builder.AppendSwitchIfNotNull("-e", FileExtension);
            builder.AppendSwitchIfNotNull(FileExtension, FileExtension);

            builder.AppendSwitch("-l");
            builder.AppendSwitch(LanguageName);

            builder.AppendSwitch("-o");
            builder.AppendSwitch(OutputPath);

            foreach (var reference in References)
            {
                builder.AppendSwitch("-r");
                builder.AppendSwitch(reference.GetMetadata("FullPath"));
            }

            foreach (var source in Sources)
            {
                builder.AppendSwitch("-s");
                builder.AppendSwitch(source.GetMetadata("FullPath"));
            }

            return builder.ToString();
        }
    }
}
