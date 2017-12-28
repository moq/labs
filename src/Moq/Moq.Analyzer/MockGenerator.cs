using System.Linq;
using Moq.Processors;
using Stunts;

namespace Moq
{
    class MockGenerator : StuntGenerator
    {
        public MockGenerator(NamingConvention naming)
            : base(naming, new IDocumentProcessor[]
                {
                    new EnsureSdkReference(),
                    new CSharpMocked(),
                    new VisualBasicMocked(),
                }.Concat(GetDefaultProcessors()).ToArray())
        {
        }
    }
}
