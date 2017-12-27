using System;
using System.Collections.Generic;
using System.Text;

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
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T)mi.Arguments[0]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, TResult>(this TResult target, Func<T1, T2, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, TResult>(this TResult target, Func<T1, T2, T3, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, TResult>(this TResult target, Func<T1, T2, T3, T4, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7], (T9)mi.Arguments[8]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7], (T9)mi.Arguments[8], (T10)mi.Arguments[9]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7], (T9)mi.Arguments[8], (T10)mi.Arguments[9], (T11)mi.Arguments[10]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7], (T9)mi.Arguments[8], (T10)mi.Arguments[9], (T11)mi.Arguments[10], (T12)mi.Arguments[11]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7], (T9)mi.Arguments[8], (T10)mi.Arguments[9], (T11)mi.Arguments[10], (T12)mi.Arguments[11], (T13)mi.Arguments[12]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7], (T9)mi.Arguments[8], (T10)mi.Arguments[9], (T11)mi.Arguments[10], (T12)mi.Arguments[11], (T13)mi.Arguments[12], (T14)mi.Arguments[13]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7], (T9)mi.Arguments[8], (T10)mi.Arguments[9], (T11)mi.Arguments[10], (T12)mi.Arguments[11], (T13)mi.Arguments[12], (T14)mi.Arguments[13], (T15)mi.Arguments[14]), mi.Arguments));

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this TResult target, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> value)
            => Returns<TResult>(value, (mi, next)
                => mi.CreateValueReturn(value((T1)mi.Arguments[0], (T2)mi.Arguments[1], (T3)mi.Arguments[2], (T4)mi.Arguments[3], (T5)mi.Arguments[4], (T6)mi.Arguments[5], (T7)mi.Arguments[6], (T8)mi.Arguments[7], (T9)mi.Arguments[8], (T10)mi.Arguments[9], (T11)mi.Arguments[10], (T12)mi.Arguments[11], (T13)mi.Arguments[12], (T14)mi.Arguments[13], (T15)mi.Arguments[14], (T16)mi.Arguments[15]), mi.Arguments));
    }
}
