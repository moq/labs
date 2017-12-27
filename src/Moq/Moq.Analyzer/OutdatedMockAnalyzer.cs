using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq.Properties;
using Stunts;

namespace Moq
{
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class OutdatedMockAnalyzer : OutdatedStuntAnalyzer
    {
        internal const string DiagnosticId = "MOQ002";

        static readonly LocalizableString Title = new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Title));
        static readonly LocalizableString Description = new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Description));
        static readonly LocalizableString MessageFormat = new ResourceString(nameof(Resources.OutdatedMockAnalyzer_Message));

        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Build", DiagnosticSeverity.Warning, true, Description);

        public override DiagnosticDescriptor Descriptor => descriptor;

        public OutdatedMockAnalyzer() : base(new MockNamingConvention(), typeof(MockGeneratorAttribute)) { }
    }
}
