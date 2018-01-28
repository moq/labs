using System;

namespace Moq.CSharp
{
    public interface IParser
    {
        bool TryParse(string input, out DateTimeOffset date);
    }
}
