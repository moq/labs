using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Stunts;

namespace Moq.Sdk
{
    public partial class MockSetup : IMockSetup, IEquatable<MockSetup>
    {
        readonly IMethodInvocation invocation;
        readonly IArgumentMatcher[] matchers;
        readonly Lazy<IStructuralEquatable> equatable;

        public MockSetup(IMethodInvocation invocation, IArgumentMatcher[] matchers)
        {
            this.invocation = invocation;
            this.matchers = matchers;
            equatable = new Lazy<IStructuralEquatable>(CreateEquatable);
        }

        public IMethodInvocation Invocation => invocation;

        public IArgumentMatcher[] Matchers => matchers;

        public bool AppliesTo(IMethodInvocation actualInvocation)
        {
            if (actualInvocation == null)
                throw new ArgumentNullException(nameof(actualInvocation));

            if (invocation.MethodBase != actualInvocation.MethodBase)
                return false;

            if (actualInvocation.Arguments.Count != matchers.Length)
                return false;

            for (var i = 0; i < actualInvocation.Arguments.Count; i++)
            {
                if (!matchers[i].Matches(actualInvocation.Arguments[i]))
                    return false;
            }

            return true;
        }

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

            var parameters = invocation.MethodBase.GetParameters();

            return result
                .Append("(")
                .Append(string.Join(", ", parameters.Select((p, i) =>
                    Stringly.ToTypeName(p.ParameterType) + " " +
                    p.Name + " = " +
                    matchers[i]
                )))
                .Append(")")
                .ToString();
        }

        IStructuralEquatable CreateEquatable()
        {
            // TODO: Since Func and Action already support a max of 8, maybe that's all we need?
            // TODO: correction, they support 16 :S. Maybe use a tuple item to chain more args?
            if (matchers.Length <= 8)
            {
                switch (matchers.Length)
                {
                    case 0:
                        return Tuple.Create(invocation);
                    case 1:
                        return Tuple.Create(invocation, matchers[0]);
                    case 2:
                        return Tuple.Create(invocation, matchers[0], matchers[1]);
                    case 3:
                        return Tuple.Create(invocation, matchers[0], matchers[1], matchers[2]);
                    case 4:
                        return Tuple.Create(invocation, matchers[0], matchers[1], matchers[2], matchers[3]);
                    case 5:
                        return Tuple.Create(invocation, matchers[0], matchers[1], matchers[2], matchers[3], matchers[4]);
                    case 6:
                        return Tuple.Create(invocation, matchers[0], matchers[1], matchers[2], matchers[3], matchers[4], matchers[5]);
                    case 7:
                        return Tuple.Create(invocation, matchers[0], matchers[1], matchers[2], matchers[3], matchers[4], matchers[5], matchers[6]);
                    case 8:
                        return Tuple.Create(invocation, matchers[0], matchers[1], matchers[2], matchers[3], matchers[4], matchers[5], Tuple.Create(matchers[6], matchers[7]));
                    default:
                        throw new NotSupportedException("A maximum of 8 argument matchers are supported at the moment.");
                }
            }
            else
            {
                throw new NotSupportedException("A maximum of 8 argument matchers are supported at the moment.");
            }
        }

        #region Equality

        public bool Equals(MockSetup other) => equatable.Value.Equals(other?.equatable?.Value);

        public bool Equals(object other, IEqualityComparer comparer) => equatable.Value.Equals((other as MockSetup)?.equatable?.Value, comparer);

        public int GetHashCode(IEqualityComparer comparer) => equatable.Value.GetHashCode(comparer);

        public override bool Equals(object obj) => Equals(obj as MockSetup, EqualityComparer<object>.Default);

        public override int GetHashCode() => equatable.Value.GetHashCode();

        #endregion
    }
}
