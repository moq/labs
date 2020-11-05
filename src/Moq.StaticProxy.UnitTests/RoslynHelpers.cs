using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Xunit;
using Xunit.Abstractions;

static class RoslynHelpers
{
    public static Assembly Emit(this Compilation compilation, ITestOutputHelper output = null)
    {
        using var stream = new MemoryStream();
        var options = new EmitOptions(debugInformationFormat: DebugInformationFormat.Embedded);

        var cts = new CancellationTokenSource(10000);
        var result = compilation.Emit(stream,
            options: options,
            cancellationToken: cts.Token);
        result.AssertSuccess(output);

        stream.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(stream.ToArray());
    }

    public static void AssertSuccess(this EmitResult result, ITestOutputHelper output = null)
    {
        if (!result.Success)
        {
            if (output != null)
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    //output.WriteLine(diagnostic.Location.ToString());
                }
            }

            Assert.False(true,
                "Emit failed:\r\n" +
                Environment.NewLine +
                string.Join(Environment.NewLine, result.Diagnostics.Select(d => d.ToString())));
        }
    }
}