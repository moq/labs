using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Moq.Proxy
{
    public class ProxyFactory
    {
        private const string TypeNameFormat = "<Mock#_{0}>";
        private static readonly DynamicAssembly assembly = DynamicAssembly.Current;

        private static readonly Dictionary<Type, Type> delegateInterfaceCache = new Dictionary<Type, Type>();

        public object CreateProxy(Type mockType, IProxyInterceptor interceptor, Type[] interfaces, params object[] arguments)
        {
            if (interfaces.Any(i => !i.IsInterface))
            {
                throw new ArgumentException(); // TODO set a proper message
            }

            if (mockType.IsInterface)
            {
                interfaces = ExpandInterfaces(new[] { mockType }.Concat(interfaces)).ToArray();
                mockType = typeof(object);
            }
            else
            {
                interfaces = ExpandInterfaces(interfaces).ToArray();
            }

            var proxyType = assembly.GetType(new[] { mockType }.Concat(interfaces), () => CreateType(mockType, interfaces));
            return Activator.CreateInstance(proxyType, new[] { interceptor }.Concat(arguments ?? new object[0]).ToArray());
        }

        private static void BuildGenericArguments(Type[] genericArguments, TypeBuilder typeBuilder)
        {
            var typeParameters = typeBuilder.DefineGenericParameters(genericArguments.Select(t => t.Name).ToArray())
                .Select((b, i) => new { ParameterBuilder = b, GenericType = genericArguments[i] });

            foreach (var typeParameter in typeParameters)
            {
                typeParameter.ParameterBuilder.SetGenericParameterAttributes(typeParameter.GenericType.GenericParameterAttributes);
                foreach (var constraint in typeParameter.GenericType.GetGenericParameterConstraints())
                {
                    typeParameter.ParameterBuilder.SetBaseTypeConstraint(constraint);
                }
            }
        }

        private static Type CreateType(Type type, IEnumerable<Type> interfaces)
        {
            var typeName = string.Format(CultureInfo.InvariantCulture, TypeNameFormat, Guid.NewGuid().ToString("n"));
            var typeBuilder = assembly.DefineType(typeName, TypeAttributes.Public, type, interfaces);
            if (type.IsGenericTypeDefinition)
            {
                BuildGenericArguments(type.GetGenericArguments(), typeBuilder);
            }

            var interceptorField = typeBuilder.DefineField(
                "interceptor",
                typeof(IProxyInterceptor),
                FieldAttributes.Private | FieldAttributes.InitOnly);

            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly);

            foreach (var constructor in constructors)
            {
                new ConstructorEmitter(typeBuilder, interceptorField).Build(constructor);
            }

            EmitImplementation(type, typeBuilder, interceptorField);

            foreach (var interfaceType in interfaces)
            {
                if (interfaceType.IsAssignableFrom(type))
                {
                    EmitImplementation2(type, interfaceType, typeBuilder, interceptorField);
                }
                else
                {
                    EmitImplementation(interfaceType, typeBuilder, interceptorField);
                }
            }

            return typeBuilder.CreateType();
        }

        private static void EmitImplementation(Type type, TypeBuilder typeBuilder, FieldBuilder interceptorField)
        {
            var genericMappings = GetGenericMappings(type);
            var methodEmiter = new MethodEmitter(typeBuilder, interceptorField, genericMappings);

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.IsVirtual && !m.IsFinal && !m.IsAssembly);

            foreach (var method in methods)
            {
                methodEmiter.Build(method);
            }
        }
        private static void EmitImplementation2(Type t, Type type, TypeBuilder typeBuilder, FieldBuilder interceptorField)
        {
            var genericMappings = GetGenericMappings(type);
            var methodEmiter = new MethodEmitter(typeBuilder, interceptorField, genericMappings);

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.IsVirtual && !m.IsFinal && !m.IsAssembly);

            var interfaceMap = t.GetInterfaceMap(type);

            foreach (var method in methods)
            {
                var index = Array.IndexOf(interfaceMap.InterfaceMethods, method);
                if (index != -1)
                {
                    var xx = t.GetInterfaceMap(type).TargetMethods[index];
                    if (!xx.IsFinal)
                    {
                        methodEmiter.Build(method);
                    }
                }
            }
        }
        private static IEnumerable<Type> ExpandInterfaces(IEnumerable<Type> interfaces)
        {
            return interfaces.Union(interfaces.SelectMany(i => i.GetInterfaces()));
        }

        private static IDictionary<Type, Type> GetGenericMappings(Type type)
        {
            if (type.IsGenericType)
            {
                var argumentTypes = type.GetGenericArguments();
                return type.GetGenericTypeDefinition().GetGenericArguments()
                    .Select((a, i) => new { Argument = a, Type = argumentTypes[i] })
                    .ToDictionary(m => m.Argument, m => m.Type);
            }

            return new Dictionary<Type, Type>(0);
        }
    }
}