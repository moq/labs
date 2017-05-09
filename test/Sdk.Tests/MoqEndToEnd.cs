using Microsoft.CodeAnalysis;
using Xunit;
using Moq.Proxy;

namespace Moq.Sdk.Tests
{
    public class MoqEndToEnd
    {
        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public void RecordsInvocation(string languageName)
        {
            var calculator = Moq.Of<ICalculator>(languageName);

            calculator.Mode = "Basic";

            var mock = ((IMocked)calculator).Mock;

            var invocations = mock.Invocations;

            Assert.Equal(1, invocations.Count);
        }

        [InlineData(LanguageNames.CSharp)]
        [InlineData(LanguageNames.VisualBasic)]
        [Theory]
        public void AbstractMethodContinues(string languageName)
        {
            var target = Moq.Of<Foo>(languageName);

            target.Do();
        }
    }

    public abstract class Foo
    {
        public abstract void Do();
    }
    chota
    public static class Moq
    {
        public static T Of<T>(string languageName = LanguageNames.CSharp, bool save = false)
            => DynamicProxy.Of<T>(languageName, typeof(IMocked), save)
                .Result
                .AddBehavior(new MockProxyBehavior())
                .AddBehavior(new DefaultValueBehavior());
    }
}