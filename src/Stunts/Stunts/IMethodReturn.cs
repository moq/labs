using System;
using System.Collections.Generic;

namespace Stunts
{
    /// <summary>
    /// Represents the result of invoking a method.
    /// </summary>
	public interface IMethodReturn
	{
        /// <summary>
        /// An arbitrary property bag used during the invocation.
        /// </summary>
        IDictionary<string, object> Context { get; }

        /// <summary>
        /// Optional exception if the method invocation results in 
        /// an exception being thrown.
        /// </summary>
		Exception Exception { get; }

        /// <summary>
        /// Collection of out/ref arguments.
        /// </summary>
        IArgumentCollection Outputs { get; }

        /// <summary>
        /// The value being returned for a non-void method if no exception 
        /// was thrown.
        /// </summary>
		object ReturnValue { get; }
	}
}