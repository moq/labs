using System;

namespace Moq
{
    partial class CallbackExtensions
    {
        public static TResult Callback<TResult>(this TResult target, Action callback)
            => Callback(target, args => callback());

        public static TResult Callback<T, TResult>(this TResult target, Action<T> callback)
            => Callback(target, args => callback((T)args[0]));

        public static TResult Callback<T1, T2, TResult>(this TResult target, Action<T1, T2> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1]));
    }
}
