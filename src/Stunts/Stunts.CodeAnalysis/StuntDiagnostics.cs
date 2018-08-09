using Microsoft.CodeAnalysis;
using Stunts.Properties;

namespace Stunts
{
    public static class StuntDiagnostics
    {
        public static DiagnosticDescriptor MissingStunt { get; } = new DiagnosticDescriptor(
            "ST001",
            new ResourceString(nameof(Resources.MissingStunt_Title)),
            new ResourceString(nameof(Resources.MissingStunt_Message)),
            "Build",
            DiagnosticSeverity.Warning,
            true,
            new ResourceString(nameof(Resources.MissingStunt_Description)));

        public static DiagnosticDescriptor OutdatedStunt { get; } = new DiagnosticDescriptor(
            "ST002",
            new ResourceString(nameof(Resources.OutdatedStunt_Title)),
            new ResourceString(nameof(Resources.OutdatedStunt_Message)),
            "Build",
            DiagnosticSeverity.Warning,
            true,
            new ResourceString(nameof(Resources.OutdatedStunt_Description)));

        public static DiagnosticDescriptor BaseTypeNotFirst { get; } = new DiagnosticDescriptor(
            "ST003",
            new ResourceString(nameof(Resources.BaseTypeNotFirst_Title)),
            new ResourceString(nameof(Resources.BaseTypeNotFirst_Message)),
            "Build",
            DiagnosticSeverity.Error,
            true,
            new ResourceString(nameof(Resources.BaseTypeNotFirst_Description)));

        public static DiagnosticDescriptor DuplicateBaseType { get; } = new DiagnosticDescriptor(
            "ST004",
            new ResourceString(nameof(Resources.DuplicateBaseType_Title)),
            new ResourceString(nameof(Resources.DuplicateBaseType_Message)),
            "Build",
            DiagnosticSeverity.Error,
            true,
            new ResourceString(nameof(Resources.DuplicateBaseType_Description)));

        public static DiagnosticDescriptor SealedBaseType { get; } = new DiagnosticDescriptor(
            "ST005",
            new ResourceString(nameof(Resources.SealedBaseType_Title)),
            new ResourceString(nameof(Resources.SealedBaseType_Message)),
            "Build",
            DiagnosticSeverity.Error,
            true,
            new ResourceString(nameof(Resources.SealedBaseType_Description)));
    }
}
