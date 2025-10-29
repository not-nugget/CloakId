namespace CloakId;

/// <summary>
/// Provides helpers for various different operations against <see cref="Type"/>s
/// </summary>
public static class TypeHelpers
{
    /// <summary>
    /// Is <paramref name="type"/> a CLR-unmanaged numeric type? Unraps <see cref="Nullable{T}"/>s
    /// </summary>
    public static bool IsNumericType(this Type type)
    {
        var actualType = Nullable.GetUnderlyingType(type) ?? type;
        return actualType == typeof(int) ||
               actualType == typeof(uint) ||
               actualType == typeof(byte) ||
               actualType == typeof(sbyte) ||
               actualType == typeof(long) ||
               actualType == typeof(ulong) ||
               actualType == typeof(short) ||
               actualType == typeof(ushort);
    }

    /// <summary>
    /// Is <paramref name="type"/> <see langword="typeof"/>(<see cref="Guid"/>) or <see langword="typeof"/>(<see cref="Nullable{T}"/>) of <see cref="Guid"/>?
    /// </summary>
    public static bool IsGuidType(this Type type)
    {
        return (Nullable.GetUnderlyingType(type) ?? type) == typeof(Guid);
    }

    /// <summary>
    /// Gets a user-friendly representation of <paramref name="type"/>
    /// </summary>
    public static string GetFriendlyTypeName(this Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            return GetSimpleTypeName(underlyingType) + "?";
        }
        return GetSimpleTypeName(type);
    }

    /// <summary>
    /// Gets a simplistic representation of <paramref name="type"/>
    /// </summary>
    public static string GetSimpleTypeName(this Type type)
    {
        return type.Name switch
        {
            "Int32" => "int",
            "UInt8" => "byte",
            "UInt32" => "uint",
            "Int64" => "long",
            "Int8" => "sbyte",
            "UInt64" => "ulong",
            "Int16" => "short",
            "UInt16" => "ushort",
            _ => type.Name.ToLowerInvariant()
        };
    }
}
