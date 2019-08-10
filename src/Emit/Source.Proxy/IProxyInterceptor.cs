namespace Moq.Proxy
{
	/// <summary>
	/// Defines a contract to interact with proxy method invocations.
	/// </summary>
	public interface IProxyInterceptor
	{
		/// <summary>
		/// Invokes the implementation of a proxy method.
		/// </summary>
		/// <param name="input">The input arguments.</param>
		/// <returns>The return information of the method.</returns>
		IMethodReturn Invoke(IMethodCall input);
	}
}