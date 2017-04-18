using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moq.Proxy
{
    /// <summary>
    /// Allows accessing the default <see cref="IProxyFactory"/> to use to 
    /// create proxies.
    /// </summary>
    public class ProxyFactory : IProxyFactory
    {
        /// <summary>
        /// Gets or sets the default <see cref="IProxyFactory"/> to use 
        /// to create proxies.
        /// </summary>
        public static IProxyFactory Default { get; set; } = new ProxyFactory();

        private ProxyFactory() { }

        /// <summary>
        /// See <see cref="IProxyFactory.CreateProxy(Assembly, Type, IEnumerable{Type}, object[])"/>
        /// </summary>
        public object CreateProxy(Assembly proxiesAssembly, Type baseType, IEnumerable<Type> implementedInterfaces, object[] construtorArguments)
        {
            var name = "ProxyOf" + baseType.Name + string.Join("", implementedInterfaces.Select(x => x.Name));
            var type = proxiesAssembly.GetType(name, true, false);

            return Activator.CreateInstance(type, construtorArguments);
        }
    }
}
