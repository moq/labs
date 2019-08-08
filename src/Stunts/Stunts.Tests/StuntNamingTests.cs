﻿using System;
using Xunit;

namespace Stunts.Tests
{
    public class StuntNamingTests
    {
        [Theory]
        [InlineData("StuntFactoryIDisposableIServiceProvider" + StuntNaming.DefaultSuffix, typeof(StuntFactory), typeof(IServiceProvider), typeof(IDisposable))]
        public void GetNameOrdersTypes(string expectedName, Type baseType, params Type[] implementedInterfaces)
            => Assert.Equal(expectedName, StuntNaming.GetName(baseType, implementedInterfaces));

        [Theory]
        [InlineData(StuntNaming.DefaultNamespace + ".StuntFactoryIDisposableIServiceProvider" + StuntNaming.DefaultSuffix, typeof(StuntFactory), typeof(IServiceProvider), typeof(IDisposable))]
        public void GetFullNameOrdersTypes(string expectedName, Type baseType, params Type[] implementedInterfaces)
            => Assert.Equal(expectedName, StuntNaming.GetFullName(baseType, implementedInterfaces));
    }
}
