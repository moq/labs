using System;
using System.Collections.Generic;
using Moq.Sdk;

namespace Moq
{
    public static class Arg
    {
        public static T Any<T>()
        {
            CallContext<Stack<IArgumentMatcher>>.GetData(nameof(IArgumentMatcher), () => new Stack<IArgumentMatcher>())
                .Push(AnyArgumentMatcher<T>.Default);

            return default(T);
        }

        public static T Is<T>(Func<T, bool> condition)
        {
            CallContext<Stack<IArgumentMatcher>>.GetData(nameof(IArgumentMatcher), () => new Stack<IArgumentMatcher>())
                .Push(new IsArgumentMatcher<T>(condition));

            return default(T);
        }
    }
}
