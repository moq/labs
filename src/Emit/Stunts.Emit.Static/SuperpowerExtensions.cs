using System.Diagnostics;
using System.Threading;
using Superpower;
using Superpower.Parsers;

namespace Stunts.Emit
{
    internal static class SuperpowerExtensions
    {
        static int id;
        
        public static TextParser<T> Log<T>(this TextParser<T> parser, string name)
        {
#if !SUPERPOWER
            return parser;
#endif

            if (!Debugger.IsAttached)
                return parser;

            var current = Interlocked.Increment(ref id);
            Debug.WriteLine($"[{current}] Constructing instance of {name}");
            return i =>
            {
                Debug.WriteLine($"[{current}] Invoking with input: {i}");
                var result = parser(i);
                Debug.WriteLine($"[{current}] Result: {result}");
                return result;
            };
        }

        public static TextParser<T> Token<T>(this TextParser<T> parser)
            => from _ in Span.WhiteSpace.Optional()
               from value in parser
               from __ in Span.WhiteSpace.Optional()
               select value;
    }
}
