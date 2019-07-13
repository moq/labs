using System;
using Microsoft.CodeAnalysis;
using Stunts.Properties;

namespace Stunts
{
    /// <summary>
    /// Known diagnostics reported by the Stunts analyzer.
    /// </summary>
    public static class StuntDiagnostics
    {
        /// <summary>
        /// Diagnostic reported whenever an expected stunt is not found in the current compilation.
        /// </summary>
        public static DiagnosticDescriptor MissingStunt { get; } = new DiagnosticDescriptor(
            "ST001",
            new ResourceString(nameof(Resources.MissingStunt_Title)),
            new ResourceString(nameof(Resources.MissingStunt_Message)),
            "Build",
            bool.TryParse(Environment.GetEnvironmentVariable("AutoCodeFix"), out var value) && value ? DiagnosticSeverity.Warning : DiagnosticSeverity.Info,
            true,
            new ResourceString(nameof(Resources.MissingStunt_Description)));

        /// <summary>
        /// Diagnostic reported whenever an existing stunt is outdated with regards to the interfaces 
        /// it implements or abstract base class it inherits from.
        /// </summary>
        public static DiagnosticDescriptor OutdatedStunt { get; } = new DiagnosticDescriptor(
            "ST002",
            new ResourceString(nameof(Resources.OutdatedStunt_Title)),
            new ResourceString(nameof(Resources.OutdatedStunt_Message)),
            "Build",
            bool.TryParse(Environment.GetEnvironmentVariable("AutoCodeFix"), out var value) && value ? DiagnosticSeverity.Warning : DiagnosticSeverity.Info,
            true,
            new ResourceString(nameof(Resources.OutdatedStunt_Description)));

        /// <summary>
        /// Diagnostic reported whenever type parameters specified for a 
        /// <see cref="StuntGeneratorAttribute"/>-annotated method contain a base 
        /// type but it is not the first provided type parameter. This matches 
        /// the compiler requirement of having the base class as the first type too.
        /// </summary>
        public static DiagnosticDescriptor BaseTypeNotFirst { get; } = new DiagnosticDescriptor(
            "ST003",
            new ResourceString(nameof(Resources.BaseTypeNotFirst_Title)),
            new ResourceString(nameof(Resources.BaseTypeNotFirst_Message)),
            "Build",
            DiagnosticSeverity.Error,
            true,
            new ResourceString(nameof(Resources.BaseTypeNotFirst_Description)));

        /// <summary>
        /// Diagnostic reported whenever the base type specified for a 
        /// <see cref="StuntGeneratorAttribute"/>-annotated method is duplicated.
        /// </summary>
        public static DiagnosticDescriptor DuplicateBaseType { get; } = new DiagnosticDescriptor(
            "ST004",
            new ResourceString(nameof(Resources.DuplicateBaseType_Title)),
            new ResourceString(nameof(Resources.DuplicateBaseType_Message)),
            "Build",
            DiagnosticSeverity.Error,
            true,
            new ResourceString(nameof(Resources.DuplicateBaseType_Description)));

        /// <summary>
        /// Diagnostic reported whenever the specified base type for a 
        /// <see cref="StuntGeneratorAttribute"/>-annotated method sealed.
        /// </summary>
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
