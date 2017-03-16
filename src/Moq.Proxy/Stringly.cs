using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Utilities for rendering .NET types as C# type names, with support for generics, 
/// nested types and aliases.
/// </summary>
static partial class Stringly
{
    /// <summary>
    /// Gets the C# name for the type, including proper rendering of generics, 
    /// using the simple name of types.
    /// </summary>
    /// <remarks>
    /// For example, for a generic enumerable of boolean, which has a full name of 
    /// <c>System.Collections.Generic.IEnumerable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]</c>, 
    /// this method returns IEnumerable&lt;Boolean&gt;.
    /// </remarks>
    /// <param name="type" this="true">The type to convert to a simple C# type name.</param>
    public static string ToTypeName(this Type type)
    {
        // If type is by ref, we need to get the actual element type of the ref. 
        // i.e. Object[]& has ElementType = Object[]
        var actualType = type.IsByRef && type.HasElementType ?
            type.GetElementType() : type;

        var genericArguments = new Queue<Type>();

        if (actualType.IsConstructedGenericType)
        {
            if (actualType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return ToTypeName(actualType.GenericTypeArguments[0]) + "?";

            foreach (var genericArgument in actualType.GenericTypeArguments)
            {
                genericArguments.Enqueue(genericArgument);
            }
        }

        return actualType.ToTypeName(genericArguments);
    }

    static string ToTypeName(this Type type, Queue<Type> genericArguments)
    {
        var typeName = string.Empty;

        if (type.DeclaringType != null && !type.IsGenericParameter)
        {
            typeName = type.DeclaringType.ToTypeName(genericArguments) + ".";
        }

        if (type.IsConstructedGenericType)
        {
            var index = type.Name.IndexOf('`');
            if (index != -1)
            {
                // TODO: detect Nullable<T>

                var genericArgumentsCount = int.Parse(type.Name.Substring(index + 1, type.Name.Length - index - 1));
                var values = new List<Type>();

                while (genericArgumentsCount > 0)
                {
                    values.Add(genericArguments.Dequeue());
                    genericArgumentsCount--;
                }

                typeName = typeName + type.Name.Substring(0, index) +
                    "<" +
                    string.Join(", ", values.Select(t => t.ToTypeName())) +
                    ">";
            }
            else
            {
                typeName = typeName + type.Name;
            }
        }
        else
        {
            if (type == typeof(byte)) return typeName + "byte";
            if (type == typeof(sbyte)) return typeName + "sbyte";
            if (type == typeof(short)) return typeName + "short";
            if (type == typeof(ushort)) return typeName + "ushort";
            if (type == typeof(int)) return typeName + "int";
            if (type == typeof(uint)) return typeName + "uint";
            if (type == typeof(long)) return typeName + "long";
            if (type == typeof(ulong)) return typeName + "ulong";
            if (type == typeof(char)) return typeName + "char";
            if (type == typeof(float)) return typeName + "float";
            if (type == typeof(double)) return typeName + "double";
            if (type == typeof(decimal)) return typeName + "decimal";
            if (type == typeof(bool)) return typeName + "bool";
            if (type == typeof(object)) return typeName + "object";
            if (type == typeof(string)) return typeName + "string";

            typeName = typeName + type.Name;
        }

        return typeName;
    }

    /// <summary>
    /// Gets the C# name for the type, including proper rendering of generics, 
    /// using full names of types.
    /// </summary>
    /// <remarks>
    /// For example, for a generic enumerable of boolean, which has a full name of 
    /// <c>System.Collections.Generic.IEnumerable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]</c>, 
    /// this method returns System.Collections.Generic.IEnumerable&lt;System.Boolean&gt;.
    /// </remarks>
    /// <param name="type" this="true">The type to convert to a C# full type name.</param>
    public static string ToTypeFullName(this Type type)
    {
        // If type is by ref, we need to get the actual element type of the ref. 
        // i.e. Object[]& has ElementType = Object[]
        var actualType = type.IsByRef && type.HasElementType ?
            type.GetElementType() : type;

        var genericArguments = new Queue<Type>();

        if (actualType.IsConstructedGenericType)
        {
            foreach (var genericArgument in actualType.GenericTypeArguments)
            {
                genericArguments.Enqueue(genericArgument);
            }
        }

        return actualType.ToTypeFullName(genericArguments);
    }

    static string ToTypeFullName(this Type type, Queue<Type> genericArguments)
    {
        var typeFullName = string.Empty;
        var typeName = type.Namespace + "." + type.Name;

        if (type.DeclaringType != null && !type.IsGenericParameter)
        {
            typeFullName = type.DeclaringType.ToTypeFullName(genericArguments) + ".";
        }

        if (type.DeclaringType != null || type.IsGenericParameter)
        {
            typeName = type.Name;
        }

        if (type.IsConstructedGenericType)
        {
            var index = typeName.IndexOf('`');
            if (index != -1)
            {
                var genericArgumentsCount = int.Parse(typeName.Substring(index + 1, typeName.Length - index - 1));
                var values = new List<Type>();

                while (genericArgumentsCount > 0)
                {
                    values.Add(genericArguments.Dequeue());
                    genericArgumentsCount--;
                }

                typeFullName = typeFullName + typeName.Substring(0, index) +
                    "<" +
                    string.Join(", ", values.Select(t => t.ToTypeFullName())) +
                    ">";
            }
            else
            {
                typeFullName = typeFullName + typeName;
            }
        }
        else
        {
            typeFullName = typeFullName + typeName;
        }

        return typeFullName;
    }
}