﻿using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Moq.Proxy.Scaffold
{
    [ExportLanguageService(typeof(IDocumentVisitor), LanguageNames.VisualBasic, DocumentVisitorLayer.Scaffold)]
    [Shared]

    class VisualBasicCodeFixes : CodeFixDocumentVisitor
    {
        [ImportingConstructor]
        public VisualBasicCodeFixes(ICodeAnalysisServices services)
            : base(services,
                  CodeFixNames.All.SimplifyNames,
                  CodeFixNames.All.RemoveUnnecessaryImports,
                  CodeFixNames.VisualBasic.ImplementAbstractClass,
                  CodeFixNames.VisualBasic.ImplementInterface,
                  CodeFixNames.VisualBasic.AddOverloads)
        {
        }
    }
}
