using System;
using System.Reflection;
using Moq.Proxy;
using Moq.Sdk;

namespace Moq.Tests
{
    public static class Mock
    {
        [ProxyGenerator]
        public static T Of<T>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly());

        [ProxyGenerator]
        public static T Of<T, T1>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly(), typeof(T1));

        [ProxyGenerator]
        public static T Of<T, T1, T2>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly(), typeof(T1), typeof(T2));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly(), typeof(T1), typeof(T2), typeof(T3));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly(), typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4, T5>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly(), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly(), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly(), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7, T8>() => Moq.Mock.Of<T>(Assembly.GetExecutingAssembly(), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
    }
}