using Sample;
using Moq.Sdk;
using Xunit;
using Xunit.Abstractions;
using Moq.Tests.Recursive;

namespace Moq.Tests
{
    public class DynamicProxyTests : MoqTests
    {
        public DynamicProxyTests(ITestOutputHelper output) : base(output)
        {
            MockFactory.Default = new DynamicMockFactory();
        }
    }

    public class DynamicRecursiveMockTests : RecursiveMocksTests
    {
        public DynamicRecursiveMockTests()
        {
            MockFactory.Default = new DynamicMockFactory();
        }
    }

    public class DynamicRefOutTests : RefOut.RefOutTests
    {
        public DynamicRefOutTests(ITestOutputHelper output) : base(output)
        {
            MockFactory.Default = new DynamicMockFactory();
        }
    }

    public class DynamicVerificationTests : VerificationTests
    {
        public DynamicVerificationTests(ITestOutputHelper output) : base(output)
        {
            MockFactory.Default = new DynamicMockFactory();
            //Mock.Of<BaseWithCtor>("foo");
            //output.WriteLine(DynamicMockFactory.generator.ProxyBuilder.ModuleScope.SaveAssembly());
        }

        [Fact]
        public void ctor() { }

        [Fact]
        public void VerifySyntaxOnceOnVerify1() => VerifySyntaxOnceOnVerify();
    }

    public class BaseWithCtor
    {
        public BaseWithCtor(string value)
        {

        }
    }
}