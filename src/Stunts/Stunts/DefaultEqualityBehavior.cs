using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Stunts
{
    /// <summary>
    /// A <see cref="IStuntBehavior"/> that implements a stunt's 
    /// <c>GetHashCode</c> and <c>Equals</c> using <see cref="object.GetHashCode"/> 
    /// and <see cref="object.ReferenceEquals(object, object)"/> respectively.
    /// </summary>
    public class DefaultEqualityBehavior : IStuntBehavior
    {
        /// <summary>
        /// Always returns <see langword="true" />
        /// </summary>
        public bool AppliesTo(IMethodInvocation invocation)
            => invocation.MethodBase.Name == nameof(GetHashCode) || invocation.MethodBase.Name == nameof(Equals);

        /// <summary>
        /// Fills in the ref, out and return values with the defaults determined 
        /// by the <see cref="DefaultValue"/> utility class.
        /// </summary>
        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            if (invocation.MethodBase.Name == nameof(GetHashCode))
                return invocation.CreateValueReturn(RuntimeHelpers.GetHashCode(invocation.Target));
            else if (invocation.MethodBase.Name == nameof(Equals))
                return invocation.CreateValueReturn(object.ReferenceEquals(invocation.Target, invocation.Arguments[0]));

            return getNext().Invoke(invocation, getNext);
        }
    }
}