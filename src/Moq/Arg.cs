using System.Collections.Generic;
using Moq.Sdk;

namespace Moq
{
    public static class Arg
    {
        public static T Any<T>()
        {
            CallContext<Stack<IArgumentMatcher>>.GetData(nameof(IArgumentMatcher), () => new Stack<IArgumentMatcher>())
                .Push(AnyValueMatcher<T>.Default);

            return default(T);
        }
    }
}
