using Moq.Sdk;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Customizes the code generation naming conventions for target namespace 
    /// and type suffix.
    /// </summary>
    public class MockNamingConvention : NamingConvention
    {
        /// <summary>
        /// Gets the generated code namespace, which is <see cref="MockNaming.Namespace"/>.
        /// </summary>
        public override string Namespace => MockNaming.Namespace;

        /// <summary>
        /// Gets the generated type names suffix, which is <see cref="MockNaming.NameSuffix"/>.
        /// </summary>
        public override string NameSuffix => MockNaming.NameSuffix;
    }
}
