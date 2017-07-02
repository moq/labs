using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Xunit;
using Xunit.Abstractions;

namespace Moq.Proxy.Tests
{
    public class CompositionTests
    {
        ITestOutputHelper output;

        public CompositionTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void CanGetProxyGenerationServices()
        {
            var host = ProxyGenerator.CreateHost();
            var workspace = new AdhocWorkspace(host);

            var services = workspace.Services.GetService<ICodeAnalysisServices>();

            var serviceTypes = typeof(ProxyGenerator).Assembly.GetTypes()
                .Select(x => new
                {
                    Type = x,
                    Export = x.GetCustomAttributes(typeof(ExportLanguageServiceAttribute), false)
                        .OfType<ExportLanguageServiceAttribute>().FirstOrDefault()
                })
                .Where(x => x.Export != null)
                .Select(x => new
                {
                    Key = Tuple.Create(x.Export.Language, x.Export.ServiceType, x.Export.Layer),
                    x.Type
                })
                .GroupBy(x => x.Key)
                .ToArray();

            foreach (var group in serviceTypes)
            {
                var instances = services.GetLanguageServices(group.Key.Item1, group.Key.Item2, group.Key.Item3).ToArray();

                Assert.Equal(group.Count(), instances.Length);
                //output.WriteLine(group.Key.Item1 + ":" + group.Key.Item2.Substring(0, group.Key.Item2.IndexOf(",")) + ":" + group.Key.Item3 + "=" + instances.Length);
            }
        }
    }
}
