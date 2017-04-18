// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// Copied from https://github.com/AArnott/CodeGeneration.Roslyn/blob/3c4395f/src/CodeGeneration.Roslyn.Tasks.Helper/Helper.cs#L251-L307

using System;
using System.Globalization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;

namespace Moq.Proxy
{
    internal class ProgressLogger : IProgress<Diagnostic>
    {
        private readonly TaskLoggingHelper logger;
        private readonly string inputFilename;

        internal ProgressLogger(TaskLoggingHelper logger, string inputFilename)
        {
            this.logger = logger;
            this.inputFilename = inputFilename;
        }

        public void Report(Diagnostic value)
        {
            var lineSpan = value.Location.GetLineSpan();
            switch (value.Severity)
            {
                case DiagnosticSeverity.Info:
                    logger.LogMessage(
                        value.Descriptor.Category,
                        value.Descriptor.Id,
                        value.Descriptor.HelpLinkUri,
                        value.Location.SourceTree.FilePath,
                        lineSpan.StartLinePosition.Line + 1,
                        lineSpan.StartLinePosition.Character + 1,
                        lineSpan.EndLinePosition.Line + 1,
                        lineSpan.EndLinePosition.Character + 1,
                        MessageImportance.Normal,
                        value.GetMessage(CultureInfo.CurrentCulture));
                    break;
                case DiagnosticSeverity.Warning:
                    logger.LogWarning(
                        value.Descriptor.Category,
                        value.Descriptor.Id,
                        value.Descriptor.HelpLinkUri,
                        value.Location.SourceTree.FilePath,
                        lineSpan.StartLinePosition.Line + 1,
                        lineSpan.StartLinePosition.Character + 1,
                        lineSpan.EndLinePosition.Line + 1,
                        lineSpan.EndLinePosition.Character + 1,
                        value.GetMessage(CultureInfo.CurrentCulture));
                    break;
                case DiagnosticSeverity.Error:
                    logger.LogError(
                        value.Descriptor.Category,
                        value.Descriptor.Id,
                        value.Descriptor.HelpLinkUri,
                        value.Location.SourceTree.FilePath,
                        lineSpan.StartLinePosition.Line + 1,
                        lineSpan.StartLinePosition.Character + 1,
                        lineSpan.EndLinePosition.Line + 1,
                        lineSpan.EndLinePosition.Character + 1,
                        value.GetMessage(CultureInfo.CurrentCulture));
                    break;
                default:
                    break;
            }
        }
    }
}