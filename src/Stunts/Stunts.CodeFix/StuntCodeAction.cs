using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Stunts
{
    public class StuntCodeAction : CodeAction
    {
        string title;
        Document document;
        readonly Diagnostic diagnostic;
        readonly NamingConvention naming;

        public StuntCodeAction(string title, Document document, Diagnostic diagnostic, NamingConvention naming)
        {
            this.title = title;
            this.document = document;
            this.diagnostic = diagnostic;
            this.naming = naming;
        }

        public override string EquivalenceKey => diagnostic.Id + ":" + diagnostic.Properties["TargetFullName"];

        public override string Title => title;

        protected virtual StuntGenerator CreateGenerator(NamingConvention naming) => new StuntGenerator(naming);

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel == null)
                return document;

            var compilation = await document.Project.GetCompilationAsync(cancellationToken);
            var symbols = diagnostic.Properties["Symbols"]
                .Split('|')
                .Select(compilation.GetTypeByMetadataName)
                .Where(t => t != null)
                .ToArray();

            var generator = SyntaxGenerator.GetGenerator(document.Project);
            var stunts = CreateGenerator(naming);

            return await CreateStunt(symbols, generator, stunts, cancellationToken);
        }

        async Task<Document> CreateStunt(IEnumerable<INamedTypeSymbol> symbols, SyntaxGenerator generator, StuntGenerator stunts, CancellationToken cancellationToken)
        {
            var (name, syntax) = stunts.CreateStunt(symbols, generator);

            // TODO: F#
            var extension = document.Project.Language == LanguageNames.CSharp ? ".cs" : ".vb";
            string[] folders;
            if (!diagnostic.Properties.TryGetValue("Location", out var file) ||
                string.IsNullOrEmpty(file))
            {
                file = Path.Combine(Path.GetDirectoryName(document.Project.FilePath), naming.Namespace, name + extension);
                folders = naming.Namespace.Split('.');
            }
            else
            {
                folders = Path.GetDirectoryName(file)
                    .Replace(Path.GetDirectoryName(document.Project.FilePath), "")
                    .Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            var stuntDoc = document.Project.Documents.FirstOrDefault(d => d.Name == Path.GetFileName(file) && d.Folders.SequenceEqual(folders));
            var isBuildTime = document.Project.Solution.Workspace.Options.GetOption(StuntOptions.IsBuildTime);

            // Also probe intermediate output path for build-time codegen.
            if (stuntDoc == null && isBuildTime)
            {
                // Get configured generator options
                if (document.Project.AnalyzerOptions.GetCodeFixSettings().TryGetValue("IntermediateOutputPath", out var intermediateOutputPath))
                {
                    folders = intermediateOutputPath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                        .Concat(folders)
                        .ToArray();

                    file = Path.Combine(Path.GetDirectoryName(document.Project.FilePath), intermediateOutputPath, naming.Namespace, name + ".g" + extension);

                    // Search for the doc at the new location.
                    stuntDoc = document.Project.Documents.FirstOrDefault(d => d.Name == Path.GetFileName(file) && d.Folders.SequenceEqual(folders));
                }
            }

            if (stuntDoc == null)
            {
                if (document.Project.Solution.Workspace is AdhocWorkspace workspace)
                {
                    stuntDoc = workspace.AddDocument(DocumentInfo
                        .Create(
                            DocumentId.CreateNewId(document.Project.Id),
                            Path.GetFileName(file),
                            folders: folders,
                            filePath: file))
                        .WithSyntaxRoot(syntax);
                }
                else
                {
                    // Ensure target directory exists since both end up as 
                    // linked folders until we add a generated stunt to it, because 
                    // the API contentFiles are linked with the same folder name.
                    if (naming.Namespace == nameof(Stunts) || naming.Namespace == "Mocks")
                        Directory.CreateDirectory(Path.GetDirectoryName(file));

                    stuntDoc = document.Project.AddDocument(
                        Path.GetFileName(file),
                        syntax,
                        folders,
                        file);
                }
            }
            else
            {
                stuntDoc = stuntDoc.WithSyntaxRoot(syntax);
            }

            stuntDoc = await stunts.ApplyProcessors(stuntDoc, cancellationToken).ConfigureAwait(false);
            
            // This is somewhat expensive, but with the formatting, the code doesn't even compile :/
            stuntDoc = await Simplifier.ReduceAsync(stuntDoc).ConfigureAwait(false);
            if (document.Project.Language != LanguageNames.VisualBasic)
                stuntDoc = await Formatter.FormatAsync(stuntDoc, Formatter.Annotation).ConfigureAwait(false);

            syntax = await stuntDoc.GetSyntaxRootAsync().ConfigureAwait(false);
            stuntDoc = stuntDoc.WithSyntaxRoot(syntax);

            return stuntDoc;
        }
    }
}
