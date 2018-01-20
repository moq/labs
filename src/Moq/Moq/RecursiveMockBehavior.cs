using System;
using System.Linq;
using System.Reflection;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// 
    /// </summary>
    public class RecursiveMockBehavior : IStuntBehavior
    {
        public bool AppliesTo(IMethodInvocation invocation)
            => SetupScope.IsActive;

        public IMethodReturn Invoke(IMethodInvocation invocation, GetNextBehavior getNext)
        {
            if (invocation.MethodBase is MethodInfo info &&
                info.ReturnType != typeof(void) && 
                info.ReturnType.CanBeIntercepted())
            {
                var result = getNext().Invoke(invocation, getNext);
                if (result.ReturnValue == null)
                {
                    // Turn the null value into a mock for the current invocation setup
                    var currentMock = ((IMocked)invocation.Target).Mock;
                    var recursiveMock = ((IMocked)MockFactory.Default.CreateMock(
                        // Use the same assembly as the current target
                        invocation.Target.GetType().GetTypeInfo().Assembly, 
                        info.ReturnType, 
                        new Type[0], 
                        new object[0])).Mock;

                    // Clone the current mock's behaviors, except for the setups
                    foreach (var behavior in currentMock.Behaviors.Where(x => !(x is IMockBehavior)))
                        recursiveMock.Behaviors.Add(behavior);

                    // Set up the current invocation to return the created value
                    var setup = currentMock.BehaviorFor(MockContext.CurrentSetup);
                    var returnBehavior = setup.Behaviors.OfType<ReturnsBehavior>().FirstOrDefault();
                    if (returnBehavior != null)
                        returnBehavior.ValueGetter = () => recursiveMock.Object;
                    else
                        setup.Behaviors.Add(new ReturnsBehavior(() => recursiveMock.Object));

                    // Copy over values from the result, so that outputs contain the default values.
                    var arguments = invocation.Arguments.ToArray();
                    var parameters = invocation.MethodBase.GetParameters();
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        // This covers both out & ref
                        if (parameter.ParameterType.IsByRef)
                            arguments[i] = result.Outputs[parameter.Name];
                    }

                    return invocation.CreateValueReturn(recursiveMock.Object, arguments);
                }

                return result;
            }

            return getNext().Invoke(invocation, getNext);
        }
    }
}
