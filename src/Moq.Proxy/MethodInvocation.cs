using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Moq.Proxy
{
	/// <summary>
	/// Default implementation of <see cref="IMethodInvocation"/>.
	/// </summary>
	public class MethodInvocation : IMethodInvocation
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

        public override string ToString()
        {
            var result = new StringBuilder();
            if (MethodBase is MethodInfo info)
            {
                if (info.ReturnType != typeof(void))
                    result.Append(Stringly.ToTypeName(info.ReturnType)).Append(" ");
                else
                    result.Append("void ");
            }
            
            result.Append(MethodBase.Name);
            if (MethodBase.IsGenericMethod)
            {
                var generic = ((MethodInfo)MethodBase).GetGenericMethodDefinition();
                result
                    .Append("<")
                    .Append(string.Join(", ", generic.GetGenericArguments().Select(t => t.Name)))
                    .Append(">");
            }

            result
                .Append("(")
                .Append(Arguments.ToString())
                .Append(")");
            
            return result.ToString();
        }
    }
}