using System;
using System.Collections.Generic;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class ConditionalMatcherTests
    {
        [Fact]
        public void ThrowsIfNullCondition()
            => Assert.Throws<ArgumentNullException>(() => new ConditionalMatcher<PlatformID>(null));

        [Fact]
        public void ThrowsIfNullName()
            => Assert.Throws<ArgumentNullException>(() => new ConditionalMatcher<PlatformID>(_ => true, null));

        [Fact]
        public void ArgumentTypeIsGenericType()
            => Assert.Equal(typeof(PlatformID), new ConditionalMatcher<PlatformID>(_ => true).ArgumentType);

        [Fact]
        public void MatchesConditionally()
            => Assert.True(new ConditionalMatcher<PlatformID>(_ => true).Matches(PlatformID.Win32NT));

        [Fact]
        public void MatchesDerivedType()
            => Assert.True(new ConditionalMatcher<Base>(_ => true).Matches(new Derived()));

        [Fact]
        public void DoesNotMatchOtherTypes()
            => Assert.False(new ConditionalMatcher<Base>(_ => true).Matches(new object()));

        [Fact]
        public void DoesNotMatchNullForNonNullableValueType()
            => Assert.False(new ConditionalMatcher<PlatformID>(_ => true).Matches(null));

        [Fact]
        public void MatchesNullForNullableValueType()
            => Assert.True(new ConditionalMatcher<PlatformID?>(_ => true).Matches(null));

        [Fact]
        public void MatchesNullForReferenceType()
            => Assert.True(new ConditionalMatcher<string>(x => x == null).Matches(default(string)));

        [Fact]
        public void EqualsByConditionFunctionAndName()
        {
            Func<string, bool> condition = s => s.Length > 0;
            var matcher = new ConditionalMatcher<string>(condition, "foo");

            Assert.True(matcher.Equals(new ConditionalMatcher<string>(condition, "foo")));
            Assert.Equal(matcher.GetHashCode(), new ConditionalMatcher<string>(condition, "foo").GetHashCode());

            Assert.False(matcher.Equals(new ConditionalMatcher<string>(condition, "bar")));
            Assert.False(matcher.Equals(new ConditionalMatcher<string>(s => s.Length > 0, "foo")));

            Assert.False(matcher.Equals(new ConditionalMatcher<int>(_ => true, "foo")));
        }

        class Base { }
        class Derived : Base { }
    }
}
