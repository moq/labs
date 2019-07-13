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
    /// <summary>
    /// Code action that performs the actual code generation.
    /// </summary>
    public class StuntCodeAction : CodeAction
    {
        readonly string title;
        readonly Document document;
        readonly Diagnostic diagnostic;
        readonly NamingConvention naming;

        /// <summary>
        /// Initializes the action.
        /// </summary>
        public StuntCodeAction(string title, Document document, Diagnostic diagnostic, NamingConvention naming)
        {
            this.title = title;
            this.document = document;
            this.diagnostic = diagnostic;
            this.naming = naming;
        }

        /// <inheritdoc />
        public override string EquivalenceKey => diagnostic.Id + ":" + diagnostic.Properties["TargetFullName"];

        /// <inheritdoc />
        public override string Title => title;

        /// <summary>
        /// Gets the generator for the given coding convention.
        /// </summary>
        protected virtual StuntGenerator CreateGenerator(NamingConvention naming) => new StuntGenerator(naming);

        /// <inheritdoc />
        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            if (root == null)
                return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
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

            // The logical folder is always the split namespace, since generated code is 
            // expected to be added as hidden linked files.
            var folders = naming.Namespace.Split('.');

            if (!diagnostic.Properties.TryGetValue("Location", out var file) ||
                string.IsNullOrEmpty(file))
            {
                file = Path.Combine(Path.GetDirectoryName(document.Project.FilePath), Path.Combine(folders), name + extension);
            }

            var stuntDoc = document.Project.Documents.FirstOrDefault(d => d.Name == Path.GetFileName(file) && d.Folders.SequenceEqual(folders));

            // NOTE: the environment variable tells us we're being run with AutoCodeFix enabled and 
            // active, meaning we should generate files into the intermediate output path instead.
            var autoCodeFixEnabled = bool.TryParse(Environment.GetEnvironmentVariable("AutoCodeFix"), out var value) && value;

            // Update target file path if running from build-time codegen.
            if (stuntDoc == null && autoCodeFixEnabled)
            {
                // Get configured generator options to build the final path
                if (document.Project.AnalyzerOptions.GetCodeFixSettings().TryGetValue("IntermediateOutputPath", out var intermediateOutputPath))
                {
                    file = Path.Combine(intermediateOutputPath, Path.Combine(folders), name + ".g" + extension);
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
                    // When running the generator from design-time, ensure the folder exists.
                    // This is necessary since we link files into the Mocks/Stunts folder, 
                    // which becomes a "linked" folder itself, and when adding the document, 
                    // VS fails to so.
                    if (!autoCodeFixEnabled)
                    {
                        var directory = Path.Combine(Path.GetDirectoryName(document.Project.FilePath), Path.Combine(folders));
                        Directory.CreateDirectory(directory);
                    }

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

            stuntDoc = await stunts.ApplyProcessors(stuntDoc, cancellationToken);
            
            // This is somewhat expensive, but with the formatting, the code doesn't even compile :/
            stuntDoc = await Simplifier.ReduceAsync(stuntDoc);
            if (document.Project.Language != LanguageNames.VisualBasic)
                stuntDoc = await Formatter.FormatAsync(stuntDoc, Formatter.Annotation);

            syntax = await stuntDoc.GetSyntaxRootAsync();
            stuntDoc = stuntDoc.WithSyntaxRoot(syntax);

            return stuntDoc;
        }
    }
}
