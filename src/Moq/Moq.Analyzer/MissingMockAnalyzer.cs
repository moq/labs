using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq.Properties;
using Stunts;

namespace Moq
{
    // TODO: F#
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class MissingMockAnalyzer : MissingStuntAnalyzer
    {
        internal const string DiagnosticId = "MOQ001";

        static readonly LocalizableString Title = new ResourceString(nameof(Resources.MissingMockAnalyzer_Title));
        static readonly LocalizableString Description = new ResourceString(nameof(Resources.MissingMockAnalyzer_Description));
        static readonly LocalizableString MessageFormat = new ResourceString(nameof(Resources.MissingMockAnalyzer_Message));

        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, "Build", DiagnosticSeverity.Warning, true, Description);

        public override DiagnosticDescriptor Descriptor => descriptor;

        public MissingMockAnalyzer() : base(new MockNamingConvention(), typeof(MockGeneratorAttribute)) { }
    }
}
