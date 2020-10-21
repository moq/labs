using System;
using System.ComponentModel;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Defines the number of expected invocations.
    /// </summary>
    public readonly struct Times : IEquatable<Times>
    {
        readonly Lazy<int> hashCode;

        /// <summary>
        /// Initializes the constraint with the given <paramref name="from"/> and 
        /// <paramref name="to"/> values.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Times(int from, int to)
        {
            From = from;
            To = to;
            hashCode = new Lazy<int>(() => HashCode.Combine(from, to));
        }

        /// <summary>
        /// At least one call is expected.
        /// </summary>
        public static Times AtLeastOnce { get; } = new Times(1, int.MaxValue);

        /// <summary>
        /// At most one call is expected.
        /// </summary>
        public static Times AtMostOnce { get; } = new Times(0, 1);

        /// <summary>
        /// No calls are expected.
        /// </summary>
        public static Times Never { get; } = new Times(0, 0);

        /// <summary>
        /// Exactly one call is expected.
        /// </summary>
        public static Times Once { get; } = new Times(1, 1);

        /// <summary>
        /// The minimum calls expected.
        /// </summary>
        public int From { get; }

        /// <summary>
        /// The maximum calls expected.
        /// </summary>
        public int To { get; }

        /// <summary>Deconstructs this instance.</summary>
        /// <param name="from">This output parameter will receive the minimum required number of calls satisfying this instance (i.e. the lower inclusive bound).</param>
        /// <param name="to">This output parameter will receive the maximum allowed number of calls satisfying this instance (i.e. the upper inclusive bound).</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out int from, out int to)
        {
            if (hashCode == null)
            {
                // default(Times) == AtLeastOnce
                AtLeastOnce.Deconstruct(out from, out to);
            }
            else
            {
                from = From;
                to = To;
            }
        }

        /// <summary>
        /// At least <paramref name="count"/> calls are expected.
        /// </summary>
        /// <param name="count">The minimum number of expected calls.</param>
        public static Times AtLeast(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            return new Times(count, int.MaxValue);
        }

        /// <summary>
        /// At most <paramref name="count"/> calls are expected.
        /// </summary>
        /// <param name="count">The maximum number of expected calls.</param>
        public static Times AtMost(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return new Times(0, count);
        }

        /// <summary>
        /// Exactly <paramref name="count"/> call is expected.
        /// </summary>
        /// <param name="count">The times that a method or property can be called.</param>
        public static Times Exactly(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return new Times(count, count);
        }

        /// <summary>
        /// Validates whether the given <paramref name="count"/> value satisfies this 
        /// occurrence constraint.
        /// </summary>
        /// <param name="count">The number of calls to validate against this occurrence constraint.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="count"/> is included in the range <see cref="From"/>-<see cref="To"/>
        /// (inclusive).
        /// </returns>
        public bool Validate(int count)
        {
            // By deconstructing, we apply our default behavior of considering default(Times) == AtLeastOnce.
            var (from, to) = this;
            return count >= from && count <= to;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Times"/> value.
        /// </summary>
        /// <param name="other">A <see cref="Times"/> value to compare to this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> has the same value as this instance;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Times other)
        {
            // By deconstructing first, we apply the behavior for default(Times) == AtLeastOnce
            var (from, to) = this;
            var (otherFrom, otherTo) = other;

            return from == otherFrom && to == otherTo;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified <see cref="Times"/> value.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> has the same value as this instance;
        /// otherwise, <see langword="false"/>.
        /// </returns>s
        public override bool Equals(object obj) => obj is Times other && Equals(other);

        /// <summary>
        ///   Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///  A hash code for this instance, suitable for use in hashing algorithms
        ///  and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => hashCode == null ? AtLeastOnce.GetHashCode() : hashCode.Value;

        /// <summary>
        ///   Determines whether two specified <see cref="Times"/> objects have the same value.
        /// </summary>
        /// <param name="left">The first <see cref="Times"/>.</param>
        /// <param name="right">The second <see cref="Times"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if <paramref name="left"/> has the same value as <paramref name="right"/>;
        ///   otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Times left, Times right) => left.Equals(right);

        /// <summary>
        ///   Determines whether two specified <see cref="Times"/> objects have different values.
        /// </summary>
        /// <param name="left">The first <see cref="Times"/>.</param>
        /// <param name="right">The second <see cref="Times"/>.</param>
        /// <returns>
        ///   <see langword="true"/> if the value of <paramref name="left"/> is different from
        ///   <paramref name="right"/>'s; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Times left, Times right) => !left.Equals(right);

        /// <summary>
        /// Converts an integer to <see cref="AtLeastOnce"/> if <paramref name="count"/> is <c>-1</c>, 
        /// <see cref="Never"/> if <paramref name="count"/> is <c>0</c>, 
        /// <see cref="Once"/> if <paramref name="count"/> is <c>1</c> and 
        /// <see cref="Exactly(int)"/> with <paramref name="count"/> otherwise.
        /// </summary>
        public static implicit operator Times(int count) => count switch
        {
            -1 => AtLeastOnce,
            0 => Never,
            1 => Once, 
            _ => Exactly(count)
        };

        /// <summary>
        /// Provide a friendly representation of the expected range.
        /// </summary>
        public override string ToString()
        {
            if (this == Once)
                return nameof(Once);
            if (this == Never)
                return nameof(Never);
            if (this == AtLeastOnce)
                return nameof(AtLeastOnce);
            if (this == AtMostOnce)
                return nameof(AtMostOnce);
            if (From == 0 && To != 0)
                return $"{nameof(AtMost)}({To})";
            if (From != 0 && To == int.MaxValue)
                return $"{nameof(AtLeast)}({From})";

            return $"{From}..{To}";
        }
    }
}
