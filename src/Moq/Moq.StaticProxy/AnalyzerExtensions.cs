using System.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Moq
{
    internal static class AnalyzerExtensions
    {
        public static void CheckDebugger(this AnalyzerConfigOptionsProvider analyzerOptions, string debugableName)
        {
            if (analyzerOptions.GlobalOptions.TryGetValue("build_property.DebugSourceGenerators", out var debugValue) &&
                bool.TryParse(debugValue, out var shouldDebug) &&
                shouldDebug)
            {
                Debugger.Launch();
            }
            else if (analyzerOptions.GlobalOptions.TryGetValue("build_property.Debug" + debugableName, out debugValue) &&
                bool.TryParse(debugValue, out shouldDebug) &&
                shouldDebug)
            {
                Debugger.Launch();
            }
        }
    }
}
