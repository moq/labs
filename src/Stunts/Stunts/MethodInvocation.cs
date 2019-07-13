using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections;
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

        /// <summary>
        /// The arguments of the method invocation.
        /// </summary>
        public IArgumentCollection Arguments { get; }

        /// <summary>
        /// An arbitrary property bag used during the invocation.
        /// </summary>
        public IDictionary<string, object> Context { get; }

        /// <summary>
        /// The runtime method being invoked.
        /// </summary>
        public MethodBase MethodBase { get; }

        /// <summary>
        /// The ultimate target of the method invocation, typically 
        /// a stunt object.
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// Creates the method invocation return that ends the 
        /// current invocation with an exception.
        /// </summary>
        /// <param name="exception">The exception to throw from the method invocation.</param>
        /// <returns>The <see cref="IMethodReturn"/> for the current invocation.</returns>
        public IMethodReturn CreateExceptionReturn(Exception exception) 
            => new MethodReturn(this, exception);

        /// <summary>
        /// Creates a method invocation return that represents 
        /// a thrown exception.
        /// </summary>
        /// <param name="returnValue">Optional return value from the method invocation. <see langword="null"/> for <see langword="void"/> methods.</param>
        /// <param name="allArguments">Ordered list of all arguments to the method invocation, including ref/out arguments.</param>
        /// <returns>The <see cref="IMethodReturn"/> for the current invocation.</returns>
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