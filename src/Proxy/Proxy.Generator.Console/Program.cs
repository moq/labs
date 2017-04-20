using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Simplification;
using Mono.Options;

namespace Moq.Proxy
{
    class Program
    {
        static int Main(string[] args) => RunAsync(args).Result;

        static async Task<int> RunAsync(string[] args)
        {
            var shouldShowHelp = false;
            var extension = "";
            var languageName = "";
            var outputPath = "";
            var references = new List<string>();
            var sources = new List<string>();
            var additional = new List<string>();

            var options = new OptionSet
            {
                { "e|extension=", "optional file extension of the generated proxy documents, such as '.cs' or '.vb'. Inferred from language if not specified.", e => extension = e },
                { "l|language=", "a Roslyn-supported language name, such as 'C#' or 'Visual Basic'", l => languageName = l },
                { "o|output=", "the output directory to write the proxy files to", o => outputPath = o },
                { "r|reference=", "an assembly reference required for compiling the sources", r => references.Add(r.Trim()) },
                { "s|source=", "a source file in the language specified which should be processed for proxy generation", s => sources.Add(s.Trim()) },
                { "a|additional=", "optional additional interfaces to implement in all generated proxies", a => additional.Add(a.Trim()) },
                { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
            };

            List<string> extra;
            var appName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);

            try
            {
                extra = options.Parse(args);

                // Parse extra response file if specified
                var responseFile = extra.FirstOrDefault(x => x[0] == '@');
                if (responseFile != null)
                {
                    extra = options.Parse(File.ReadAllLines(responseFile.Substring(1)));
                }

                if (shouldShowHelp ||
                    string.IsNullOrEmpty(outputPath) ||
                    string.IsNullOrEmpty(languageName) ||
                    (sources.Count == 0 && references.Count == 0))
                {
                    // show some app description message
                    Console.WriteLine($"Usage: {appName} [OPTIONS]+");
                    Console.WriteLine();

                    Console.WriteLine("Options:");
                    options.WriteOptionDescriptions(Console.Out);
                }

                if (string.IsNullOrEmpty(extension))
                    extension = languageName == LanguageNames.VisualBasic ? ".vb" : ".cs";

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                var generator = new ProxyGenerator();
                var proxies = await generator.GenerateProxiesAsync(languageName, references.ToImmutableArray(), sources.ToImmutableArray(), additional.ToImmutableArray(), CancellationToken.None);

                foreach (var proxy in proxies)
                {
                    var proxyFile = Path.Combine(outputPath, proxy.Name + extension);
                    var document = await Simplifier.ReduceAsync(proxy);
                    var syntax = await document.GetSyntaxRootAsync();
                    var output = syntax.NormalizeWhitespace().ToFullString();
                    if (!File.Exists(proxyFile) || !File.ReadAllText(proxyFile).Equals(output, StringComparison.Ordinal))
                    {
                        File.WriteAllText(proxyFile, output);
                    }

                    Console.WriteLine(proxyFile);
                }

                return 0;
            }
            catch (OptionException e)
            {
                Console.WriteLine($"{appName}: {e.Message}");
                Console.WriteLine($"Try '{appName} --help' for more information.");
                return -1;
            }
            catch (Exception e)
            {
                // TODO: should we render something different in this case? (non-options exception?)
                Console.WriteLine($"{appName}: {e.Message}");
                Console.WriteLine($"Try '{appName} --help' for more information.");
                return -1;
            }
        }
    }
}