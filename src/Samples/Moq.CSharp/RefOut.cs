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

        delegate bool TryParse(string input, out DateTimeOffset date);
    }
}