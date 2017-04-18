using System;
using System.Collections.Generic;
using System.Reflection;

namespace Moq.Proxy
{
    /// <summary>
    /// Represents a method invocation.
    /// </summary>
	public interface IMethodInvocation
	{
        /// <summary>
        /// The arguments of the method invocation.
        /// </summary>
		IArgumentCollection Arguments { get; }

        /// <summary>
        /// An arbitrary property bag used during the invocation.
        /// </summary>
		IDictionary<string, object> Context { get; }

        /// <summary>
        /// The runtime method being invoked.
        /// </summary>
		MethodBase MethodBase { get; }

        /// <summary>
        /// The ultimate target of the method invocation, typically 
        /// a proxy object.
        /// </summary>
		object Target { get; }

        /// <summary>
        /// Creates the method invocation return that ends the 
        /// current invocation.
        /// </summary>
        /// <param name="returnValue">Optional return value from the method invocation. <see langword="null"/> for <see langword="void"/> methods.</param>
        /// <param name="allArguments">Ordered list of all arguments to the method invocation, including ref/out arguments.</param>
        /// <returns>The <see cref="IMethodReturn"/> for the current invocation.</returns>
		IMethodReturn CreateValueReturn(object returnValue, params object[] allArguments);

        /// <summary>
        /// Creates a method invocation return that represents 
        /// a thrown exception.
        /// </summary>
        /// <param name="exception">The exception to throw from the method invocation.</param>
        /// <returns>The <see cref="IMethodReturn"/> for the current invocation.</returns>
		IMethodReturn CreateExceptionReturn(Exception exception);
	}
}