using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
using Moq.Sdk;

namespace Moq.Proxy.Dynamic
{
    public class DynamicProxyFactory : IProxyFactory
    {
        static readonly ProxyGenerator generator;
        static readonly ProxyGenerationOptions proxyOptions;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "By Design")]
        static DynamicProxyFactory()
        {
            AttributesToAvoidReplicating.Add<System.Runtime.InteropServices.MarshalAsAttribute>();
            AttributesToAvoidReplicating.Add<System.Runtime.InteropServices.TypeIdentifierAttribute>();

            proxyOptions = new ProxyGenerationOptions { Hook = new ProxyMethodHook() };
            generator = new ProxyGenerator();
        }

        /// <inheritdoc />
        public object CreateProxy(Assembly caller, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
        {
            if (baseType.GetTypeInfo().IsInterface)
            {
                var fixedInterfaces = new Type[implementedInterfaces.Length + 1];
                fixedInterfaces[0] = baseType;
                implementedInterfaces.CopyTo(fixedInterfaces, 1);
                implementedInterfaces = fixedInterfaces;
                baseType = typeof(object);
            }

            if (!implementedInterfaces.Contains(typeof(IProxy)))
            {
                var fixedInterfaces = new Type[implementedInterfaces.Length + 1];
                fixedInterfaces[0] = typeof(IProxy);
                implementedInterfaces.CopyTo(fixedInterfaces, 1);
                implementedInterfaces = fixedInterfaces;
            }

            // TODO: the proxy factory should automatically detect requests to proxy 
            // delegates and generate an interface on the fly for them, without Moq 
            // having to know about it at all.

            return generator.CreateClassProxy(baseType, implementedInterfaces, proxyOptions, constructorArguments, new Interceptor());
        }
    }
}
