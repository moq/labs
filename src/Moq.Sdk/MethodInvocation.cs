using System;
using System.Collections.Generic;
using System.Reflection;

namespace Moq.Sdk
{
	/// <summary>
	/// Default implementation of <see cref="IMethodInvocation"/>.
	/// </summary>
	class MethodInvocation : IMethodInvocation
	{
		public MethodInvocation(object target, MethodBase method, params object[] arguments)
		{
            // TODO: validate that arguments length and type match the method info?
			Target = target;
			MethodBase = method;
			Arguments = new ArgumentCollection(arguments, method.GetParameters());
			Context = new Dictionary<string, object>();
		}

		public IArgumentCollection Arguments { get; }

		public IDictionary<string, object> Context { get; }

		public MethodBase MethodBase { get; }

		public object Target { get; }

		public IMethodReturn CreateExceptionReturn(Exception exception) => new MethodReturn(this, exception);

		public IMethodReturn CreateValueReturn(object returnValue, params object[] allArguments) => new MethodReturn(this, returnValue, allArguments);
	}
}