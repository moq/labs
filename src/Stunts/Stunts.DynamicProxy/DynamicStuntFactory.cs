using System;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;

namespace Stunts.Sdk
{
    /// <summary>
    /// Provides a <see cref="IStuntFactory"/> that creates types at run-time using Castle DynamicProxy.
    /// </summary>
    public class DynamicStuntFactory : IStuntFactory
    {
        static readonly ProxyGenerator generator;
        static readonly ProxyGenerationOptions options;

        static DynamicStuntFactory()
        {
#pragma warning disable 618
            AttributesToAvoidReplicating.Add<SecurityPermissionAttribute>();
#pragma warning restore 618

            AttributesToAvoidReplicating.Add<System.Runtime.InteropServices.MarshalAsAttribute>();
            AttributesToAvoidReplicating.Add<System.Runtime.InteropServices.TypeIdentifierAttribute>();

            options = new ProxyGenerationOptions { Hook = new ObjectMethodsHook() };
#if DEBUG
            // This allows invoking generator.ProxyBuilder.ModuleScope.SaveAssembly() for troubleshooting.
            generator = new ProxyGenerator(new DefaultProxyBuilder(new ModuleScope(true)));
#else
            generator = new ProxyGenerator();
#endif
        }

        /// <inheritdoc />
        public object CreateStunt(Assembly stuntsAssembly, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
        {
            var notImplemented = false;

            if (baseType.IsInterface)
            {
                var fixedInterfaces = new Type[implementedInterfaces.Length + 1];
                fixedInterfaces[0] = baseType;
                implementedInterfaces.CopyTo(fixedInterfaces, 1);
                implementedInterfaces = fixedInterfaces;
                baseType = typeof(object);
                notImplemented = true;
            }

            if (!implementedInterfaces.Contains(typeof(IStunt)))
            {
                var fixedInterfaces = new Type[implementedInterfaces.Length + 1];
                fixedInterfaces[0] = typeof(IStunt);
                implementedInterfaces.CopyTo(fixedInterfaces, 1);
                implementedInterfaces = fixedInterfaces;
            }

            if (baseType.BaseType == typeof(MulticastDelegate))
            {
                var mixinOptions = new ProxyGenerationOptions();
                mixinOptions.AddDelegateTypeMixin(baseType);
                var proxy = CreateProxy(typeof(object), implementedInterfaces, mixinOptions, Array.Empty<object>(), notImplemented);
                return Delegate.CreateDelegate(baseType, proxy, proxy.GetType().GetMethod("Invoke"));
            }
            else
            {
                return CreateProxy(baseType, implementedInterfaces, options, constructorArguments, notImplemented);
            }
        }

        /// <summary>
        /// Creates the proxy with the <see cref="Generator"/>, adding interceptors to implement its behavior.
        /// </summary>
        protected virtual object CreateProxy(Type baseType, Type[] implementedInterfaces, ProxyGenerationOptions options, object[] constructorArguments, bool notImplemented)
            // TODO: bring the approach from https://github.com/moq/moq4/commit/806e9919eab9c1f3879b9e9bda895fa76ecf9d92 for performance.
            => generator.CreateClassProxy(baseType, implementedInterfaces, options, constructorArguments, new DynamicStuntInterceptor(notImplemented));

        /// <summary>
        /// The <see cref="ProxyGenerator"/> used to create proxy types.
        /// </summary>
        protected ProxyGenerator Generator => generator;
    }
}
