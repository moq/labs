﻿using System.Linq;
using System.Threading;
using Moq.Processors;
using Moq.Sdk;
using Avatars;
using Avatars.CodeAnalysis;
using Avatars.Processors;

namespace Moq
{
    /// <summary>
    /// Customizes the Avatar.Sdk <see cref="AvatarDocumentGenerator"/> 
    /// with Moq-specific document processors.
    /// </summary>
    internal class MockDocumentGenerator : AvatarDocumentGenerator
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
                }).ToArray())
        {
            GeneratorAttribute = typeof(MockGeneratorAttribute);
        }
    }
}
