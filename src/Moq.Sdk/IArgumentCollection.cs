using System.Collections.Generic;
using System.Reflection;

namespace Moq.Sdk
{
    /// <summary>
    /// Represents the arguments of a method invocation.
    /// </summary>
	public interface IArgumentCollection : IEnumerable<object>
	{
        /// <summary>
        /// Whether the collection contains a parameter with the 
        /// given name (based on the <see cref="ParameterInfo"/>s
        /// for the collection, rather than the actual values, since 
        /// they could be null/missing.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		bool Contains(string name);

        /// <summary>
        /// Count <see cref="ParameterInfo"/>s in the collection.
        /// </summary>
		int Count { get; }

        /// <summary>
        /// Get the <see cref="ParameterInfo"/> at the given index.
        /// </summary>
		ParameterInfo GetInfo(int index);

        /// <summary>
        /// Gets the <see cref="ParameterInfo"/> with the given name.
        /// </summary>
		ParameterInfo GetInfo(string name);

        /// <summary>
        /// Gets the index of the parameter with the given name or <c>-1</c>
        /// if it's not found.
        /// </summary>
        int IndexOf(string name);

        /// <summary>
        /// Gets the name of the parameter at the given index.
        /// </summary>
        string NameOf(int index);

        /// <summary>
        /// Gets or sets the value of the argument with the given name. 
        /// </summary>
        object this[string name] { get; set; }

        /// <summary>
        /// Gets or sets the value of the argument at the given index.
        /// </summary>
		object this[int index] { get; set; }
	}
}
