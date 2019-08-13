using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Superpower;
using Mono.Cecil.Rocks;

namespace Stunts.Emit
{
    public class StaticTypeResolver : IDisposable
    {
        IAssemblyResolver resolver;

        public StaticTypeResolver(string assemblyFile)
        {
            AssemblyFile = assemblyFile;
            AssemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFile);
            resolver = new DefaultAssemblyResolver();

            Compilation = CSharpCompilation.Create("foo", references: AssemblyDefinition.MainModule.AssemblyReferences
                .Select(name => MetadataReference
                    .CreateFromFile(resolver.Resolve(name).MainModule.FileName)))
                .AddReferences(MetadataReference.CreateFromFile(AssemblyFile));
        }

        public AssemblyDefinition AssemblyDefinition { get; }

        public string AssemblyFile { get; }

        public Compilation Compilation { get; }

        public void Dispose() => AssemblyDefinition?.Dispose();

        public ITypeSymbol ResolveSymbol(TypeReference reference)
            => ResolveSymbol(TypeNameInfo.FromReference(reference));

        public ITypeSymbol ResolveSymbol(string metadataName)
            => ResolveSymbol(TypeNameInfo.FromMetadataName(metadataName));

        public ITypeSymbol ResolveSymbol(TypeNameInfo typeName)
        {
            var baseName = typeName.FullName;
            if (typeName.GenericArguments.Length > 0)
                baseName += "`" + typeName.GenericArguments.Length;

            var symbol = Compilation.GetTypeByMetadataName(baseName);
            if (symbol == null)
            {
                var nameBuilder = new StringBuilder(baseName);
                // Start replacing . with + to catch nested types, from 
                // last to first
                while (symbol == null)
                {
                    var indexOfDot = nameBuilder.ToString().LastIndexOf('.');
                    if (indexOfDot == -1)
                        break;

                    nameBuilder[indexOfDot] = '+';
                    symbol = Compilation.GetTypeByMetadataName(nameBuilder.ToString());
                }
            }

            if (symbol == null)
                return null;

            if (typeName.GenericArguments.Length > 0)
                symbol = symbol.Construct(typeName.GenericArguments.Select(x => ResolveSymbol(x)).ToArray());

            if (typeName.ArrayRank > 0 && symbol != null)
                return Compilation.CreateArrayTypeSymbol(symbol, typeName.ArrayRank);

            return symbol;
        }

        public TypeReference ResolveReference(ITypeSymbol symbol)
            => ResolveReference(TypeNameInfo.FromSymbol(symbol));

        public TypeReference ResolveReference(string metadataName)
            => ResolveReference(TypeNameInfo.FromMetadataName(metadataName));

        public TypeReference ResolveReference(TypeNameInfo typeName)
        {
            var baseName = typeName.FullName;
            if (typeName.GenericArguments.Length > 0)
                baseName += "`" + typeName.GenericArguments.Length;

            var baseType = AssemblyDefinition.Modules
                .Concat(AssemblyDefinition.Modules
                    .SelectMany(module => module.AssemblyReferences)
                    .Select(name => resolver.Resolve(name))
                    .SelectMany(asm => asm.Modules)
                )
                .Select(module => module.GetType(baseName, true))
                .FirstOrDefault(reference => reference != null);

            if (baseType == null)
                throw new ArgumentException("Resolve failed: " + typeName.ToDisplayName(), nameof(typeName));

            if (typeName.GenericArguments.Length > 0)
                baseType = baseType.MakeGenericInstanceType(typeName.GenericArguments.Select(x => ResolveReference(x)).ToArray());
            
            if (typeName.ArrayRank > 0)
                baseType = baseType.MakeArrayType(typeName.ArrayRank);

            return baseType;
        }
    }
}
