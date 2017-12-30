using Xunit;

namespace Stunts.Tests
{
#pragma warning disable xUnit1013 // Public method should be marked as test
    public class MethodInvocationTests
    {
        public void Do() { }

        [Fact]
        public void TestDo()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(Do)));

            var actual = invocation.ToString();

            Assert.Equal("void Do()", actual);
        }

        public void DoWithInt(int value) { }

        [Fact]
        public void TestDoWithInt()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(DoWithInt)), 5);

            var actual = invocation.ToString();

            Assert.Equal("void DoWithInt(int value = 5)", actual);
        }

        public void DoWithNullableInt(int? value) { }

        [Fact]
        public void TestDoWithNullableInt()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(DoWithNullableInt)), 5);

            var actual = invocation.ToString();

            Assert.Equal("void DoWithNullableInt(int? value = 5)", actual);
        }

        public void DoWithNullableIntNull(int? value) { }

        [Fact]
        public void TestDoWithNullableIntNull()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(DoWithNullableIntNull)), default(int?));

            var actual = invocation.ToString();

            Assert.Equal("void DoWithNullableIntNull(int? value = null)", actual);
        }

        public void DoWithString(string value) { }

        [Fact]
        public void TestDoWithString()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(DoWithString)), "foo");

            var actual = invocation.ToString();

            Assert.Equal("void DoWithString(string value = \"foo\")", actual);
        }

        public void DoWithNullString(string value) { }

        [Fact]
        public void TestDoWithNullString()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(DoWithNullString)), default(string));

            var actual = invocation.ToString();

            Assert.Equal("void DoWithNullString(string value = null)", actual);
        }

        public bool DoReturn() => true;

        [Fact]
        public void TestDoReturn()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(DoReturn)));

            var actual = invocation.ToString();

            Assert.Equal("bool DoReturn()", actual);
        }

        public void DoRef(ref int i) { }

        [Fact]
        public void TestDoRef()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(DoRef)), 5);

            var actual = invocation.ToString();

            Assert.Equal("void DoRef(ref int i = 5)", actual);
        }

        public void DoOut(out int value) { value = 5; }

        [Fact]
        public void TestDoOut()
        {
            var invocation = new MethodInvocation(this, typeof(MethodInvocationTests).GetMethod(nameof(DoOut)), 5);

            var actual = invocation.ToString();

            Assert.Equal("void DoOut(out int value)", actual);
        }
    }
#pragma warning restore xUnit1013 // Public method should be marked as test
}
