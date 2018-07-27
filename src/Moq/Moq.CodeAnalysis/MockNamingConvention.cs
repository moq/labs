using Moq.Sdk;
using Stunts;

namespace Moq
{
    public class MockNamingConvention : NamingConvention
    {
        public override string Namespace => MockNaming.Namespace;

        public override string NameSuffix => MockNaming.NameSuffix;
    }
}
