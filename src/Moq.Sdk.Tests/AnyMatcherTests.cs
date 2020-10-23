﻿using System;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class AnyMatcherTests
    {
        [Fact]
        public void ThrowsIfNullType()
            => Assert.Throws<ArgumentNullException>(() => new AnyMatcher(null));

        [Fact]
        public void EqualForSameType()
        {
            var x = new AnyMatcher(typeof(int));
            var y = new AnyMatcher(typeof(int));
            var z = new AnyMatcher(typeof(string));

            Assert.Same(x.ArgumentType, y.ArgumentType);
            Assert.Equal(x, y);
            Assert.True(((object)x).Equals(y));
            Assert.False(x.Equals(new object()));
            Assert.False(x.Equals((object)null));
            Assert.Equal(x.GetHashCode(), y.GetHashCode());
            Assert.NotEqual(x, z);
            Assert.False(((object)x).Equals(z));
        }

        [Fact]
        public void NotEqualForDifferentGenericType()
        {
            var x = AnyMatcher<int>.Default;
            var y = AnyMatcher<string>.Default;

            Assert.NotEqual(x.ArgumentType, y.ArgumentType);
            Assert.NotEqual(x, y);
            Assert.NotEqual((object)x, y);
            Assert.NotEqual(x.GetHashCode(), y.GetHashCode());
        }

        [Fact]
        public void MatchesValuesWithType()
        {
            var x = new AnyMatcher(typeof(int));

            Assert.True(x.Matches(5));
            Assert.False(x.Matches("foo"));
        }

        [Fact]
        public void MatchesValuesWithGenericType()
        {
            var x = AnyMatcher<int>.Default;

            Assert.True(x.Matches(5));
            Assert.False(x.Matches("foo"));
        }

        [Fact]
        public void GenericNullableIntMatchesNull()
        {
            var x = AnyMatcher<int?>.Default;

            Assert.True(x.Matches(null));
        }

        [Fact]
        public void NonNullableNeverMatchesNull()
        {
            var x = new AnyMatcher(typeof(int));
            var y = AnyMatcher<int>.Default;

            Assert.False(x.Matches(null));
            Assert.False(y.Matches(null));
        }

        [Fact]
        public void NullableIntMatchesNull()
        {
            var x = new AnyMatcher(typeof(int?));

            Assert.True(x.Matches(null));
        }

        [Fact]
        public void ReferenceTypeMatchesNull()
        {
            var x = AnyMatcher<string>.Default;

            Assert.True(x.Matches(null));
        }

        [Fact]
        public void MatchesDerivedGenericType()
        {
            var x = AnyMatcher<Base>.Default;

            Assert.True(x.Matches(new Derived()));
        }

        [Fact]
        public void MatchesDerivedType()
        {
            var x = new AnyMatcher(typeof(Base));

            Assert.True(x.Matches(new Derived()));
        }

        private class Base { }

        private class Derived : Base { }
    }
}
