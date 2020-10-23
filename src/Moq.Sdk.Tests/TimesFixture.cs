using System;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class TimesFixture
    {
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(int.MaxValue, true)]
        public void DefaultTimesRangesBetweenOneAndMaxValue(int count, bool verifies)
        {
            Assert.Equal(verifies, default(Times).Validate(count));
        }

        [Fact]
        public void AtLeastOnceRangesBetweenOneAndMaxValue()
        {
            var target = Times.AtLeastOnce;

            Assert.False(target.Validate(-1));
            Assert.False(target.Validate(0));
            Assert.True(target.Validate(1));
            Assert.True(target.Validate(5));
            Assert.True(target.Validate(int.MaxValue));
        }

        [Fact]
        public void AtLeastThrowsIfTimesLessThanOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Times.AtLeast(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Times.AtLeast(-1));
        }

        [Fact]
        public void AtLeastRangesBetweenTimesAndMaxValue()
        {
            var target = Times.AtLeast(10);

            Assert.False(target.Validate(-1));
            Assert.False(target.Validate(0));
            Assert.False(target.Validate(9));
            Assert.True(target.Validate(10));
            Assert.True(target.Validate(int.MaxValue));
        }

        [Fact]
        public void AtMostOnceRangesBetweenZeroAndOne()
        {
            var target = Times.AtMostOnce;

            Assert.False(target.Validate(-1));
            Assert.True(target.Validate(0));
            Assert.True(target.Validate(1));
            Assert.False(target.Validate(5));
            Assert.False(target.Validate(int.MaxValue));
        }

        [Fact]
        public void AtMostThrowsIfTimesLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Times.AtMost(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Times.AtMost(-2));
        }

        [Fact]
        public void AtMostRangesBetweenZeroAndTimes()
        {
            var target = Times.AtMost(10);

            Assert.False(target.Validate(-1));
            Assert.True(target.Validate(0));
            Assert.True(target.Validate(6));
            Assert.True(target.Validate(10));
            Assert.False(target.Validate(11));
            Assert.False(target.Validate(int.MaxValue));
        }

        [Fact]
        public void ExactlyThrowsIfTimesLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Times.Exactly(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Times.Exactly(-2));
        }

        [Fact]
        public void ExactlyCheckExactTimes()
        {
            var target = Times.Exactly(10);

            Assert.False(target.Validate(-1));
            Assert.False(target.Validate(0));
            Assert.False(target.Validate(9));
            Assert.True(target.Validate(10));
            Assert.False(target.Validate(11));
            Assert.False(target.Validate(int.MaxValue));
        }

        [Fact]
        public void NeverChecksZeroTimes()
        {
            var target = Times.Never;

            Assert.False(target.Validate(-1));
            Assert.True(target.Validate(0));
            Assert.False(target.Validate(1));
            Assert.False(target.Validate(int.MaxValue));
        }

        [Fact]
        public void OnceChecksOneTime()
        {
            var target = Times.Once;

            Assert.False(target.Validate(-1));
            Assert.False(target.Validate(0));
            Assert.True(target.Validate(1));
            Assert.False(target.Validate(int.MaxValue));
        }

        public class Deconstruction
        {
            [Fact]
            public void AtLeast_n_deconstructs_to_n_MaxValue()
            {
                const int n = 42;

                var (from, to) = Times.AtLeast(n);
                Assert.Equal(n, from);
                Assert.Equal(int.MaxValue, to);
            }

            [Fact]
            public void AtLeastOnce_deconstructs_to_1_MaxValue()
            {
                var (from, to) = Times.AtLeastOnce;
                Assert.Equal(1, from);
                Assert.Equal(int.MaxValue, to);
            }

            [Fact]
            public void AtMost_n_deconstructs_to_0_n()
            {
                const int n = 42;

                var (from, to) = Times.AtMost(n);
                Assert.Equal(0, from);
                Assert.Equal(n, to);
            }

            [Fact]
            public void AtMostOnce_deconstructs_to_0_1()
            {
                var (from, to) = Times.AtMostOnce;
                Assert.Equal(0, from);
                Assert.Equal(1, to);
            }

            [Fact]
            public void Exactly_n_deconstructs_to_n_n()
            {
                const int n = 42;
                var (from, to) = Times.Exactly(n);
                Assert.Equal(n, from);
                Assert.Equal(n, to);
            }

            [Fact]
            public void Once_deconstructs_to_1_1()
            {
                var (from, to) = Times.Once;
                Assert.Equal(1, from);
                Assert.Equal(1, to);
            }

            [Fact]
            public void Never_deconstructs_to_0_0()
            {
                var (from, to) = Times.Never;
                Assert.Equal(0, from);
                Assert.Equal(0, to);
            }
        }

        public class Equality
        {
#pragma warning disable xUnit2000 // Constants and literals should be the expected argument
            [Fact]
            public void default_Equals_AtLeastOnce()
            {
                Assert.Equal(Times.AtLeastOnce, default(Times));
            }
#pragma warning restore xUnit2000

            [Fact]
            public void default_GetHashCode_equals_AtLeastOnce_GetHashCode()
            {
                Assert.Equal(Times.AtLeastOnce.GetHashCode(), default(Times).GetHashCode());
            }

            [Fact]
            public void Once_equals_Once()
            {
                Assert.Equal(Times.Once, Times.Once);
            }

            [Fact]
            public void Once_equals_Exactly_1()
            {
                Assert.Equal(Times.Once, Times.Exactly(1));
            }
        }
    }
}
