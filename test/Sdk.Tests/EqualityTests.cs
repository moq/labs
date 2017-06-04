using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Moq.Proxy;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class EqualityTests
    {
        [Fact]
        public void MethodInvocationEqualsByTargetMethodArgs()
        {
            var method = new Action<bool, string, PlatformID>(AMethod).Method;
            var args = new object[] { true, "foo", PlatformID.Win32NT };
            var target = this;

            IStructuralEquatable equatable = new MethodInvocation(target, method, args);
            IStructuralEquatable other = new MethodInvocation(target, method, args);

            Assert.True(equatable.Equals(other));
            Assert.True(equatable.Equals(other, EqualityComparer<object>.Default));
            Assert.Equal(equatable.GetHashCode(), other.GetHashCode());
            Assert.Equal(equatable.GetHashCode(EqualityComparer<object>.Default), other.GetHashCode(EqualityComparer<object>.Default));
        }

        [Fact]
        public void MethodInvocationEqualsEvenIfContextDiffers()
        {
            var method = new Action<bool, string, PlatformID>(AMethod).Method;
            var args = new object[] { true, "foo", PlatformID.Win32NT };
            var target = this;

            var equatable = new MethodInvocation(target, method, args);
            equatable.Context["foo"] = "bar";
            var other = new MethodInvocation(target, method, args);

            Assert.True(equatable.Equals(other));
        }

        [Fact]
        public void MethodInvocationDoesNotEqualNull()
        {
            var method = new Action<bool, string, PlatformID>(AMethod).Method;
            var args = new object[] { true, "foo", PlatformID.Win32NT };
            var target = this;

            var equatable = new MethodInvocation(target, method, args);

            Assert.False(equatable.Equals(null));
        }

        [Fact]
        public void MethodInvocationDoesNotEqualOtherObject()
        {
            var method = new Action<bool, string, PlatformID>(AMethod).Method;
            var args = new object[] { true, "foo", PlatformID.Win32NT };
            var target = this;

            var equatable = new MethodInvocation(target, method, args);

            Assert.False(equatable.Equals(new object()));
        }

        [Fact]
        public void AnyMatcherEquality()
        {
            var matcher = AnyMatcher<bool>.Default;

            Assert.True(matcher.Equals(AnyMatcher<bool>.Default));
            Assert.False(matcher.Equals(null));
            Assert.False(matcher.Equals(AnyMatcher<string>.Default));
        }

        [Fact]
        public void ConditionalMatcherEqualsByConditionFunctionAndName()
        {
            Func<string, bool> condition = s => s.Length > 0;
            var matcher = new ConditionalMatcher<string>(condition, "foo");

            Assert.True(matcher.Equals(new ConditionalMatcher<string>(condition, "foo")));
            Assert.True(matcher.Equals(new ConditionalMatcher<string>(condition, "foo"), EqualityComparer<object>.Default));
            Assert.Equal(matcher.GetHashCode(), new ConditionalMatcher<string>(condition, "foo").GetHashCode());
            Assert.Equal(matcher.GetHashCode(EqualityComparer<object>.Default), new ConditionalMatcher<string>(condition, "foo").GetHashCode(EqualityComparer<object>.Default));

            Assert.False(matcher.Equals(new ConditionalMatcher<string>(condition, "bar")));
            Assert.False(matcher.Equals(new ConditionalMatcher<string>(s => s.Length > 0, "foo")));
        }

        [Fact]
        public void ValueMatcherEqualsByTypeAndValue()
        {
            var matcher = new ValueMatcher(typeof(string), "foo");
            var other = new ValueMatcher(typeof(string), "foo");

            Assert.True(matcher.Equals(other));
            Assert.True(matcher.Equals(other, EqualityComparer<object>.Default));
            Assert.Equal(matcher.GetHashCode(), other.GetHashCode());
            Assert.Equal(matcher.GetHashCode(EqualityComparer<object>.Default), other.GetHashCode(EqualityComparer<object>.Default));

            Assert.False(matcher.Equals(new ValueMatcher(typeof(string), "bar")));
            Assert.False(matcher.Equals(new ValueMatcher(typeof(object), "foo")));
        }

        [Fact]
        public void TupleOfEqualMatchersAreEqual()
        {
            var any = AnyMatcher<string>.Default;
            Func<string, bool> condition = s => s.Length > 0;
            var conditional = new ConditionalMatcher<string>(condition, "foo");
            var value = new ValueMatcher(typeof(string), "foo");

            var t1 = Tuple.Create(any, conditional, value);
            var t2 = Tuple.Create(any, conditional, value);

            Assert.True(t1.Equals(t2));
            Assert.Equal(t1.GetHashCode(), t2.GetHashCode());
        }

        [Fact]
        public void MockSetupEqualsByInvocationAndMatchers()
        {
            var method = new Action<bool, string, PlatformID>(AMethod).Method;
            var args = new object[] { true, "foo", PlatformID.Win32NT };
            var target = this;

            var any = AnyMatcher<string>.Default;
            Func<string, bool> condition = s => s.Length > 0;
            var conditional = new ConditionalMatcher<string>(condition, "foo");
            var value = new ValueMatcher(typeof(string), "foo");

            var setup = new MockSetup(new MethodInvocation(target, method, args), new[] { any, conditional, value });
            var other = new MockSetup(new MethodInvocation(target, method, args), new[] { any, conditional, value });

            Assert.True(setup.Equals(other));
            Assert.Equal(setup.GetHashCode(), other.GetHashCode());

            var hash = new HashSet<IMockSetup>();
            Assert.True(hash.Add(setup));
            Assert.False(hash.Add(other));
            Assert.True(hash.Contains(setup));
            Assert.True(hash.Contains(other));

            Assert.False(setup.Equals(
                new MockSetup(new MethodInvocation(new object(), method, args), new[] { any, conditional, value })));
        }

        [Fact]
        public void Matchers()
        {
            var hash = new HashSet<IArgumentMatcher>();
            var value1 = new ValueMatcher(typeof(string), "foo");
            var value2 = new ValueMatcher(typeof(string), "foo");

            Assert.True(hash.Add(value1));
            Assert.False(hash.Add(value2));
        }

        void AMethod(bool b, string s, PlatformID p) { }
    }
}
