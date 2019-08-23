using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Stunts.Tests
{
    public class ArgumentCollectionTests
    {
        static readonly MethodInfo testMethod = typeof(ArgumentCollectionTests).GetMethod(nameof(Do), BindingFlags.Static | BindingFlags.Public);

        [Fact]
        public void ThrowsIfMismatchValuesLength()
            => Assert.Throws<ArgumentException>(() => new ArgumentCollection(new object[] { 5, "Foo" }, testMethod.GetParameters()));

        [Fact]
        public void ThrowsIfGetNameNotFound()
            => Assert.Throws<KeyNotFoundException>(() => new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters())["foo"]);

        [Fact]
        public void ThrowsIfSetNameNotFound()
            => Assert.Throws<KeyNotFoundException>(() => new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters())["foo"] = 1);

        [Fact]
        public void ThrowsIfGetInfoByNameNotFound()
            => Assert.Throws<KeyNotFoundException>(() => new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters()).GetInfo("foo"));

        [Fact]
        public void AccessValueByIndex()
        {
            var arguments = new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters());

            Assert.Equal(5, arguments[0]);

            arguments[0] = 10;

            Assert.Equal(10, arguments[0]);
        }

        [Fact]
        public void AccessValueByName()
        {
            var arguments = new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters());

            Assert.Equal(5, arguments["value"]);

            arguments["value"] = 10;

            Assert.Equal(10, arguments["value"]);
        }

        [Fact]
        public void ContainsByName() 
            => Assert.True(new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters()).Contains("value"));

        [Fact]
        public void GetNameFromIndex()
            => Assert.Equal("value", new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters()).NameOf(0));

        [Fact]
        public void GetIndexFromName()
            => Assert.Equal(0, new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters()).IndexOf("value"));


        [Fact]
        public void GetInfoFromIndex()
            => Assert.NotNull(new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters()).GetInfo(0));

        [Fact]
        public void GetInfoFromName()
            => Assert.NotNull(new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters()).GetInfo("value"));

        [Fact]
        public void EnumerateGeneric()
            => Assert.Collection(new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters()), value => Assert.Equal(5, value));

        [Fact]
        public void Enumerate()
        {
            IEnumerable arguments = new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters());
            foreach (var value in arguments)
            {
                Assert.Equal(5, value);
            }
        }

        [Fact]
        public void Count()
            => Assert.Equal(1, new ArgumentCollection(new object[] { 5 }, testMethod.GetParameters()).Count);

        public static void Do(int value) { }
    }
}
