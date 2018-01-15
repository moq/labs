using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using Stunts.Processors;

namespace Stunts
{
    /// <summary>
    /// Main code generator.
    /// </summary>
    public class StuntGenerator
    {
        // The naming conventions to use for determining class and namespace names.
        NamingConvention naming;

        // Configured processors, by language, then phase.
        Dictionary<string, Dictionary<ProcessorPhase, IDocumentProcessor[]>> processors;

        /// <summary>
        /// Instantiates the set of default <see cref="IDocumentProcessor"/> for the generator, 
        /// used for example when using the default constructor <see cref="StuntGenerator()"/>.
        /// </summary>
        public static IDocumentProcessor[] GetDefaultProcessors() => new IDocumentProcessor[]
        {
            new EnsureStuntsReference(),
            new DefaultImports(),
            new CSharpFileHeader(),
            new CSharpScaffold(),
            new CSharpRewrite(),
            new CSharpStunt(),
            new CSharpCompilerGenerated(),
            new VisualBasicFileHeader(),
            new VisualBasicScaffold(),
            new VisualBasicRewrite(),
            new VisualBasicStunt(),
            new VisualBasicParameterFixup(),
            new VisualBasicCompilerGenerated(),
        };

        /// <summary>
        /// Initializes the generator with the default <see cref="NamingConvention"/> 
        /// and the <see cref="GetDefaultProcessors"/> default processors.
        /// </summary>
        public StuntGenerator() : this(new NamingConvention(), GetDefaultProcessors()) { }

        /// <summary>
        /// Initializes the generator with a custom <see cref="NamingConvention"/> and 
        /// the <see cref="GetDefaultProcessors"/> default processors.
        /// </summary>
        public StuntGenerator(NamingConvention naming) : this(naming, GetDefaultProcessors()) { }

        /// <summary>
        /// Initializes the generator with the default <see cref="NamingConvention"/> 
        /// and the given set of <see cref="IDocumentProcessor"/>s.
        /// </summary>
        public StuntGenerator(params IDocumentProcessor[] processors) : this(new NamingConvention(), (IEnumerable<IDocumentProcessor>)processors) { }

        /// <summary>
        /// Initializes the generator with the default <see cref="NamingConvention"/> 
        /// and the given set of <see cref="IDocumentProcessor"/>s.
        /// </summary>
        public StuntGenerator(IEnumerable<IDocumentProcessor> processors) : this(new NamingConvention(), processors) { }

        /// <summary>
        /// Initializes the generator with a custom <see cref="NamingConvention"/> and 
        /// the given set of <see cref="IDocumentProcessor"/>s.
        /// </summary>
        public StuntGenerator(NamingConvention naming, IEnumerable<IDocumentProcessor> processors)
        {
            this.naming = naming;
            // Splits the processors by supported language and then by phase.
            this.processors = processors
                .SelectMany(processor => processor.Languages.Select(lang => new { Processor = processor, Language = lang }))
                .GroupBy(proclang => proclang.Language)
                .ToDictionary(
                    bylang => bylang.Key,
                    bylang => bylang
                        .GroupBy(proclang => proclang.Processor.Phase)
                        .ToDictionary(
                            byphase => byphase.Key, 
                            byphase => byphase.Select(proclang => proclang.Processor).ToArray()));
        }

        /// <summary>
        /// Generates a stunt document that implements the given types.
        /// </summary>
        /// <remarks>
        /// This aggregating method basically invokes <see cref="CreateStunt(IEnumerable{INamedTypeSymbol}, SyntaxGenerator)"/> 
        /// followed by <see cref="ApplyProcessors(Document, CancellationToken)"/>.
        /// </remarks>
        public async Task<Document> GenerateDocumentAsync(Project project, ITypeSymbol[] types, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var generator = SyntaxGenerator.GetGenerator(project);
            var (name, syntax) = CreateStunt(types.OfType<INamedTypeSymbol>(), generator);
            var code = syntax.NormalizeWhitespace().ToFullString();

            var filePath = Path.GetTempFileName();
#if DEBUG
            // In debug builds, we persist the file so we can inspect the generated code
            // for troubleshooting.
            File.WriteAllText(filePath, code);
#endif

            Document document;

            if (project.Solution.Workspace is AdhocWorkspace workspace)
            {
                document = workspace.AddDocument(DocumentInfo.Create(
                    DocumentId.CreateNewId(project.Id),
                    name,
                    folders: naming.Namespace.Split('.'),
                    filePath: filePath,
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create()))));
            }
            else
            {
                document = project.AddDocument(
                    name,
                    SourceText.From(code),
                    folders: naming.Namespace.Split('.'),
                    filePath: filePath);
            }

            document = await ApplyProcessors(document, cancellationToken).ConfigureAwait(false);

#if DEBUG
            // Update the persisted temp file in debug builds.
            File.WriteAllText(filePath, code);
#endif

            return document;
        }

        /// <summary>
        /// Generates the empty stunt code as a <see cref="SyntaxNode"/>, also returning 
        /// the resulting class name. It contains just the main compilation unit, namespace 
        /// imports, namespace declaration and class declaration with the given base type and 
        /// interfaces, with no class members at all.
        /// </summary>
        public (string name, SyntaxNode syntax) CreateStunt(IEnumerable<INamedTypeSymbol> symbols, SyntaxGenerator generator)
        {
            var name = naming.GetName(symbols);
            var imports = new HashSet<string>();
            var (baseType, implementedInterfaces) = symbols.ValidateGeneratorTypes();

            if (baseType != null && baseType.ContainingNamespace != null && baseType.ContainingNamespace.CanBeReferencedByName)
                imports.Add(baseType.ContainingNamespace.ToDisplayString());

            foreach (var iface in implementedInterfaces.Where(i => i.ContainingNamespace != null && i.ContainingNamespace.CanBeReferencedByName))
            {
                imports.Add(iface.ContainingNamespace.ToDisplayString());
            }

            var syntax = generator.CompilationUnit(imports
                .Select(generator.NamespaceImportDeclaration)
                .Concat(new[]
                {
                    generator.NamespaceDeclaration(naming.Namespace,
                        generator.AddAttributes(
                            generator.ClassDeclaration(name,
                                accessibility: Accessibility.Public,
                                modifiers: DeclarationModifiers.Partial,
                                baseType: baseType == null ? null : generator.IdentifierName(baseType.Name),
                                interfaceTypes: implementedInterfaces
                                    .Select(x => generator.IdentifierName(x.ContainingType != null 
                                        ? x.ContainingType.Name + "." + x.Name 
                                        : x.Name))
                            )
                        )
                    )
                }));

            return (name, syntax);
        }

        /// <summary>
        /// Applies all received <see cref="IDocumentProcessor"/>s received in the generator constructor.
        /// </summary>
        public async Task<Document> ApplyProcessors(Document document, CancellationToken cancellationToken)
        {
#if DEBUG
            // While debugging the geneneration itself, don't let the cancellation timeouts 
            // from tests to cause this to fail.
            if (Debugger.IsAttached)
                cancellationToken = CancellationToken.None;
#endif

            var language = document.Project.Language;
            if (!processors.TryGetValue(language, out var supportedProcessors))
                return document;

            if (supportedProcessors.TryGetValue(ProcessorPhase.Prepare, out var prepares))
            {
                foreach (var prepare in prepares)
                {
                    document = await prepare.ProcessAsync(document, cancellationToken).ConfigureAwait(false);
                }
            }

            if (supportedProcessors.TryGetValue(ProcessorPhase.Scaffold, out var scaffolds))
            {
                foreach (var scaffold in scaffolds)
                {
                    document = await scaffold.ProcessAsync(document, cancellationToken).ConfigureAwait(false);
                }
            }

            if (supportedProcessors.TryGetValue(ProcessorPhase.Rewrite, out var rewriters))
            {
                foreach (var rewriter in rewriters)
                {
                    document = await rewriter.ProcessAsync(document, cancellationToken).ConfigureAwait(false);
                }
            }

            if (supportedProcessors.TryGetValue(ProcessorPhase.Fixup, out var fixups))
            {
                foreach (var fixup in fixups)
                {
                    document = await fixup.ProcessAsync(document, cancellationToken).ConfigureAwait(false);
                }
            }

            return document;
        }
    }
}
