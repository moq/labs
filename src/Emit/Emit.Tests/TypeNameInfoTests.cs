using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Stunts.Emit.Static;
using Xunit;

namespace Emit.Tests
{
    public class TypeNameInfoSpec
    {
        [InlineData(typeof(int))]
        [InlineData(typeof(int?))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(Environment.SpecialFolder))]
        [InlineData(typeof(int[,]))]
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(IEnumerable<int?>))]
        [InlineData(typeof(IEnumerable<int[]>))]
        [InlineData(typeof(IEnumerable<IDictionary<System.Boolean[], Environment.SpecialFolder>>))]
        [InlineData(typeof(IEnumerable<KeyValuePair<string, int>>))]
        [InlineData(typeof(IDictionary<string, KeyValuePair<int[], IEnumerable<bool>>>))]
        [InlineData(typeof(IEnumerable<ITypeSymbol[]>))]
        [InlineData(typeof(System.ComponentModel.TypeConverter.StandardValuesCollection))]
        [Theory]
        public void RoundtripFromMetadataName(Type type)
        {
            var expected = type.AssemblyQualifiedName;

            var info = TypeNameInfo.FromMetadataName(expected);
            var actual = info.ToMetadataName();

            Assert.Equal(expected, actual);
        }

        [InlineData(typeof(int[][]))]
        [InlineData(typeof(int[][,]))]
        [InlineData(typeof(int[,][,]))]
        [Theory]
        public void UnsupportedArrayTypes(Type type)
        {
            Assert.Throws<NotSupportedException>(() => TypeNameInfo.FromMetadataName(type.AssemblyQualifiedName));
        }
    }
}
