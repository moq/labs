using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Stunts.Tasks
{
    public class CollectFixableWarnings : Task
    {
        [Required]
        public string ProjectFile { get; set; }

        [Output]
        public ITaskItem[] FixableWarnings { get; set; } = new ITaskItem[0];

        public override bool Execute()
        {
            var key = ProjectFile + "|" + typeof(WarningsRecorder).FullName;
            if (BuildEngine4.GetRegisteredTaskObject(key, RegisteredTaskObjectLifetime.Build) is WarningsRecorder recorder)
            {
                FixableWarnings = recorder.Warnings
                    .Select(w => new TaskItem(w.Code, new Dictionary<string, string>
                    {
                        { nameof(BuildWarningEventArgs.File), w.File },
                        { nameof(BuildWarningEventArgs.LineNumber), w.LineNumber.ToString() },
                        { nameof(BuildWarningEventArgs.ColumnNumber), w.ColumnNumber.ToString() },
                        { nameof(BuildWarningEventArgs.Message), w.Message },
                        { nameof(BuildWarningEventArgs.SenderName), w.SenderName },
                        { nameof(BuildWarningEventArgs.ProjectFile), w.ProjectFile },
                    })).ToArray();

                BuildEngine4.UnregisterTaskObject(key, RegisteredTaskObjectLifetime.Build);
                recorder.Dispose();
            }

            return true;
        }
    }
}