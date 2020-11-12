using System;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class NotMatcherTests
    {
        [Fact]
        public void NotInt()
        {
            var matcher = new NotMatcher<int>(5);

            Assert.False(matcher.Matches(5));
            Assert.True(matcher.Matches(6));
            Assert.True(matcher.Matches(null));
        }

        [Fact]
        public void NotNullInt()
        {
            var matcher = new NotMatcher<int?>(null);

            Assert.True(matcher.Matches(5));
            Assert.False(matcher.Matches(null));
        }

        [Fact]
        public void NotObject()
        {
            var expected = Guid.NewGuid();
            var matcher = new NotMatcher<Value>(new Value(expected));

            Assert.False(matcher.Matches(new Value(expected)));
            Assert.True(matcher.Matches(new Value(Guid.NewGuid())));
        }

        [Fact]
        public void MatcherEquals()
        {
            var id = Guid.NewGuid();
            var expected = new NotMatcher<Value>(new Value(id));
            var actual = new NotMatcher<Value>(new Value(id));

            Assert.Equal(expected, actual);
            Assert.True(expected.Equals(actual));
            Assert.True(((object)expected).Equals(actual));
            Assert.False(expected.Equals(null));
            Assert.Equal(expected.GetHashCode(), actual.GetHashCode());
        }

        class Value
        {
            public Value(Guid id) => Id = id;

            public Guid Id { get; }

            public override bool Equals(object obj) => Id.Equals((obj as Value)?.Id);

            public override int GetHashCode() => Id.GetHashCode();
        }
    }
}
