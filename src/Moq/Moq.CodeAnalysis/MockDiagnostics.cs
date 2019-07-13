using System;
using Microsoft.CodeAnalysis;
using Moq.Properties;

namespace Moq
{
    /// <summary>
    /// Known diagnostics reported by the Moq analyzers.
    /// </summary>
    public static class MockDiagnostics
    {
        /// <summary>
        /// Diagnostic reported whenever an expected mock is not found in the current compilation.
        /// </summary>
        public static DiagnosticDescriptor Missing { get; } = new DiagnosticDescriptor(
            "MOQ001",
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Title)),
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Message)),
            "Build",
            bool.TryParse(Environment.GetEnvironmentVariable("AutoCodeFix"), out var value) && value ? DiagnosticSeverity.Warning : DiagnosticSeverity.Info,
            true,
            new ResourceString(nameof(Resources.MissingMockAnalyzer_Description)));

        /// <summary>
        /// Diagnostic reported whenever an existing mock is outdated with regards to the interfaces 
        /// it implements or abstract base class it inherits from.
        /// </summary>
        public static DiagnosticDescriptor Outdated { get; } = new DiagnosticDescriptor(
            "MOQ002",
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Title)),
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Message)),
            "Build",
            bool.TryParse(Environment.GetEnvironmentVariable("AutoCodeFix"), out var value) && value ? DiagnosticSeverity.Warning : DiagnosticSeverity.Info,
            true,
            new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Description)));
    }
}
