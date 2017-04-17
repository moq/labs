using System;
using Moq.Proxy;
using Moq.Sdk;

namespace Moq
{
    public static class Mock
    {
        [ProxyGenerator]
        public static T Of<T>() => Of<T>(typeof(IMocked));

        [ProxyGenerator]
        public static T Of<T, T1>() => Of<T>(typeof(T1), typeof(IMocked));

        [ProxyGenerator]
        public static T Of<T, T1, T2>() => Of<T>(typeof(T1), typeof(T2), typeof(IMocked));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3>() => Of<T>(typeof(T1), typeof(T2), typeof(T3), typeof(IMocked));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4>() => Of<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(IMocked));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4, T5>() => Of<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(IMocked));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6>() => Of<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(IMocked));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7>() => Of<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(IMocked));

        [ProxyGenerator]
        public static T Of<T, T1, T2, T3, T4, T5, T6, T7, T8>() => Of<T>(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(IMocked));

        static T Of<T>(params Type[] types)
        {
            var proxy = (IProxy)ProxyFactory.Default.CreateProxy(typeof(T), types, new object[0]);

            proxy.Behaviors.Add(new MockProxyBehavior());
            proxy.Behaviors.Add(new DefaultValueProxyBehavior());

            return (T)proxy;
        }
    }
}
