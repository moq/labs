using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Superpower;
using Mono.Cecil.Rocks;
using System.Collections.Generic;

namespace Stunts.Emit.Static
{
    public class StaticTypeResolver : IDisposable
    {
        IAssemblyResolver resolver;
        IList<ModuleDefinition> modules;

        public StaticTypeResolver(string assemblyFile)
        {
            AssemblyFile = assemblyFile;
            AssemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFile);
            resolver = new DefaultAssemblyResolver();
            modules = GetModules(AssemblyDefinition, new HashSet<string>()).ToList();

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

        public ITypeSymbol ResolveSymbol(Type type)
            => ResolveSymbol(type.FullName);

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

        public TypeReference ResolveReference(Type type)
            => ResolveReference(type.FullName);

        public TypeReference ResolveReference(string metadataName)
            => ResolveReference(TypeNameInfo.FromMetadataName(metadataName));

        public TypeReference ResolveReference(TypeNameInfo typeName) => ResolveReference(typeName, null);

        public TypeReference ResolveReference(TypeNameInfo typeName, TypeReference owner = null)
        {
            if (typeName.IsTypeParameter)
                return new GenericParameter(typeName.FullName, owner);

            var baseName = typeName.FullName;
            if (typeName.GenericArguments.Length > 0)
                baseName += "`" + typeName.GenericArguments.Length;

            TypeReference baseType = default;
            var nameBuilder = new StringBuilder(baseName);

            while (baseType == null)
            {
                baseType = modules
                    .Select(module => module.GetType(nameBuilder.ToString(), true))
                    .FirstOrDefault(reference => reference != null);

                // Start replacing . with + to catch nested types, from 
                // last to first
                if (baseType == null)
                {
                    var indexOfDot = nameBuilder.ToString().LastIndexOf('.');
                    if (indexOfDot == -1)
                        break;

                    nameBuilder[indexOfDot] = '+';
                }
            }

            if (baseType == null)
                return null;

            if (typeName.GenericArguments.Length > 0)
                baseType = baseType.MakeGenericInstanceType(typeName.GenericArguments.Select(x => ResolveReference(x, baseType)).ToArray());

            if (typeName.ArrayRank > 0)
                baseType = baseType.MakeArrayType(typeName.ArrayRank);

            return baseType;
        }

        IEnumerable<ModuleDefinition> GetModules(AssemblyDefinition assembly, HashSet<string> found)
        {
            if (found.Contains(assembly.FullName))
                yield break;

            found.Add(assembly.FullName);

            foreach (var module in assembly.Modules)
            {
                yield return module;
                foreach (var reference in module.AssemblyReferences.Select(x => resolver.Resolve(x)))
                {
                    foreach (var submodule in GetModules(reference, found))
                    {
                        yield return submodule;
                    }
                }
            }
        }
    }
}
