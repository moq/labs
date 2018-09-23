namespace Moq
{
    using System;
    using System.CodeDom.Compiler;
    using System.Reflection;
    using Moq.Sdk;

    /// <summary>
    /// Instantiates mocks for the specified types.
    /// </summary>
    [GeneratedCode("Stunts", "5.0")]
    partial class Mock
    {
        /// <summary>
        /// Creates the mock instance by using the specified types to 
        /// lookup the mock type in the assembly defining this class.
        /// </summary>
        private static T Create<T>(MockBehavior behavior, object[] constructorArgs, params Type[] interfaces)
        {
            var mocked = (IMocked)MockFactory.Default.CreateMock(typeof(Mock).GetTypeInfo().Assembly, typeof(T), interfaces, constructorArgs);

            mocked.SetBehavior(behavior);

            return (T)mocked;
        }
   }
}