using Moq.Sdk;
using Stunts.CodeAnalysis;

namespace Moq
{
    /// <summary>
    /// Customizes the code generation naming conventions for target namespace 
    /// and type suffix.
    /// </summary>
    public class MockNamingConvention : NamingConvention
    {
        /// <summary>
        /// Gets the generated code namespace, which is <see cref="MockNaming.DefaultNamespace"/>.
        /// </summary>
        public override string Namespace => MockNaming.DefaultNamespace;

        /// <summary>
        /// Gets the generated type names suffix, which is <see cref="MockNaming.DefaultSuffix"/>.
        /// </summary>
        public override string NameSuffix => MockNaming.DefaultSuffix;
    }
}
