using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Moq
{
    /// <summary>Supports the legacy API.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct Times : IEquatable<Times>
    {
        readonly int from;
        readonly int to;
        readonly Kind kind;

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Sdk.Times ToSdk()
        {
            switch (kind)
            {
                case Kind.AtLeastOnce:
                    return Sdk.Times.AtLeastOnce;
                case Kind.AtMostOnce:
                    return Sdk.Times.AtMostOnce;
                case Kind.Once:
                    return Sdk.Times.Once;
                case Kind.Never:
                    return Sdk.Times.Once;
                case Kind.AtLeast:
                case Kind.AtMost:
                case Kind.BetweenExclusive:
                case Kind.BetweenInclusive:
                case Kind.Exactly:
                    return new Sdk.Times(from, to);
                default:
                    return default;
            }
        }

        Times(Kind kind, int from, int to)
        {
            this.from = from;
            this.to = to;
            this.kind = kind;
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out int from, out int to)
        {
            if (this.kind == default)
            {
                // This branch makes `default(Times)` equivalent to `Times.AtLeastOnce()`,
                // which is the implicit default across Moq's API for overloads that don't
                // accept a `Times` instance. While user code shouldn't use `default(Times)`
                // (but instead either specify `Times` explicitly or not at all), it is
                // easy enough to correct:

                Debug.Assert(this.kind == Kind.AtLeastOnce);

                from = 1;
                to = int.MaxValue;
            }
            else
            {
                from = this.from;
                to = this.to;
            }
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Times AtLeast(int callCount)
        {
            if (callCount < 1)
                throw new ArgumentOutOfRangeException(nameof(callCount));

            return new Times(Kind.AtLeast, callCount, int.MaxValue);
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Times AtLeastOnce() => new Times(Kind.AtLeastOnce, 1, int.MaxValue);

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Times AtMost(int callCount)
        {
            if (callCount < 0)
                throw new ArgumentOutOfRangeException(nameof(callCount));

            return new Times(Kind.AtMost, 0, callCount);
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Times AtMostOnce() => new Times(Kind.AtMostOnce, 0, 1);

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Times Between(int callCountFrom, int callCountTo, Range rangeKind)
        {
            if (rangeKind == Range.Exclusive)
            {
                if (callCountFrom <= 0 || callCountTo <= callCountFrom)
                    throw new ArgumentOutOfRangeException(nameof(callCountFrom));

                if (callCountTo - callCountFrom == 1)
                    throw new ArgumentOutOfRangeException(nameof(callCountTo));

                return new Times(Kind.BetweenExclusive, callCountFrom + 1, callCountTo - 1);
            }

            if (callCountFrom < 0 || callCountTo < callCountFrom)
                throw new ArgumentOutOfRangeException(nameof(callCountFrom));

            return new Times(Kind.BetweenInclusive, callCountFrom, callCountTo);
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Times Exactly(int callCount)
        {
            if (callCount < 0)
                throw new ArgumentOutOfRangeException(nameof(callCount));

            return new Times(Kind.Exactly, callCount, callCount);
        }

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Times Never() => new Times(Kind.Never, 0, 0);

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Times Once() => new Times(Kind.Once, 1, 1);

        /// <summary>Supports the legacy API.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(Times other)
        {
            var (from, to) = this;
            var (otherFrom, otherTo) = other;
            return from == otherFrom && to == otherTo;
        }

        enum Kind
        {
            AtLeastOnce,
            AtLeast,
            AtMost,
            AtMostOnce,
            BetweenExclusive,
            BetweenInclusive,
            Exactly,
            Once,
            Never,
        }
    }
}