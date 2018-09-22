namespace Stunts
{
    using System;
    using System.CodeDom.Compiler;
    using System.Reflection;

    /// <summary>
    /// Instantiates stunts for the specified types.
    /// </summary>
    [GeneratedCode("Stunts", "5.0")]
    internal partial class Stunt
    {
        private static T Create<T>(object[] constructorArgs, params Type[] interfaces) => 
            (T)StuntFactory.Default.CreateStunt(typeof(Stunt).GetTypeInfo().Assembly, typeof(T), interfaces, constructorArgs);

        /// <summary>
        /// Creates a stunt that inherits or implements the type <typeparamref name="T"/>.
        /// </summary>
        [StuntGenerator]
        public static T Of<T>(params object[] constructorArgs) => Create<T>(constructorArgs);

        [StuntGenerator]
        public static T Of<T, T1>(params object[] constructorArgs) => Create<T>(constructorArgs, typeof(T1));

        [StuntGenerator]
        public static T Of<T, T1, T2>(params object[] constructorArgs) => Create<T>(constructorArgs, typeof(T1), typeof(T2));

        [StuntGenerator]
        public static T Of<T, T1, T2, T3>(params object[] constructorArgs) => Create<T>(constructorArgs, typeof(T1), typeof(T2), typeof(T3));

        [StuntGenerator]
        public static T Of<T, T1, T2, T3, T4>(params object[] constructorArgs) => Create<T>(constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        [StuntGenerator]
        public static T Of<T, T1, T2, T3, T4, T5>(params object[] constructorArgs) => Create<T>(constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        [StuntGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6>(params object[] constructorArgs) => Create<T>(constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));

        [StuntGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7>(params object[] constructorArgs) => Create<T>(constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));

        [StuntGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7, T8>(params object[] constructorArgs) => Create<T>(constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
    }
}