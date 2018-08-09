using System;
using System.Reflection;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Instantiates mocks for the specified types.
    /// </summary>
    partial class Mock
    {
        /// <summary>
        /// Creates the mock instance by using the specified types to 
        /// lookup the mock type in the assembly defining this class.
        /// </summary>
        static T Create<T>(MockBehavior behavior, object[] constructorArgs, params Type[] interfaces)
        {
            var mocked = (IMocked)MockFactory.Default.CreateMock(typeof(Mock).GetTypeInfo().Assembly, typeof(T), interfaces, constructorArgs);

            mocked.SetBehavior(behavior);

            return (T)mocked;
        }
   }
}