using System;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class ConditionalMatcherTests
    {
        [Fact]
        public void DoesNotMatchNullForNonNullableValueType()
        {
            var matcher = new ConditionalMatcher<PlatformID>(_ => true);

            Assert.False(matcher.Matches(null));
        }

        [Fact]
        public void MatchesNullForNullableValueType()
        {
            var matcher = new ConditionalMatcher<PlatformID?>(_ => true);

            Assert.True(matcher.Matches(null));
        }

        [Fact]
        public void MatchesNullForReferenceType()
        {
            var matcher = new ConditionalMatcher<string>(x => x == null);

            Assert.True(matcher.Matches(default(string)));
        }
    }
}
