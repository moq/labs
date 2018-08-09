using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stunts
{
    /// <summary>
    /// Allows retrieving generator metadata from the configured analyzer options.
    /// </summary>
    public static class AnalyzerOptionsExtensions
    {
        static readonly Dictionary<string, string> emptySettings = new Dictionary<string, string>();

        /// <summary>
        /// Gets the optional generator settings from the configured analyzer options.
        /// </summary>
        public static IDictionary<string, string> GetCodeFixSettings(this AnalyzerOptions options)
        {
            var metadataFile = options.AdditionalFiles.FirstOrDefault(x => x.Path.EndsWith("CodeFixSettings.txt", StringComparison.OrdinalIgnoreCase));
            if (metadataFile != null)
            {
                return File.ReadAllLines(metadataFile.Path)
                    .Where(line => !string.IsNullOrEmpty(line))
                    .Select(line => line.Split('='))
                    .Where(pair => pair.Length == 2)
                    .ToDictionary(pair => pair[0], pair => pair[1]);
            }

            return emptySettings;
        }
    }
}
