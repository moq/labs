using System;
using System.Reflection;

namespace Stunts
{
    /// <summary>
    /// Allows accessing the default <see cref="IStuntFactory"/> to use to 
    /// create stunts.
    /// </summary>
    public class StuntFactory : IStuntFactory
    {
        /// <summary>
        /// Gets or sets the default <see cref="IStuntFactory"/> to use 
        /// to create stunts.
        /// </summary>
        public static IStuntFactory Default { get; set; } = new StuntFactory();

        private StuntFactory() { }

        /// <summary>
        /// See <see cref="IStuntFactory.CreateStunt(Assembly, Type, Type[], object[])"/>
        /// </summary>
        public object CreateStunt(Assembly stuntsAssembly, Type baseType, Type[] implementedInterfaces, object[] constructorArguments)
        {
            var name = StuntNaming.GetFullName(baseType, implementedInterfaces);
            var type = stuntsAssembly.GetType(name, true, false);

            return Activator.CreateInstance(type, constructorArguments);
        }
    }
}
