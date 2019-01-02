using System;
using System.Linq;

namespace Moq
{
    partial class ReturnsExtension
    {
        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T, TResult>(this TResult target, Func<T, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T)i.Arguments[0]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, TResult>(this TResult target, Func<T1, T2, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, TResult>(this TResult target, Func<T1, T2, T3, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, TResult>(this TResult target, Func<T1, T2, T3, T4, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7], (T9)i.Arguments[8]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7], (T9)i.Arguments[8], (T10)i.Arguments[9]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7], (T9)i.Arguments[8], (T10)i.Arguments[9], (T11)i.Arguments[10]), i.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7], (T9)i.Arguments[8], (T10)i.Arguments[9], (T11)i.Arguments[10], (T12)i.Arguments[11]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7], (T9)i.Arguments[8], (T10)i.Arguments[9], (T11)i.Arguments[10], (T12)i.Arguments[11], (T13)i.Arguments[12]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7], (T9)i.Arguments[8], (T10)i.Arguments[9], (T11)i.Arguments[10], (T12)i.Arguments[11], (T13)i.Arguments[12], (T14)i.Arguments[13]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7], (T9)i.Arguments[8], (T10)i.Arguments[9], (T11)i.Arguments[10], (T12)i.Arguments[11], (T13)i.Arguments[12], (T14)i.Arguments[13], (T15)i.Arguments[14]), i.Arguments.ToArray()));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> value)
            => Returns<TResult>(value, (m, i, next)
                => i.CreateValueReturn(value((T1)i.Arguments[0], (T2)i.Arguments[1], (T3)i.Arguments[2], (T4)i.Arguments[3], (T5)i.Arguments[4], (T6)i.Arguments[5], (T7)i.Arguments[6], (T8)i.Arguments[7], (T9)i.Arguments[8], (T10)i.Arguments[9], (T11)i.Arguments[10], (T12)i.Arguments[11], (T13)i.Arguments[12], (T14)i.Arguments[13], (T15)i.Arguments[14], (T16)i.Arguments[15]), i.Arguments.ToArray()));
    }
}
