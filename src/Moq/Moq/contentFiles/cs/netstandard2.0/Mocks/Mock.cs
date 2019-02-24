namespace Moq
{
    using System;
    using System.CodeDom.Compiler;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Moq.Sdk;

    /// <summary>
    /// Instantiates mocks for the specified types.
    /// </summary>
    [GeneratedCode("Moq", "5.0")]
    [CompilerGenerated]
    partial class Mock
    {
        /// <summary>
        /// Creates the mock instance by using the specified types to 
        /// lookup the mock type in the assembly defining this class.
        /// </summary>
        private static T Create<T>(MockBehavior behavior, object[] constructorArgs, params Type[] interfaces)
        {
            var mocked = (IMocked)MockFactory.Default.CreateMock(typeof(Mock).GetTypeInfo().Assembly, typeof(T), interfaces, constructorArgs);

            mocked.Initialize(behavior);

            return (T)mocked;
        }
   }
}