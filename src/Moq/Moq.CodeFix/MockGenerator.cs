using System.Linq;
using System.Threading;
using Moq.Processors;
using Moq.Sdk;
using Stunts;
using Stunts.Processors;

namespace Moq
{
    /// <summary>
    /// Customizes the Stunts.Sdk <see cref="StuntGenerator"/> 
    /// with Moq-specific document processors.
    /// </summary>
    class MockGenerator : StuntGenerator
    {
        public MockGenerator(NamingConvention naming)
            : base(naming, new IDocumentProcessor[]
                {
                    new DefaultImports(typeof(LazyInitializer).Namespace, typeof(IMocked).Namespace),
                }
                .Concat(GetDefaultProcessors())
                .Concat(new IDocumentProcessor[]
                {
                    new CSharpMocked(),
                    new VisualBasicMocked(),
                    new FixupImports(),
                }).ToArray())
        {
        }
    }
}
