using System;
using System.Diagnostics;
using Moq;
using Moq.CSharp;

namespace Sample.CSharp
{
    public class Foo
    {
        public void Test()
        {
            var mock = Mock.Of<IParser>();

            mock.Setup<TryParse>(mock.TryParse)
                .Returns((string input, out DateTimeOffset date) => DateTimeOffset.TryParse(input, out date));

            var expected = DateTimeOffset.Now;
            var value = expected.ToString("O");

            Debug.Assert(mock.TryParse(value, out var actual));
            Debug.Assert(actual == expected);
        }

        public void Test2()
        {
            var mock = Mock.Of<IRefOutParent>();

            var expected = DateTimeOffset.Now;
            var value = expected.ToString("O");

            mock.Setup<TryParse>(() => mock.RefOut.TryParse)
                .Returns((string input, out DateTimeOffset date) => DateTimeOffset.TryParse(value, out date));

            Debug.Assert(mock.RefOut.TryParse(value, out var actual));
            Debug.Assert(actual == expected);
        }

        delegate bool TryParse(string input, out DateTimeOffset date);
    }

    public interface IRefOutParent
    {
        IRefOut RefOut { get; }
    }

    public interface IRefOut
    {
        bool TryParse(string input, out DateTimeOffset date);
    }
}