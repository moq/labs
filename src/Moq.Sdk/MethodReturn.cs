using System;
using System.Collections.Generic;
using System.Reflection;

namespace Moq.Sdk
{
	/// <summary>
	/// Default implementation of <see cref="IMethodReturn"/>.
	/// </summary>
	class MethodReturn : IMethodReturn
	{
		public MethodReturn(IMethodInvocation invocation, object returnValue, object[] allArguments)
		{
			Context = invocation.Context;
			ReturnValue = returnValue;

			var outputArgs = new List<object>();
			var outputInfos = new List<ParameterInfo>();
			var allInfos = invocation.MethodBase.GetParameters();

			for (int i = 0; i < allInfos.Length; i++)
			{
				var info = allInfos[i];
				if (info.ParameterType.IsByRef)
				{
					outputArgs.Add(allArguments[i]);
					outputInfos.Add(info);
				}
			}

			Outputs = new ArgumentCollection(outputArgs, outputInfos);
		}

		public MethodReturn(IMethodInvocation invocation, Exception exception)
		{
			Context = invocation.Context;
			Outputs = new ArgumentCollection(new object[0], new ParameterInfo[0]);
			Exception = exception;
		}

		/// <summary>
		/// The collection of output parameters. If the method has no output
		/// parameters, this is a zero-length list (never null).
		/// </summary>
		public IArgumentCollection Outputs { get; }

		public object ReturnValue { get; set; }

		public Exception Exception { get; set; }

		public IDictionary<string, object> Context { get; }
	}
}
