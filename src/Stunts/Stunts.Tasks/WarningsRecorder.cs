using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;

namespace Stunts.Tasks
{
    internal class WarningsRecorder : ILogger, IDisposable
    {
        string projectFile;
        string[] diagnosticIds;
        IEventSource source;

        public WarningsRecorder(string projectFile, string[] diagnosticIds)
        {
            this.projectFile = projectFile;
            this.diagnosticIds = diagnosticIds;
        }

        public IList<BuildWarningEventArgs> Warnings { get; } = new List<BuildWarningEventArgs>();

        public string Parameters { get; set; }

        public LoggerVerbosity Verbosity { get; set; }

        public void Initialize(IEventSource eventSource)
        {
            eventSource.WarningRaised += OnWarning;
            source = eventSource;
        }

        void OnWarning(object sender, BuildWarningEventArgs e)
        {
            if (projectFile.Equals(e.ProjectFile, StringComparison.InvariantCultureIgnoreCase) &&
                diagnosticIds.Any(id => e.Code.Equals(id, StringComparison.OrdinalIgnoreCase)))
                Warnings.Add(e);
        }

        public void Dispose() => source.WarningRaised -= OnWarning;

        public void Shutdown() { }
    }
}
