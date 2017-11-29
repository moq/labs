using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Stunts.Tests
{
    public class DefaultEqualityBehaviorTests
    {
        [Fact]
        public void AppliesToGetHashCode()
        {
            var method = typeof(Foo).GetMethod(nameof(object.GetHashCode));
            var behavior = new DefaultEqualityBehavior();

            Assert.True(behavior.AppliesTo(new MethodInvocation(new Foo(), method, new object[0])));
        }

        [Fact]
        public void AppliesToEquals()
        {
            var method = typeof(Foo).GetMethod(nameof(object.Equals));
            var behavior = new DefaultEqualityBehavior();

            Assert.True(behavior.AppliesTo(new MethodInvocation(new Foo(), method, new object[] { new Foo() })));
        }

        [Fact]
        public void GetsHashCode()
        {
            var method = typeof(Foo).GetMethod(nameof(object.GetHashCode));
            var behavior = new DefaultEqualityBehavior();
            var target = new Foo();

            var expected = target.GetHashCode();
            var actual = (int)behavior.Invoke(new MethodInvocation(target, method, new object[0]), () => null).ReturnValue;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EqualsSameInstance()
        {
            var method = typeof(Foo).GetMethod(nameof(object.Equals));
            var behavior = new DefaultEqualityBehavior();
            var target = new Foo();

            var actual = (bool)behavior.Invoke(new MethodInvocation(target, method, new object[] { target }), () => null).ReturnValue;

            Assert.True(actual);
        }

        [Fact]
        public void NotEqualsDifferentInstance()
        {
            var method = typeof(Foo).GetMethod(nameof(object.Equals));
            var behavior = new DefaultEqualityBehavior();
            var target = new Foo();

            var actual = (bool)behavior.Invoke(new MethodInvocation(target, method, new object[] { new Foo() }), () => null).ReturnValue;

            Assert.False(actual);
        }

        public class Foo
        {
            public override string ToString()
            {
                return base.ToString();
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
        }
    }
}
