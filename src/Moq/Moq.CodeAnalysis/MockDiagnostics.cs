using Microsoft.CodeAnalysis;
using Moq.Properties;

namespace Moq
{
    public static class MockDiagnostics
    {        
        public static DiagnosticDescriptor Missing { get; } = new DiagnosticDescriptor(
            "MOQ001",
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Title)),
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Message)),
            "Build",
            DiagnosticSeverity.Warning,
            true,
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Description)));

        public static DiagnosticDescriptor Outdated { get; } = new DiagnosticDescriptor(
            "MOQ002",
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Title)),
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Message)),
            "Build",
            DiagnosticSeverity.Warning,
            true,
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Description)));
    }
}
