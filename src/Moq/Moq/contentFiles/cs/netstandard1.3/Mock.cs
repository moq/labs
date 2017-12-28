using System;
using System.Reflection;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Instantiates stunts for the specified types.
    /// </summary>
    internal partial class Mock
    {
        static T Create<T>(MockBehavior behavior, object[] constructorArgs, params Type[] interfaces)
        {
            var mocked = (IMocked)MockFactory.Default.CreateMock(typeof(Mock).GetTypeInfo().Assembly, typeof(T), interfaces, constructorArgs);

            mocked.Mock.Behaviors.Add(new MockTrackingBehavior());

            if (behavior == MockBehavior.Strict)
            {
                mocked.Mock.Behaviors.Add(new PropertyBehavior { SetterRequiresSetup = true });
                mocked.Mock.Behaviors.Add(new StrictMockBehavior());
            }
            else
            {
                mocked.Mock.Behaviors.Add(new PropertyBehavior());
                mocked.Mock.Behaviors.Add(new DefaultValueBehavior());
            }

            mocked.Mock.Behaviors.Add(new DefaultEqualityBehavior());

            return (T)mocked;
        }
   }
}