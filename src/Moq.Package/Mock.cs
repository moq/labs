using System;
using System.CodeDom.Compiler;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Moq.Linq;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Instantiates mocks for the specified types.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [CompilerGenerated]
    internal partial class Mock
    {
        private static readonly Assembly mocksAssembly = typeof(Mock).GetTypeInfo().Assembly;

        /// <summary>
        /// Gets the configuration and introspection for the given mocked instance.
        /// </summary>
        public static IMoq<T> Get<T>(T instance) where T : class => new Moq<T>(instance.AsMock());

        /// <summary>
        /// Creates the mock instance by using the specified types to 
        /// lookup the mock type in the assembly defining this class.
        /// </summary>
        private static T Create<T>(MockBehavior behavior, object[] constructorArgs, params Type[] interfaces) where T : class
        {
            var mocked = (IMocked)MockFactory.Default.CreateMock(mocksAssembly, typeof(T), interfaces, constructorArgs);
            mocked.Initialize(behavior);
            return (T)mocked;
        }

        /// <summary>
        /// Creates the mock instance by using the specified Expression 
        /// to setup the mock.
        /// </summary>
        private static T Create<T>(Expression<Func<T, bool>> setups, MockBehavior behavior)
        {
            var mocked = (IMocked)MockFactory.Default.CreateMock(mocksAssembly, typeof(T), new Type[0], new object[0]);
            mocked.Initialize(behavior);
            using (new SetupScope())
                new MockSetupsBuilder(mocked).Visit(setups);
            return (T)mocked;
        }

        /// <summary>
        /// Creates a mock object of the indicated type.
        /// </summary>
        /// <param name="setups">The predicate with the specification of how the mocked object should behave.</param>
        /// <typeparam name="T">The type of the mocked object.</typeparam>
        /// <returns>The mocked object created.</returns>
        public static T Of<T>(Expression<Func<T, bool>> setups) => Create<T>(setups, MockBehavior.Loose);

        /// <summary>
        /// Creates a mock object of the indicated type.
        /// </summary>
        /// <param name="setups">The predicate with the specification of how the mocked object should behave.</param>
        /// <param name="behavior">Behavior of the mock.</param>
        /// <typeparam name="T">The type of the mocked object.</typeparam>
        /// <returns>The mocked object created.</returns>
        public static T Of<T>(Expression<Func<T, bool>> setups, MockBehavior behavior) => Create<T>(setups, behavior);
    }
}