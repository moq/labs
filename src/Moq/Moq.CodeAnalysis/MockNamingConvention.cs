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
        public override string Namespace => MockNaming.Namespace;

        public override string NameSuffix => MockNaming.NameSuffix;
    }
}
