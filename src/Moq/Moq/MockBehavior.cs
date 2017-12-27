namespace Moq
{
	/// <summary>
	/// Options to customize the behavior of the mock. 
	/// </summary>
	public enum MockBehavior
	{
        /// <summary>
        /// Will never throw exceptions, returning default  
        /// values when necessary (null for reference types, 
        /// zero for value types or empty enumerables and arrays).
        /// </summary>
        Loose,
        /// <summary>
        /// Causes the mock to always throw 
        /// an exception for invocations that don't have a 
        /// corresponding setup.
        /// </summary>
        Strict,
	}
}
