using System;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Stunts.Tasks
{
    public class RecordFixableWarnings : Task
    {
        [Required]
        public string[] FixableDiagnosticIds { get; set; } = new string[0];

        [Required]
        public string ProjectFile { get; set; }

        public override bool Execute()
        {
            var lifetime = RegisteredTaskObjectLifetime.Build;
            var key = ProjectFile + "|" + typeof(WarningsRecorder).FullName;
            if (BuildEngine4.GetRegisteredTaskObject(key, lifetime) == null)
            {
                try
                {
                    var recorder = new WarningsRecorder(ProjectFile, FixableDiagnosticIds);
                    var flags = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public;
                    var context = BuildEngine.GetType().InvokeMember("LoggingContext", flags, null, BuildEngine, null);
                    var logging = context.GetType().InvokeMember("LoggingService", flags, null, context, null);
                    logging.GetType().InvokeMember("RegisterLogger", flags | BindingFlags.InvokeMethod, null, logging, new[] { recorder });

                    BuildEngine4.RegisterTaskObject(key, recorder, lifetime, false);
                }
                catch (Exception ex)
                {
                    Log.LogErrorFromException(ex, true);
                    return false;
                }
            }

            return true;
        }
    }
}