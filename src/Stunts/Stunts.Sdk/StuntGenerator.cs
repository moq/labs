using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using Stunts.Processors;
using Stunts.Properties;

namespace Stunts
{
    public class StuntGenerator
    {
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
            new CSharpScaffold(),
            new CSharpFileHeader(),
            new CSharpRewrite(),
            new CSharpCompilerGenerated(),
            new VisualBasicScaffold(),
            new VisualBasicFileHeader(),
            new VisualBasicRewrite(),
            new VisualBasicParameterFixup(),
            new VisualBasicCompilerGenerated(),
        };

        public StuntGenerator() : this(new NamingConvention(), GetDefaultProcessors()) { }

        public StuntGenerator(NamingConvention naming) : this(naming, GetDefaultProcessors()) { }

        protected StuntGenerator(NamingConvention naming, IDocumentProcessor[] processors)
        {
            this.naming = naming;
            this.processors = processors
                .GroupBy(processor => processor.Language)
                .ToDictionary(
                    bylang => bylang.Key,
                    bylang => bylang
                        .GroupBy(processor => processor.Phase)
                        .ToDictionary(byphase => byphase.Key, byphase => byphase.ToArray()));
        }

        public async Task<Document> GenerateDocumentAsync(Project project, ITypeSymbol[] types, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var generator = SyntaxGenerator.GetGenerator(project);
            var (name, syntax) = CreateStunt(types.OfType<INamedTypeSymbol>(), generator);
            var code = syntax.NormalizeWhitespace().ToFullString();

            var filePath = Path.GetTempFileName();
#if DEBUG
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

            document = await ApplyVisitors(document, cancellationToken).ConfigureAwait(false);

#if DEBUG
            File.WriteAllText(filePath, code);
#endif

            return document;
        }

        public (string name, SyntaxNode syntax) CreateStunt(IEnumerable<INamedTypeSymbol> symbols, SyntaxGenerator generator)
        {
            var name = naming.GetName(symbols);
            var imports = new HashSet<string>
            {
                typeof(EventArgs).Namespace,
                typeof(ObservableCollection<>).Namespace,
                typeof(MethodBase).Namespace,
                typeof(IStunt).Namespace,
                typeof(CompilerGeneratedAttribute).Namespace,
            };

            var (baseType, implementedInterfaces) = ValidateTypes(symbols.ToArray());
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

        public async Task<Document> ApplyVisitors(Document document, CancellationToken cancellationToken)
        {
#if DEBUG
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

        static (INamedTypeSymbol baseType, ImmutableArray<INamedTypeSymbol> additionalInterfaces) ValidateTypes(INamedTypeSymbol[] types)
        {
            if (types.Length == 0)
                throw new ArgumentException(Strings.SymbolRequired);

            var baseType = default(INamedTypeSymbol);
            var additionalInterfaces = default(IEnumerable<INamedTypeSymbol>);
            if (types[0].TypeKind == TypeKind.Class)
            {
                baseType = types[0];
                if (types.Skip(1).Any(x => x.TypeKind == TypeKind.Class))
                    throw new ArgumentException(Strings.WrongBaseType(string.Join(",", types.Select(x => x.Name))));
                if (types.Skip(1).Any(x => x.TypeKind != TypeKind.Interface))
                    throw new ArgumentException(Strings.InvalidStuntTypes(string.Join(",", types.Select(x => x.Name))));

                additionalInterfaces = types.Skip(1);
            }
            else
            {
                if (types.Any(x => x.TypeKind == TypeKind.Class))
                    throw new ArgumentException(Strings.WrongBaseType(string.Join(",", types.Select(x => x.Name))));
                if (types.Any(x => x.TypeKind != TypeKind.Interface))
                    throw new ArgumentException(Strings.InvalidStuntTypes(string.Join(",", types.Select(x => x.Name))));

                additionalInterfaces = types;
            }

            return (baseType, additionalInterfaces.OrderBy(x => x.Name).ToImmutableArray());
        }
    }
}
