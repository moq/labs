using System.Linq;
using System.Threading;
using Moq.Processors;
using Moq.Sdk;
using Stunts;
using Stunts.CodeAnalysis;
using Stunts.Processors;

namespace Moq
{
    /// <summary>
    /// Customizes the Stunts.Sdk <see cref="StuntGenerator"/> 
    /// with Moq-specific document processors.
    /// </summary>
    internal class MockDocumentGenerator : StuntDocumentGenerator
    {
        public MockDocumentGenerator() : this(new MockNamingConvention()) { }

        public MockDocumentGenerator(NamingConvention naming)
            : base(naming, new IDocumentProcessor[]
                {
                    new DefaultImports(typeof(LazyInitializer).Namespace!, typeof(IMocked).Namespace!),
                }
                .Concat(DefaultProcessors.Where(p => !(p is FixupImports)))
                .Concat(new IDocumentProcessor[]
                {
                    new CSharpMocked(),
                    new VisualBasicMocked(),
                    new FixupImports(),
                    new CSharpFileHeader(),
                    new CSharpPragmas(),
                    new VisualBasicFileHeader(),
                }).ToArray())
        {
            GeneratorAttribute = typeof(MockGeneratorAttribute);
        }
    }
}
