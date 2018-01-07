using System.Collections;
using System.Collections.Generic;

namespace Stunts
{
    /// <summary>
    /// Implements <see cref="IEqualityComparer{T}"/> for structural equality comparisons 
    /// using the <see cref="StructuralComparisons.StructuralEqualityComparer"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StructuralComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// Singleton instance of a comparer for <typeparamref name="T"/>.
        /// </summary>
        public static IEqualityComparer<T> Default { get; } = new StructuralComparer<T>();

        /// <inheritdoc />
        public bool Equals(T x, T y) => StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

        /// <inheritdoc />
        public int GetHashCode(T obj) => StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
    }
}
