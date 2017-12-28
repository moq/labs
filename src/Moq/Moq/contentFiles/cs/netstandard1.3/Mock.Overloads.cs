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
        /// <summary>
        /// Creates a stunt that inherits or implements the type <typeparamref name="T"/>.
        /// </summary>
        [MockGenerator]
        public static T Of<T>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs);

        [MockGenerator]
        public static T Of<T, T1>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs, typeof(T1));

        [MockGenerator]
        public static T Of<T, T1, T2>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs, typeof(T1), typeof(T2));

        [MockGenerator]
        public static T Of<T, T1, T2, T3>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs, typeof(T1), typeof(T2), typeof(T3));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4, T5>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7, T8>(params object[] constructorArgs) => Create<T>(MockBehavior.Loose, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));

        /// <summary>
        /// Creates a stunt that inherits or implements the type <typeparamref name="T"/>.
        /// </summary>
        [MockGenerator]
        public static T Of<T>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs);

        [MockGenerator]
        public static T Of<T, T1>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs, typeof(T1));

        [MockGenerator]
        public static T Of<T, T1, T2>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs, typeof(T1), typeof(T2));

        [MockGenerator]
        public static T Of<T, T1, T2, T3>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs, typeof(T1), typeof(T2), typeof(T3));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4, T5>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));

        [MockGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7, T8>(MockBehavior behavior, params object[] constructorArgs) => Create<T>(behavior, constructorArgs, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
    }
}