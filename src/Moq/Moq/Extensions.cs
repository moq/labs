using Stunts;
using System.Reflection;
using System;

namespace Moq
{
    static class Extensions
    {
        public static void EnsureCompatible(this IMethodInvocation invocation, Delegate @delegate)
        {
            var method = @delegate.GetMethodInfo();
            if (invocation.Arguments.Count != method.GetParameters().Length)
                throw new ArgumentException("Callback arguments do not match target invocation arguments.");

            // TODO: validate assignability
        }
    }
}
