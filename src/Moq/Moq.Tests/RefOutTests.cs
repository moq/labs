using Xunit;
using static Moq.Syntax;

namespace Moq.Tests.RefOut
{
    public class RefOutTests
    {
        [Fact]
        public void CanUseRefOut()
        {
            var mock = Mock.Of<IRefOut>();
            var refstring = "bar";
            var outstring = "baz";

            mock.Try("foo", ref refstring, out outstring)
                .Returns(true);

            Assert.True(mock.Try("foo", ref refstring, out _));
        }

        [Fact]
        public void CanSetRefOutReturns()
        {
            var mock = Mock.Of<IRefOut>();
            var refstring = "bar";
            string outstring;

            mock.Try("foo", ref refstring, out outstring)
                .Returns(c =>
                {
                    c[1] = "ref";
                    c[2] = "out";
                    return true;
                });

            Assert.True(mock.Try("foo", ref refstring, out outstring));
            Assert.Equal("ref", refstring);
            Assert.Equal("out", outstring);
        }
    }

    public interface IRefOut
    {
        bool Try(string input, ref string refstring, out string outstring);
    }
}
