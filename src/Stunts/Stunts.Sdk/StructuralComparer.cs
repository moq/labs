using System.Collections;
using System.Collections.Generic;

namespace Stunts
{
    public class StructuralComparer<T> : IEqualityComparer<T>
    {
        public static IEqualityComparer<T> Default { get; } = new StructuralComparer<T>();

        public bool Equals(T x, T y) => StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

        public int GetHashCode(T obj) => StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
    }
}
