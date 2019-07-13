using System.Linq;
using System.Reflection;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// A <see cref="IStuntBehavior"/> that returns default values from an 
    /// invocation, both for the method return type as well as any out/ref 
    /// parameters.
    /// </summary>
    public class DefaultValueBehavior : IStuntBehavior
    {
        /// <summary>
        /// Initializes the behavior with a default <see cref="DefaultValueProvider"/>.
        /// </summary>
        public DefaultValueBehavior()
            : this(new DefaultValueProvider()) { }

        /// <summary>
        /// Initializes the behavior with a specific <see cref="DefaultValueProvider"/>.
        /// </summary>
        public DefaultValueBehavior(DefaultValueProvider provider) => Provider = provider;

        /// <summary>
        /// Gets or sets the provider of default values for the behavior.
        /// </summary>
        public DefaultValueProvider Provider { get; set; }

        /// <summary>
        /// Always returns <see langword="true" />
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation) => true;

        /// <summary>
        /// Fills in the ref, out and return values with the defaults determined 
        /// by the <see cref="DefaultValueProvider"/> utility class.
        /// </summary>
        IMethodReturn IStuntBehavior.Execute(IMethodInvocation invocation, GetNextBehavior next)
        {
            var arguments = invocation.Arguments?.ToArray() ?? new object[0];
            var parameters = invocation.MethodBase?.GetParameters() ?? new ParameterInfo[0];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                // Only provide default values for out parameters. 
                // NOTE: does not touch ByRef values.
                if (parameter.IsOut)
                    arguments[i] = Provider.GetDefault(parameter.ParameterType);
            }

            var returnValue = default(object);
            if (invocation.MethodBase is MethodInfo info &&
                info.ReturnType != typeof(void))
            {
                returnValue = Provider.GetDefault(info.ReturnType);
            }

            return invocation.CreateValueReturn(returnValue, arguments);
        }
    }
}
