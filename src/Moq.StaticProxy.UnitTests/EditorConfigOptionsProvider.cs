using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Moq
{
    public class EditorConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        readonly AnalyzerConfigOptions globalOptions;
        readonly Dictionary<string, AnalyzerConfigOptions> fileOptions;

        public static AnalyzerConfigOptionsProvider Create(params string[] fileNames)
            => Create((IEnumerable<string>)fileNames);

        public static AnalyzerConfigOptionsProvider Create(IEnumerable<string> fileNames)
        {
            var global = fileNames.SelectMany(file => File
                .ReadAllLines(file)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .TakeWhile(line => line[0] != '['))
                .Where(line => !line.StartsWith("#"))
                .Select(line => line.Split('='))
                .Where(pair => pair.Length == 2)
                .Select(pair => new KeyValuePair<string, string>(pair[0].Trim(), pair[1].Trim()))
                .Distinct(new KeyValuePairComparer())
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            if (Debugger.IsAttached)
                global["build_property.DebugSourceGenerators"] = "true";

            var options = new ConcurrentDictionary<string, Dictionary<string, string>>();
            foreach (var lines in fileNames.Select(file => File
                .ReadAllLines(file)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .SkipWhile(line => line[0] != '[')))
            {
                string? section = default;
                foreach (var line in lines)
                {
                    if (line[0] == '[')
                    {
                        section = line.TrimStart('[').TrimEnd(']');
                        continue;
                    }

                    var pair = line.Split('=');
                    if (pair.Length != 2)
                        continue;

                    if (section == null)
                        continue;

                    options.GetOrAdd(section, _ => new Dictionary<string, string>(StringComparer.Ordinal))[pair[0].Trim()] = pair[1].Trim();
                }
            }

            return new EditorConfigOptionsProvider(
                new EditorConfigOptions(global),
                options.ToDictionary(x => x.Key, x => (AnalyzerConfigOptions)new EditorConfigOptions(x.Value)));
        }

        public EditorConfigOptionsProvider(AnalyzerConfigOptions globalOptions, Dictionary<string, AnalyzerConfigOptions> fileOptions)
            => (this.globalOptions, this.fileOptions)
            = (globalOptions, fileOptions);

        public override AnalyzerConfigOptions GlobalOptions => globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => throw new NotSupportedException();

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => fileOptions.TryGetValue(textFile.Path, out var options) ? options : EditorConfigOptions.Empty;

        class EditorConfigOptions : AnalyzerConfigOptions
        {
            public static AnalyzerConfigOptions Empty { get; } = new EditorConfigOptions(new Dictionary<string, string>());

            readonly Dictionary<string, string> values;

            public EditorConfigOptions(Dictionary<string, string> values) => this.values = values;

            public override bool TryGetValue(string key, out string? value) => values.TryGetValue(key, out value);
        }

        class KeyValuePairComparer : IEqualityComparer<KeyValuePair<string, string>>
        {
            public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y) => x.Key.Equals(y.Key, StringComparison.OrdinalIgnoreCase);

            public int GetHashCode(KeyValuePair<string, string> obj) => obj.Key.ToLowerInvariant().GetHashCode();
        }
    }
}
