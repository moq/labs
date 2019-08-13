using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Stunts;
using Stunts.Emit.Static;
using Xunit;
using Xunit.Abstractions;

namespace Emit.Tests
{
    public class Foo
    {
        public void Do() => throw new NotImplementedException();
    }

    public class CecilTests
    {
        static readonly string generatorAttribute = typeof(StuntGeneratorAttribute).FullName;

        readonly ITestOutputHelper output;

        public CecilTests(ITestOutputHelper output) => this.output = output;

        public void Stunter()
        {
            //Stunt.Of<IDisposable>();
            Stunt.Of<IDictionary<string[][], IList<IEnumerable<KeyValuePair<string, int>>>>>();
        }

        [Fact]
        public void Inspect()
        {
            using (var generator = new StuntsGenerator(typeof(CecilTests).Assembly.Location))
                generator.Emit();
        }
    }
}
