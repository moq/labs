using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace Stunts.Tasks
{
    static class IBuildEngineExtensions
    {
        public static IDictionary<string, string> GetGlobalProperties(this IBuildEngine buildEngine)
        {
            ProjectInstance project;

            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var engineType = buildEngine.GetType();
            var callbackField = engineType.GetField("targetBuilderCallback", flags);

            if (callbackField != null)
            {
                // .NET field naming convention.
                var callback = callbackField.GetValue(buildEngine);
                var projectField = callback.GetType().GetField("projectInstance", flags);
                project = (ProjectInstance)projectField.GetValue(callback);
            }
            else
            {
                callbackField = engineType.GetField("_targetBuilderCallback", flags);
                if (callbackField == null)
                    throw new NotSupportedException($"Failed to introspect current MSBuild Engine '{engineType.AssemblyQualifiedName}'.");

                // OSS field naming convention.
                var callback = callbackField.GetValue(buildEngine);
                var projectField = callback.GetType().GetField("_projectInstance", flags);
                project = (ProjectInstance)projectField.GetValue(callback);
            }

            return project.GlobalProperties;
        }
    }
}
