using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using static Mono.Cecil.Cil.OpCodes;
using Superpower;

namespace Stunts.Emit.Static
{
    public class StuntsGenerator : IDisposable
    {
        static readonly string generatedCode = typeof(GeneratedCodeAttribute).FullName;
        static readonly string stuntGenerator = typeof(StuntGeneratorAttribute).FullName;

        StaticTypeResolver typeResolver;
        string assemblyFile;

        public StuntsGenerator(string assemblyFile)
        {
            this.assemblyFile = assemblyFile;
            typeResolver = new StaticTypeResolver(assemblyFile);
        }

        public void Dispose() => typeResolver.Dispose();

        public string Emit()
        {
            var candidates = new ConcurrentDictionary<StuntTypeName, IList<TypeReference>>();

            foreach (var module in typeResolver.AssemblyDefinition.Modules)
            {
                foreach (var type in module.Types.Where(t => 
                    t.IsClass && 
                    !t.CustomAttributes.Any(a => a.AttributeType.FullName == generatedCode)))
                {
                    foreach (var method in type.Methods.Where(m => m.HasBody))
                    {
                        foreach (var arguments in method.Body.Instructions.Where(IsStuntGenerator).Select(GetGeneratorArguments))
                        {
                            candidates.TryAdd(StuntNaming.GetTypeName(arguments), arguments);
                        }
                    }

                    foreach (var property in type.Properties.Where(p => p.GetMethod != null && p.GetMethod.HasBody))
                    {
                        foreach (var arguments in property.GetMethod.Body.Instructions.Where(IsStuntGenerator).Select(GetGeneratorArguments))
                        {
                            candidates.TryAdd(StuntNaming.GetTypeName(arguments), arguments);
                        }
                    }
                }
            }

            foreach (var candidate in candidates.ToArray())
            {
                AddType(typeResolver.AssemblyDefinition.MainModule, candidate.Key, candidate.Value);
            }

            var targetPath = Path.ChangeExtension(assemblyFile, ".g.dll");
            typeResolver.AssemblyDefinition.Write(targetPath);
            Debug.WriteLine(targetPath);
            return targetPath;
        }

        void AddType(ModuleDefinition module, StuntTypeName name, IList<TypeReference> types)
        {
            var type = new TypeDefinition(
                name.Namespace,
                name.Name,
                TypeAttributes.BeforeFieldInit | TypeAttributes.Public);

            if (types[0].Resolve().IsClass)
            {
                type.BaseType = module.ImportReference(types[0]);
                types.RemoveAt(0);
            }
            else
            {
                type.BaseType = module.ImportReference(typeof(object));
            }

            ImplementInterfaces(module, type, types);
            OverrideMembers(module, type);
            ImplementStunt(module, type);

            type.CustomAttributes.Add(new CustomAttribute(
                typeResolver.AssemblyDefinition.MainModule.ImportReference(typeof(CompilerGeneratedAttribute).GetConstructor(Array.Empty<Type>()))));

            module.Types.Add(type);
        }

        void OverrideMembers(ModuleDefinition module, TypeDefinition type)
        {
            var symbol = typeResolver.ResolveSymbol(type.BaseType);
            var skipped = new HashSet<ISymbol>(new SymbolComparer());
            var overridable = GetAllMembers(symbol)
                .Where(s => 
                    s.DeclaredAccessibility == Accessibility.Public || 
                    s.DeclaredAccessibility == Accessibility.Protected || 
                    s.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
                .Where(s => s.Kind != SymbolKind.Method || ((IMethodSymbol)s).MethodKind == MethodKind.Ordinary)
                .Where(s =>
                {
                    if (s.IsSealed)
                    {
                        // If we encounter the symbol sealed as we move upward 
                        // the inheritance chain, we need to flag it as skipped, 
                        // even if a subsequent base class does not define it as 
                        // sealed.
                        skipped.Add(s);
                        return false;
                    }

                    return s.IsVirtual || s.IsAbstract;
                })
                .GroupBy(x => x, new SymbolComparer());

            foreach (var member in overridable.Where(s => !skipped.Contains(s.Key)))
            {
                AddMember(module, type, member.Key, false, true);
            }
        }

        IEnumerable<ISymbol> GetAllMembers(ITypeSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                yield return member;
            }

            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                foreach (var member in baseType.GetMembers())
                {
                    yield return member;
                }

                baseType = baseType.BaseType;
            }
        }

        void ImplementStunt(ModuleDefinition module, TypeDefinition type)
        {
            type.Interfaces.Add(new InterfaceImplementation(
                module.ImportReference(typeResolver.ResolveReference(typeof(IStunt)))));

            //readonly BehaviorPipeline pipeline = new BehaviorPipeline();
            var pipeline = new FieldDefinition("pipeline", 
                FieldAttributes.InitOnly | FieldAttributes.Private, 
                module.ImportReference(typeResolver.ResolveReference(typeof(BehaviorPipeline))));

            type.Fields.Add(pipeline);

            var ctor = new MethodDefinition(".ctor",
                MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.Public | MethodAttributes.HideBySig,
                module.TypeSystem.Void);

            var il = ctor.Body.GetILProcessor();

            // initialize pipeline field
            il.Emit(Ldarg_0);
            il.Emit(Newobj, module.ImportReference(
                typeResolver.ResolveReference(typeof(BehaviorPipeline)).Resolve().GetConstructors().First(c => c.Parameters.Count == 0)));
            il.Emit(Stfld, pipeline);
            il.Emit(Ldarg_0);
            il.Emit(Call, module.ImportReference(type.BaseType.Resolve().GetConstructors().First(c => !c.IsStatic && c.Parameters.Count == 0)));
            il.Emit(Nop);
            il.Emit(Ret);

            type.Methods.Add(ctor);

            var propertyType = module.ImportReference(
                typeResolver.ResolveReference(typeof(ObservableCollection<IStuntBehavior>)));

            //ObservableCollection<IStuntBehavior> IStunt.Behaviors => pipeline.Behaviors;
            var behaviors = new PropertyDefinition(
                nameof(Stunts) + "." + nameof(IStunt) + "." + nameof(IStunt.Behaviors),
                PropertyAttributes.None,
                propertyType);

            var getter = new MethodDefinition(nameof(Stunts) + "." + nameof(IStunt) + ".get_" + nameof(IStunt.Behaviors),
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual | MethodAttributes.Final,
                propertyType);

            getter.Overrides.Add(module.ImportReference(
                typeResolver.ResolveReference(typeof(IStunt)).Resolve().Properties
                    .First(p => p.Name == nameof(IStunt.Behaviors)).GetMethod));

            il = getter.Body.GetILProcessor();
            il.Emit(Ldarg_0);
            il.Emit(Ldfld, pipeline);

            il.Emit(Callvirt, module.ImportReference(
                typeResolver.ResolveReference(typeof(BehaviorPipeline))
                    .Resolve().Properties.First(p => p.Name == nameof(BehaviorPipeline.Behaviors))
                    .GetMethod));

            il.Emit(Ret);

            behaviors.GetMethod = getter;

            type.Methods.Add(getter);
            type.Properties.Add(behaviors);
        }

        void ImplementInterfaces(ModuleDefinition module, TypeDefinition type, IList<TypeReference> interfaces)
        {
            foreach (var iface in interfaces)
            {
                type.Interfaces.Add(new InterfaceImplementation(iface));
            }
            
            var mainSymbols = interfaces.Select(x => typeResolver.ResolveSymbol(x)).ToImmutableArray();
            var allSymbols = mainSymbols.Concat(mainSymbols.OfType<INamedTypeSymbol>().SelectMany(t => t.Interfaces)).ToImmutableArray();

            //var all = allSymbols.SelectMany(i => i.GetMembers()
            //    .Where(s => s.Kind != SymbolKind.Method || ((IMethodSymbol)s).MethodKind == MethodKind.Ordinary)).ToArray();

            var members = allSymbols.SelectMany(i => i.GetMembers()
                .Where(s => s.Kind != SymbolKind.Method || ((IMethodSymbol)s).MethodKind == MethodKind.Ordinary))
                .GroupBy(x => x, new SymbolComparer());

            foreach (var member in members)
            {
                AddMember(module, type, member.Key);
                foreach (var ambiguous in member.Skip(1))
                {
                    AddMember(module, type, ambiguous, true);
                }
            }
        }

        TypeDefinition AddMember(ModuleDefinition module, TypeDefinition type, ISymbol symbol, bool explicitImplementation = false, bool overrideMember = false) 
            => symbol.Kind switch
            {
                SymbolKind.Event => AddEvent(module, type, (IEventSymbol)symbol, explicitImplementation, overrideMember),
                SymbolKind.Property => AddProperty(module, type, (IPropertySymbol)symbol, explicitImplementation, overrideMember),
                SymbolKind.Method => AddMethod(module, type, (IMethodSymbol)symbol, explicitImplementation, overrideMember),
                _ => type
            };

        TypeDefinition AddEvent(ModuleDefinition module, TypeDefinition type, IEventSymbol symbol, bool explicitImplementation = false, bool overrideMember = false)
        {
            var definition = new EventDefinition(
                !explicitImplementation ? symbol.Name : GetExplicitMemberName(symbol.ContainingType, symbol.Name),
                EventAttributes.None,
                module.ImportReference(typeResolver.ResolveReference(symbol.Type)));

            var addMethod = ToMethodDefinition(module, symbol.AddMethod, explicitImplementation, overrideMember);
            addMethod.Attributes |= MethodAttributes.SpecialName;
            AddThrowNotImplemented(module, addMethod.Body.GetILProcessor());
            definition.AddMethod = addMethod;
            type.Methods.Add(addMethod);

            var removeMethod = ToMethodDefinition(module, symbol.RemoveMethod, explicitImplementation, overrideMember);
            removeMethod.Attributes |= MethodAttributes.SpecialName;
            AddThrowNotImplemented(module, removeMethod.Body.GetILProcessor());
            definition.RemoveMethod = removeMethod;
            type.Methods.Add(removeMethod);

            type.Events.Add(definition);
            return type;
        }

        TypeDefinition AddProperty(ModuleDefinition module, TypeDefinition type, IPropertySymbol symbol, bool explicitImplementation = false, bool overrideMember = false)
        {
            var property = new PropertyDefinition(
                symbol.IsIndexer ? "Item" :
                    !explicitImplementation ? symbol.Name : GetExplicitMemberName(symbol.ContainingType, symbol.Name),
                PropertyAttributes.None,
                module.ImportReference(
                    typeResolver.ResolveReference(symbol.Type) ??
                    throw new ArgumentException($"Failed to resolve {symbol.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)}")));

            if (type.CustomAttributes.Count == 0)
            {
                // [DefaultMemberAttribute("Item")]
                var defaultMember = new CustomAttribute(module.ImportReference(
                    typeResolver.ResolveReference(typeof(System.Reflection.DefaultMemberAttribute))
                        .Resolve()
                        .GetConstructors()
                        .First(c => 
                            c.Parameters.Count == 1 && 
                            c.Parameters[0].ParameterType.FullName == typeof(string).FullName)));

                defaultMember.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.String, "Item"));
                type.CustomAttributes.Add(defaultMember);
            }

            if (symbol.GetMethod != null)
            {
                property.GetMethod = ToMethodDefinition(module, symbol.GetMethod, explicitImplementation, overrideMember);
                type.Methods.Add(property.GetMethod);
            }
            if (symbol.SetMethod != null)
            {
                property.SetMethod = ToMethodDefinition(module, symbol.SetMethod, explicitImplementation, overrideMember);
                type.Methods.Add(property.SetMethod);
            }

            type.Properties.Add(property);
            return type;
        }

        TypeDefinition AddMethod(ModuleDefinition module, TypeDefinition type, IMethodSymbol symbol, bool explicitImplementation = false, bool overrideMember = false)
        {
            type.Methods.Add(ToMethodDefinition(module, symbol, explicitImplementation, overrideMember));
            return type;
        }

        MethodDefinition ToMethodDefinition(ModuleDefinition module, IMethodSymbol symbol, bool explicitImplementation = false, bool overrideMember = false)
        {
            //if (symbol.TypeParameters.Length != 0)
            //    throw new NotSupportedException($"Generic methods are not supported at this time: {symbol.ContainingType.Name}.{symbol.Name}<{string.Join(",", symbol.TypeParameters.Select(x => x.Name))}>");

            var accessibility = explicitImplementation ? MethodAttributes.Private : MethodAttributesFromAccessibility(symbol.DeclaredAccessibility);
            if (!overrideMember)
                accessibility |= MethodAttributes.NewSlot;

            var resolved = typeResolver.ResolveReference(symbol.ReturnType) ??
                throw new ArgumentException($"Failed to resolve {symbol.ReturnType.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)}");

            var method = new MethodDefinition(
                !explicitImplementation ? symbol.Name : GetExplicitMemberName(symbol.ContainingType, symbol.Name),
                accessibility | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final,
                module.ImportReference(resolved));

            if (explicitImplementation)
            {
                method.Overrides.Add(module.ImportReference(ToMethodReference(module, symbol)));
            }

            foreach (var parameter in symbol.Parameters)
            {
                method.Parameters.Add(ToParameterDefinition(module, parameter));
            }

            foreach (var generic in symbol.TypeArguments)
            {
                method.GenericParameters.Add(new GenericParameter(generic.Name, method));
            }

            AddThrowNotImplemented(module, method.Body.GetILProcessor());
            return method;
        }

        ParameterDefinition ToParameterDefinition(ModuleDefinition module, IParameterSymbol parameter)
        {
            var paramType = module.ImportReference(
                typeResolver.ResolveReference(parameter.Type) ??
                throw new ArgumentException($"Failed to resolve {parameter.Type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)}."));

            if (parameter.RefKind == RefKind.Ref || parameter.RefKind == RefKind.Out)
                paramType = paramType.MakeByReferenceType();

            return new ParameterDefinition(
                parameter.Name,
                ParameterAttributesFromRefKind(parameter.RefKind),
                paramType)
            {
                IsIn = parameter.RefKind == RefKind.In,
                IsOut = parameter.RefKind == RefKind.Out,
                IsReturnValue = parameter.RefKind == RefKind.Ref,
                IsOptional = parameter.IsOptional
            };
        }

        MethodReference ToMethodReference(ModuleDefinition module, IMethodSymbol method)
        {
            var reference = new MethodReference(method.Name, typeResolver.ResolveReference(method.ReturnType))
            {
                DeclaringType = typeResolver.ResolveReference(method.ContainingType),
                HasThis = true,
            };

            foreach (var parameter in method.Parameters)
            {
                reference.Parameters.Add(ToParameterDefinition(module, parameter));
            }

            if (method.TypeParameters.Length != 0)
                throw new NotSupportedException($"Generic methods are not supported at this time: {method.ContainingType.Name}.{method.Name}<{string.Join(",", method.TypeParameters.Select(x => x.Name))}>");

            return reference;
        }

        static MethodReference MakeGenericMethodReference(TypeReference declaringType, MethodReference method, params TypeReference[] arguments)
        {
            var reference = new MethodReference(method.Name, method.ReturnType)
            {
                DeclaringType = declaringType,
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                CallingConvention = method.CallingConvention,
            };

            foreach (var parameter in method.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var generic_parameter in method.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return reference;
        }

        MethodAttributes MethodAttributesFromAccessibility(Accessibility accessibility) 
            => accessibility switch
            {
                Accessibility.Public => MethodAttributes.Public,
                Accessibility.Internal => MethodAttributes.Assembly,
                Accessibility.Protected => MethodAttributes.Family,
                Accessibility.ProtectedAndInternal => MethodAttributes.FamANDAssem,
                Accessibility.ProtectedOrInternal => MethodAttributes.FamORAssem,
                Accessibility.Private => MethodAttributes.Private,
                _ => MethodAttributes.Assembly
            };

        ParameterAttributes ParameterAttributesFromRefKind(RefKind refKind)
            => refKind switch
            {
                RefKind.In => ParameterAttributes.In,
                RefKind.Out => ParameterAttributes.Out,
                RefKind.Ref => ParameterAttributes.Retval,
                RefKind.None => ParameterAttributes.None,
                _ => ParameterAttributes.None
            };

        string GetExplicitMemberName(INamedTypeSymbol type, string member)
            => TypeNameInfo.FromSymbol(type).ToDisplayName() + "." + member;

        void AddThrowNotImplemented(ModuleDefinition module, ILProcessor il)
        {
            var ctor = module.ImportReference(typeof(NotImplementedException).GetConstructor(Array.Empty<Type>()));
            il.Emit(Newobj, ctor);
            il.Emit(Throw);
        }

        bool IsStuntGenerator(Instruction instruction) =>
            instruction.OpCode == Call &&
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
    }
}
