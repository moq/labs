using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Stunts
{
	/// <summary>
	/// Default implementation of <see cref="IMethodReturn"/>.
	/// </summary>
	class MethodReturn : IMethodReturn
	{
        IMethodInvocation invocation;
        object[] allArguments;

        public MethodReturn(IMethodInvocation invocation, object returnValue, object[] allArguments)
		{
            this.invocation = invocation;
            this.allArguments = allArguments;

			ReturnValue = returnValue;

			var outputArgs = new List<object>();
			var outputInfos = new List<ParameterInfo>();
			var allInfos = invocation.MethodBase.GetParameters();

			for (var i = 0; i < allInfos.Length; i++)
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
			Outputs = new ArgumentCollection(new object[0], new ParameterInfo[0]);
			Exception = exception;
		}

		/// <summary>
		/// The collection of output parameters. If the method has no output
		/// parameters, this is a zero-length list (never null).
		/// </summary>
		public IArgumentCollection Outputs { get; }

		public object ReturnValue { get; }

		public Exception Exception { get; }

		public IDictionary<string, object> Context { get => invocation.Context; }

        public override string ToString()
        {
            var result = new StringBuilder();
            if (invocation.MethodBase is MethodInfo info)
            {
                if (info.ReturnType != typeof(void))
                    result.Append(Stringly.ToTypeName(info.ReturnType)).Append(" ");
                else
                    result.Append("void ");
            }

            result.Append(invocation.MethodBase.Name);
            if (invocation.MethodBase.IsGenericMethod)
            {
                var generic = ((MethodInfo)invocation.MethodBase).GetGenericMethodDefinition();
                result
                    .Append("<")
                    .Append(string.Join(", ", generic.GetGenericArguments().Select(t => t.Name)))
                    .Append(">");
            }

            result
                .Append("(")
                .Append(new ArgumentCollection(allArguments, invocation.MethodBase.GetParameters()).ToString())
                .Append(")");

            if (Exception != null)
                result.Append($" => throw new {Exception.GetType().Name}(\"{Exception.Message}\")");
            else if (invocation.MethodBase is MethodInfo r && r.ReturnType != typeof(void))
                result.Append(" => ")
                    .Append(ReturnValue == null ? "null" : (r.ReturnType == typeof(string) ? $"\"{ReturnValue}\"" : ReturnValue));

            return result.ToString();
        }
    }
}
