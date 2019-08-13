using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Superpower;

namespace Stunts.Emit.Static
{
    public class StuntsGenerator : IDisposable
    {
        static readonly string generatedCode = typeof(GeneratedCodeAttribute).FullName;
        static readonly string stuntGenerator = typeof(StuntGeneratorAttribute).FullName;

        string assemblyFile;
        AssemblyDefinition assembly;
        Compilation compilation;
        IAssemblyResolver resolver;

        public StuntsGenerator(string assemblyFile)
        {
            this.assemblyFile = assemblyFile;
            assembly = AssemblyDefinition.ReadAssembly(assemblyFile);

            resolver = new DefaultAssemblyResolver();

            compilation = CSharpCompilation.Create("foo", references: assembly.MainModule.AssemblyReferences.Select(x => MetadataReference
                .CreateFromFile(resolver.Resolve(x).MainModule.FileName)))
                .AddReferences(MetadataReference.CreateFromFile(assemblyFile));
        }

        public void Dispose()
        {
            assembly?.Dispose();
        }

        public void Emit()
        {
            var candidates = new Dictionary<StuntTypeName, IList<TypeReference>>();

            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types.Where(t => t.IsClass && !t.CustomAttributes.Any(a => a.AttributeType.FullName == generatedCode)))
                {
                    foreach (var method in type.Methods.Where(m => m.HasBody))
                    {
                        foreach (var arguments in method.Body.Instructions.Where(IsStuntGenerator).Select(GetGeneratorArguments))
                        {
                            var name = StuntNaming.GetTypeName(arguments);
                            if (!candidates.ContainsKey(name))
                                candidates.Add(name, arguments);
                        }
                    }

                    foreach (var property in type.Properties.Where(p => p.GetMethod != null && p.GetMethod.HasBody))
                    {
                        foreach (var arguments in property.GetMethod.Body.Instructions.Where(IsStuntGenerator).Select(GetGeneratorArguments))
                        {
                            var name = StuntNaming.GetTypeName(arguments);
                            if (!candidates.ContainsKey(name))
                                candidates.Add(name, arguments);
                        }
                    }
                }
            }

            foreach (var candidate in candidates)
            {
                AddType(assembly.MainModule, candidate.Key, candidate.Value);
            }

            assembly.Write(Path.ChangeExtension(assemblyFile, ".g.dll"));
        }

        void AddType(ModuleDefinition module, StuntTypeName name, IList<TypeReference> types)
        {
            var type = new TypeDefinition(
                name.Namespace,
                name.Name,
                TypeAttributes.BeforeFieldInit);

            if (types[0].Resolve().IsClass)
            {
                type.BaseType = types[0];
                types.RemoveAt(0);
            }

            AddInterfaces(module, type, types);
            AddStunt(module, type);

            module.Types.Add(type);
        }

        void AddStunt(ModuleDefinition module, TypeDefinition type)
        {
            type.Interfaces.Add(new InterfaceImplementation(module.ImportReference(typeof(IStunt))));
        }

        void AddInterfaces(ModuleDefinition module, TypeDefinition type, IList<TypeReference> types)
        {
            foreach (var iface in types)
            {
                type.Interfaces.Add(new InterfaceImplementation(iface));
            }
            
            var interfaces2 = types.Concat(types.SelectMany(x =>
            {
                var ifaces = x.Resolve().Interfaces;
                if (!x.IsGenericInstance)
                    return ifaces.Select(i => i.InterfaceType);

                var generic = (GenericInstanceType)x;

                return ifaces.Select(i => i.InterfaceType.MakeGenericInstanceType(generic.GenericArguments.ToArray()));
            }));

            var symbols = types.Select(x => compilation.GetTypeByFullName(x.FullName));
            var interfaces = symbols.Concat(symbols.OfType<INamedTypeSymbol>().SelectMany(t => t.Interfaces)).ToImmutableArray();

            var all = interfaces.SelectMany(i => i.GetMembers().Where(s => s.Kind != SymbolKind.Method || ((IMethodSymbol)s).MethodKind == MethodKind.Ordinary)).ToArray();
            var members = interfaces.SelectMany(i => i.GetMembers().Where(s => s.Kind != SymbolKind.Method || ((IMethodSymbol)s).MethodKind == MethodKind.Ordinary)).GroupBy(x => x, new SymbolComparer()).ToList();

            foreach (var member in members)
            {
                AddMember(type, member.Key);
                foreach (var ambiguous in member.Skip(1))
                {
                    AddMember(type, ambiguous, true);
                }
            }
        }

        TypeDefinition AddMember(TypeDefinition type, ISymbol symbol, bool explicitImplementation = false) => symbol.Kind switch
        {
            SymbolKind.Event => AddEvent(type, (IEventSymbol)symbol, explicitImplementation),
            SymbolKind.Property => AddProperty(type, (IPropertySymbol)symbol, explicitImplementation),
            SymbolKind.Method => AddMethod(type, (IMethodSymbol)symbol, explicitImplementation),
            _ => type
        };

        TypeDefinition AddEvent(TypeDefinition type, IEventSymbol symbol, bool explicitImplementation = false)
        {
            return type;
        }

        TypeDefinition AddProperty(TypeDefinition type, IPropertySymbol symbol, bool explicitImplementation = false)
        {
            return type;
        }

        TypeDefinition AddMethod(TypeDefinition type, IMethodSymbol symbol, bool explicitImplementation = false)
        {
            if (!TryGetTypeReference(symbol.ReturnType.ToFullName(), out var returnType))
                throw new ArgumentException(symbol.ReturnType.ToFullName(), nameof(symbol));

            var method = new MethodDefinition(symbol.Name, MethodAttributes.Public, assembly.MainModule.ImportReference(returnType));

            foreach (var parameter in symbol.Parameters)
            {
                if (!TryGetTypeReference(parameter.Type.ToFullName(), out var paramType))
                    throw new ArgumentException(parameter.Type.ToFullName(), nameof(parameter));

                method.Parameters.Add(new ParameterDefinition(parameter.Name, ParameterAttributes.None, assembly.MainModule.ImportReference(paramType)));
            }

            AddThrowNotImplemented(method.Body.GetILProcessor());
            
            type.Methods.Add(method);
            return type;
        }

        bool TryGetTypeReference(string fullName, out TypeReference type)
        {
            if (TryGetTypeReference(fullName, assembly.Modules, out type))
                return true;

            return TryGetTypeReference(fullName, assembly.Modules
                .SelectMany(module => module.AssemblyReferences)
                .Select(name => resolver.Resolve(name))
                .SelectMany(asm => asm.Modules),
                out type);
        }

        bool TryGetTypeReference(string fullName, IEnumerable<ModuleDefinition> modules, out TypeReference type)
        {
            foreach (var module in modules)
            {
                if (module.TryGetTypeReference(fullName, out type))
                    return true;
            }

            type = default;
            return false;
        }

        void AddThrowNotImplemented(ILProcessor il)
        {
            var exception = assembly.MainModule.ImportReference(typeof(NotImplementedException));
            il.Emit(OpCodes.Newobj, exception);
            il.Emit(OpCodes.Throw);
        }

        bool IsStuntGenerator(Instruction instruction) =>
            instruction.OpCode == OpCodes.Call &&
            instruction.Operand is GenericInstanceMethod generator &&
            generator.ElementMethod is MethodDefinition definition &&
            definition.CustomAttributes.Any(a => a.AttributeType.FullName == stuntGenerator);

        IList<TypeReference> GetGeneratorArguments(Instruction instruction)
        {
            var arguments = ((GenericInstanceMethod)instruction.Operand).GenericArguments;
            var sorted = new List<TypeReference>();

            sorted.Add(arguments[0]);
            sorted.AddRange(arguments.Skip(1).OrderBy(x => x.Name, StringComparer.Ordinal));

            return sorted;
        }

        class SymbolComparer : IEqualityComparer<ISymbol>
        {
            static readonly SymbolDisplayFormat fullNameFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable);

            public bool Equals(ISymbol x, ISymbol y)
                => GetHashCode(x) == GetHashCode(y);

            public int GetHashCode(ISymbol symbol)
            {
                var hash = new HashCode().Add(symbol.Name);
                
                if (symbol is IMethodSymbol method)
                {
                    foreach (var generic in method.TypeArguments)
                    {
                        hash.Add(generic.ToDisplayString(fullNameFormat));
                    }
                    foreach (var parameter in method.Parameters)
                    {
                        hash.Add(parameter.Type.ToDisplayString(fullNameFormat));
                    }
                }

                if (symbol is IPropertySymbol property)
                {
                    foreach (var parameter in property.Parameters)
                    {
                        hash.Add(parameter.Type.ToDisplayString(fullNameFormat));
                    }
                }

                return hash.ToHashCode();
            }
        }

        class MethodComparer : IEqualityComparer<MethodDefinition>
        {
            public bool Equals(MethodDefinition x, MethodDefinition y)
                => GetHashCode(x) == GetHashCode(y);

            public int GetHashCode(MethodDefinition method)
            {
                var hash = new HashCode().Add(method.Name);

                foreach (var generic in method.GenericParameters)
                {
                    hash.Add(generic.FullName);
                }

                foreach (var parameter in method.Parameters)
                {
                    hash.Add(parameter.ParameterType.FullName);
                }

                return hash.ToHashCode();
            }
        }
    }
}
