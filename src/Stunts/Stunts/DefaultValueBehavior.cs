using System.Linq;
using System.Reflection;

namespace Stunts
{
    /// <summary>
    /// A <see cref="IStuntBehavior"/> that returns default values from an 
    /// invocation, both for the method return type as well as any out/ref 
    /// parameters.
    /// </summary>
    public class DefaultValueBehavior : IStuntBehavior
    {
        /// <summary>
        /// Gets or sets the provider of default values for the behavior.
        /// </summary>
        public DefaultValue Provider { get; set; } = new DefaultValue();

        /// <summary>
        /// Always returns <see langword="true" />
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => true;

        /// <summary>
        /// Fills in the ref, out and return values with the defaults determined 
        /// by the <see cref="DefaultValue"/> utility class.
        /// </summary>
        IMethodReturn IStuntBehavior.Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            var arguments = invocation.Arguments.ToArray();
            var parameters = invocation.MethodBase.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                // Only provide default values for out parameters. 
                // NOTE: does not touch ByRef values.
                if (parameter.IsOut)
                    arguments[i] = Provider.For(parameter.ParameterType);
            }

            var returnValue = default(object);
            if (invocation.MethodBase is MethodInfo info &&
                info.ReturnType != typeof(void))
            {
                returnValue = Provider.For(info.ReturnType);
            }

            return invocation.CreateValueReturn(returnValue, arguments);
        }
    }
}
