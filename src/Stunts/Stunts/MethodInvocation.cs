using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using TypeNameFormatter;
using System.Diagnostics;

namespace Stunts
{
    /// <summary>
    /// Default implementation of <see cref="IMethodInvocation"/>.
    /// </summary>
    public class MethodInvocation : IEquatable<MethodInvocation>, IMethodInvocation
    {
        /// <summary>
        /// Initializes the <see cref="MethodInvocation"/> with the given parameters.
        /// </summary>
        /// <param name="target">The target object where the invocation is being performed.</param>
        /// <param name="method">The method being invoked.</param>
        /// <param name="arguments">The optional arguments passed to the method invocation.</param>
        public MethodInvocation(object target, MethodBase method, params object[] arguments)
        {
            // TODO: validate that arguments length and type match the method info?
            Target = target ?? throw new ArgumentNullException(nameof(target));
            MethodBase = method ?? throw new ArgumentNullException(nameof(method));
            Arguments = new ArgumentCollection(arguments, method.GetParameters());
            Context = new Dictionary<string, object>();
        }

        /// <inheritdoc />
        public IArgumentCollection Arguments { get; }

        /// <inheritdoc />
        public IDictionary<string, object> Context { get; }

        /// <inheritdoc />
        public MethodBase MethodBase { get; }

        /// <inheritdoc />
        public object Target { get; }

        /// <inheritdoc />
        public HashSet<Type> SkipBehaviors { get; } = new HashSet<Type>();

        /// <inheritdoc />
        public IMethodReturn CreateExceptionReturn(Exception exception) 
            => new MethodReturn(this, exception);

        /// <inheritdoc />
        public IMethodReturn CreateValueReturn(object returnValue, params object[] allArguments) 
            => new MethodReturn(this, returnValue, allArguments);

        /// <summary>
        /// Gets a friendly representation of the invocation.
        /// </summary>
        /// <devdoc>
        /// We don't want to optimize code coverage for this since it's a debugger aid only. 
        /// Annotating this method with DebuggerNonUserCode achieves that.
        /// No actual behavior depends on these strings.
        /// </devdoc>
        [DebuggerNonUserCode]
        public override string ToString()
        {
            var result = new StringBuilder();
            if (MethodBase is MethodInfo info)
            {
                if (info.ReturnType != typeof(void))
                    result.AppendFormattedName(info.ReturnType).Append(" ");
                else
                    result.Append("void ");
            }

            if (MethodBase.IsSpecialName)
            {
                if (MethodBase.Name.StartsWith("get_", StringComparison.Ordinal))
                {
                    result.Append(MethodBase.Name.Substring(4));
                }
                else if(MethodBase.Name.StartsWith("set_", StringComparison.Ordinal))
                {
                    result.Append(MethodBase.Name.Substring(4));
                    result.Append(" = ").Append(Arguments[0]?.ToString() ?? "null");
                }
                else if (MethodBase.Name.StartsWith("add_", StringComparison.Ordinal))
                {
                    result.Append(MethodBase.Name.Substring(4) + " += ");
                }
                else if (MethodBase.Name.StartsWith("remove_", StringComparison.Ordinal))
                {
                    result.Append(MethodBase.Name.Substring(7) + " -= ");
                }
            }
            else
            {
                result.Append(MethodBase.Name);
            }

            if (MethodBase.IsGenericMethod)
            {
                var generic = ((MethodInfo)MethodBase).GetGenericMethodDefinition();
                result
                    .Append("<")
                    .Append(string.Join(", ", generic.GetGenericArguments().Select(t => t.Name)))
                    .Append(">");
            }

            // TODO: render indexer arguments?
            if (!MethodBase.IsSpecialName)
            {
                return result
                    .Append("(")
                    .Append(Arguments.ToString())
                    .Append(")")
                    .ToString();
            }

            return result.ToString();
        }

        #region Equality

        /// <summary>
        /// Tests the current invocation against another for equality, taking into account the target object 
        /// for reference equality, the object equality of both <see cref="MethodBase"/> and the sequence and 
        /// equality for all <see cref="Arguments"/>.
        /// </summary>
        /// <param name="other">The invocation to compare against.</param>
        /// <returns><see langword="true"/> if the invocations are equal, <see langword="false"/> otherwise.</returns>
        public bool Equals(IMethodInvocation other)
            => other != null && object.ReferenceEquals(Target, other.Target) && MethodBase.Equals(other.MethodBase) && Arguments.SequenceEqual(other.Arguments);

        /// <summary>
        /// Tests the current invocation against another for equality, taking into account the target object 
        /// for reference equality, the object equality of both <see cref="MethodBase"/> and the sequence and 
        /// equality for all <see cref="Arguments"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the invocations are equal, <see langword="false"/> otherwise.</returns>
        public bool Equals(MethodInvocation other)
            => Equals((IMethodInvocation)other);

        /// <summary>
        /// Tests the current invocation against another for equality, taking into account the target object 
        /// for reference equality, the object equality of both <see cref="MethodBase"/> and the sequence and 
        /// equality for all <see cref="Arguments"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the invocations are equal, <see langword="false"/> otherwise.</returns>
        public override bool Equals(object obj) 
            => Equals(obj as IMethodInvocation);

        /// <summary>
        /// Gets the hash code for the current invocation, including the <see cref="Target"/>, <see cref="MethodBase"/> 
        /// and <see cref="Arguments"/>.
        /// </summary>
        public override int GetHashCode() 
            => new HashCode().Combine(RuntimeHelpers.GetHashCode(Target)).Add(MethodBase).AddRange(Arguments).ToHashCode();

        #endregion
    }
}