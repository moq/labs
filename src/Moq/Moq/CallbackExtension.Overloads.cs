#nullable disable
// Disable nullable since this works from the Arguments collection which we know can have nulls
// A NRE here would be a mis-configuration from the user, which the compiler would have caught 
// already in the test setups anyway.
using System;

namespace Moq
{
    public partial class CallbackExtension
    {
        /// <summary>
        /// Specifies a callback to invoke when the method is called.
        /// </summary>
        public static TResult Callback<TResult>(this TResult target, Action callback)
            => Callback(target, args => callback());

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T, TResult>(this TResult target, Action<T> callback)
            => Callback(target, args => callback((T)args[0]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, TResult>(this TResult target, Action<T1, T2> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, TResult>(this TResult target, Action<T1, T2, T3> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, TResult>(this TResult target, Action<T1, T2, T3, T4> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, TResult>(this TResult target, Action<T1, T2, T3, T4, T5> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8], (T10)args[9]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8], (T10)args[9], (T11)args[10]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8], (T10)args[9], (T11)args[10], (T12)args[11]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8], (T10)args[9], (T11)args[10], (T12)args[11], (T13)args[12]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8], (T10)args[9], (T11)args[10], (T12)args[11], (T13)args[12], (T14)args[13]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8], (T10)args[9], (T11)args[10], (T12)args[11], (T13)args[12], (T14)args[13], (T15)args[14]));

        /// <summary>
        /// Specifies a callback to invoke when the method is called that receives the original arguments.
        /// </summary>
        public static TResult Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this TResult target, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> callback)
            => Callback(target, args => callback((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8], (T10)args[9], (T11)args[10], (T12)args[11], (T13)args[12], (T14)args[13], (T15)args[14], (T16)args[15]));
    }
}
