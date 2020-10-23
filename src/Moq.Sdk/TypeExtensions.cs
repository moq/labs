﻿using System;
using System.Reflection;

namespace Moq.Sdk
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets the <see cref="TypeInfo"/> for the underlying value that 
        /// the given type represents. Takes into account that a byref type 
        /// should actually be considered by its element type (so that 
        /// `string&amp;` matches `string`.
        /// </summary>
        public static TypeInfo GetValueTypeInfo(this Type type)
            => type.IsByRef && type.HasElementType ?
                type.GetElementType().GetTypeInfo() :
                type.GetTypeInfo();
    }
}
