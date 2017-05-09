using System;
using System.Collections.Generic;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Allows matching arguments in an invocation.
    /// </summary>
    public static class Arg
    {
        /// <summary>
        /// Matches any value of the given type.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        public static T Any<T>()
        {
            CallContext<Stack<IArgumentMatcher>>.GetData(nameof(IArgumentMatcher), () => new Stack<IArgumentMatcher>())
                .Push(AnyArgumentMatcher<T>.Default);

            return default(T);
        }

        /// <summary>
        /// Matches a value of the given type if it satisfies the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="condition">The condition to check against actual invocation values.</param>
        public static T Any<T>(Func<T, bool> condition)
        {
            CallContext<Stack<IArgumentMatcher>>.GetData(nameof(IArgumentMatcher), () => new Stack<IArgumentMatcher>())
                .Push(new IsArgumentMatcher<T>(condition));

            return default(T);
        }
    }

    /// <summary>
    /// Allows matching arguments of the given type <typeparamref name="T"/> 
    /// in an invocation.
    /// </summary>
    public static class Arg<T>
    {
        /// <summary>
        /// Matches any argument value with a matching type.
        /// </summary>
        public static T Any
        {
            get
            {
                CallContext<Stack<IArgumentMatcher>>.GetData(nameof(IArgumentMatcher), () => new Stack<IArgumentMatcher>())
                    .Push(AnyArgumentMatcher<T>.Default);
                return default(T);
            }
        }
    }
}