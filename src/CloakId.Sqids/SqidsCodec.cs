using System.Numerics;
using CloakId.Abstractions;
using Sqids;

namespace CloakId.Sqids;

/// <summary>
/// Sqids implementation of ICloakIdCodec for encoding and decoding integer IDs.
/// </summary>
public class SqidsCodec(
    SqidsEncoder<int> intEncoder,
    SqidsEncoder<byte> byteEncoder,
    SqidsEncoder<uint> uintEncoder,
    SqidsEncoder<long> longEncoder,
    SqidsEncoder<sbyte> sbyteEncoder,
    SqidsEncoder<ulong> ulongEncoder,
    SqidsEncoder<short> shortEncoder,
    SqidsEncoder<ushort> ushortEncoder) : ICloakIdCodec
{
    /// <inheritdoc />
    public string Encode<T>(T value) where T : IBinaryInteger<T>, IMinMaxValue<T> => value switch
    {
        int v => intEncoder.Encode(v),
        uint v => uintEncoder.Encode(v),
        byte v => byteEncoder.Encode(v),
        long v => longEncoder.Encode(v),
        sbyte v => sbyteEncoder.Encode(v),
        ulong v => ulongEncoder.Encode(v),
        short v => shortEncoder.Encode(v),
        ushort v => ushortEncoder.Encode(v),
        _ => throw new NotSupportedException($"Type '{typeof(T)}' is not supported for encoding.")
    };

    /// <inheritdoc />
    public T Decode<T>(string value) where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        try
        {
            IReadOnlyList<T>? decoded = null;
            if (typeof(T) == typeof(int)) decoded = (IReadOnlyList<T>)intEncoder.Decode(value);
            if (typeof(T) == typeof(uint)) decoded = (IReadOnlyList<T>)uintEncoder.Decode(value);
            if (typeof(T) == typeof(byte)) decoded = (IReadOnlyList<T>)byteEncoder.Decode(value);
            if (typeof(T) == typeof(long)) decoded = (IReadOnlyList<T>)longEncoder.Decode(value);
            if (typeof(T) == typeof(sbyte)) decoded = (IReadOnlyList<T>)sbyteEncoder.Decode(value);
            if (typeof(T) == typeof(ulong)) decoded = (IReadOnlyList<T>)ulongEncoder.Decode(value);
            if (typeof(T) == typeof(short)) decoded = (IReadOnlyList<T>)shortEncoder.Decode(value);
            if (typeof(T) == typeof(ushort)) decoded = (IReadOnlyList<T>)ushortEncoder.Decode(value);
            if (decoded is null)
                throw new NotSupportedException($"Type '{typeof(T)}' is not supported for decoding.");

            var reencoded = Encode(decoded.Single());
            if (value != reencoded)
            {
                throw new ArgumentException(
                    $"Invalid non-canonical encoding '{value}'. The canonical encoding for this value is '{reencoded}'.",
                    nameof(value));
            }

            return decoded[0];
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Unable to decode '{value}' to type {typeof(T).Name}.", nameof(value), ex);
        }
    }

    /// <inheritdoc />
    public string EncodeAll<T, U>(U value) where T : IBinaryInteger<T>, IMinMaxValue<T> where U : IEnumerable<T> => value switch
    {
        IEnumerable<int> v => intEncoder.Encode(v),
        IEnumerable<uint> v => uintEncoder.Encode(v),
        IEnumerable<byte> v => byteEncoder.Encode(v),
        IEnumerable<long> v => longEncoder.Encode(v),
        IEnumerable<sbyte> v => sbyteEncoder.Encode(v),
        IEnumerable<ulong> v => ulongEncoder.Encode(v),
        IEnumerable<short> v => shortEncoder.Encode(v),
        IEnumerable<ushort> v => ushortEncoder.Encode(v),
        _ => throw new NotSupportedException("Unsupported unmanaged binary integer type encountered when attempting to encode multiple numbers")
    };

    /// <inheritdoc />
    public IReadOnlyList<T> DecodeAll<T>(string value) where T : IBinaryInteger<T>, IMinMaxValue<T>
    {
        try
        {
            IReadOnlyList<T>? decoded = default;
            if (typeof(T) == typeof(int)) decoded = (IReadOnlyList<T>)intEncoder.Decode(value);
            if (typeof(T) == typeof(uint)) decoded = (IReadOnlyList<T>)uintEncoder.Decode(value);
            if (typeof(T) == typeof(byte)) decoded = (IReadOnlyList<T>)byteEncoder.Decode(value);
            if (typeof(T) == typeof(long)) decoded = (IReadOnlyList<T>)longEncoder.Decode(value);
            if (typeof(T) == typeof(sbyte)) decoded = (IReadOnlyList<T>)sbyteEncoder.Decode(value);
            if (typeof(T) == typeof(ulong)) decoded = (IReadOnlyList<T>)ulongEncoder.Decode(value);
            if (typeof(T) == typeof(short)) decoded = (IReadOnlyList<T>)shortEncoder.Decode(value);
            if (typeof(T) == typeof(ushort)) decoded = (IReadOnlyList<T>)ushortEncoder.Decode(value);
            if(decoded is null || !decoded.Any())
                throw new NotSupportedException($"Type '{typeof(T)}' is not supported for decoding.");

            var reencoded = EncodeAll<T, IReadOnlyList<T>>(decoded);
            if(value != reencoded)
            {
                throw new ArgumentException(
                    $"Invalid non-canonical encoding '{value}'. The canonical encoding for this value is '{reencoded}'.",
                    nameof(value));
            }

            return decoded;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Unable to decode '{value}' to type {typeof(T).Name}.", nameof(value), ex);
        }
    }

    #region Deprecated Boxing Methods
    /// <summary>
    /// Encodes a numeric value to a string using Sqids.
    /// </summary>
    /// <remarks>
    /// This method, along with its boxing <see cref="Encode(string, Type)"/> counterpart is deprecated in favor of the generic
    /// <see cref="Encode{T}(string)"/> and <see cref="EncodeAll{T, U}(string)"/> methods, and may be removed in a future release.
    /// Existing projects will still function when using these methods, however new projects are recommended to use the non-boxing
    /// generic versions
    [Obsolete("Prefer generic Encode* methods instead of the boxing version")]
    public string Encode(object value, Type valueType)
    {
        var actualType = Nullable.GetUnderlyingType(valueType) ?? valueType;

        return actualType switch
        {
            Type t when t == typeof(int) => intEncoder.Encode((int)value),
            Type t when t == typeof(uint) => uintEncoder.Encode((uint)value),
            Type t when t == typeof(long) => longEncoder.Encode((long)value),
            Type t when t == typeof(ulong) => ulongEncoder.Encode((ulong)value),
            Type t when t == typeof(short) => shortEncoder.Encode((short)value),
            Type t when t == typeof(ushort) => ushortEncoder.Encode((ushort)value),
            _ => throw new NotSupportedException($"Type '{actualType}' is not supported for encoding.")
        };
    }

    /// <summary>
    /// Decodes a Sqids string back to the original numeric value.
    /// Validates that the input is the canonical encoding to prevent multiple IDs resolving to the same value.
    /// </summary>
    /// <remarks>
    /// This method, along with its boxing <see cref="Decode(string, Type)"/> counterpart is deprecated in favor of the generic
    /// <see cref="Decode{T}(string)"/> and <see cref="DecodeAll{T, U}(string)"/> methods, and may be removed in a future release.
    /// Existing projects will still function when using these methods, however new projects are recommended to use the non-boxing
    /// generic versions
    /// </remarks>
    [Obsolete("Prefer generic Decode* methods instead of the boxing version")]
    public object Decode(string encodedValue, Type targetType)
    {
        var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            var (decodedValue, canonicalEncoding) = actualType switch
            {
                Type t when t == typeof(int) => DecodeInt(encodedValue),
                Type t when t == typeof(uint) => DecodeUInt(encodedValue),
                Type t when t == typeof(long) => DecodeLong(encodedValue),
                Type t when t == typeof(ulong) => DecodeULong(encodedValue),
                Type t when t == typeof(short) => DecodeShort(encodedValue),
                Type t when t == typeof(ushort) => DecodeUShort(encodedValue),
                _ => throw new NotSupportedException($"Type '{actualType}' is not supported for decoding.")
            };

            // Validate canonical encoding - prevents multiple IDs from resolving to the same value
            if (encodedValue != canonicalEncoding)
            {
                throw new ArgumentException(
                    $"Invalid non-canonical encoding '{encodedValue}'. The canonical encoding for this value is '{canonicalEncoding}'.",
                    nameof(encodedValue));
            }

            return decodedValue;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Unable to decode '{encodedValue}' to type {actualType.Name}.", nameof(encodedValue), ex);
        }
    }

    private (object DecodedValue, string CanonicalEncoding) DecodeInt(string encodedValue)
    {
        var result = intEncoder.Decode(encodedValue);
        if (result.Count == 0)
        {
            throw new ArgumentException($"Unable to decode '{encodedValue}' - invalid format.", nameof(encodedValue));
        }

        var decodedValue = result[0];
        var canonicalEncoding = intEncoder.Encode(decodedValue);
        return (decodedValue, canonicalEncoding);
    }

    private (object DecodedValue, string CanonicalEncoding) DecodeUInt(string encodedValue)
    {
        var result = uintEncoder.Decode(encodedValue);
        if (result.Count == 0)
        {
            throw new ArgumentException($"Unable to decode '{encodedValue}' - invalid format.", nameof(encodedValue));
        }

        var decodedValue = result[0];
        var canonicalEncoding = uintEncoder.Encode(decodedValue);
        return (decodedValue, canonicalEncoding);
    }

    private (object DecodedValue, string CanonicalEncoding) DecodeLong(string encodedValue)
    {
        var result = longEncoder.Decode(encodedValue);
        if (result.Count == 0)
        {
            throw new ArgumentException($"Unable to decode '{encodedValue}' - invalid format.", nameof(encodedValue));
        }

        var decodedValue = result[0];
        var canonicalEncoding = longEncoder.Encode(decodedValue);
        return (decodedValue, canonicalEncoding);
    }

    private (object DecodedValue, string CanonicalEncoding) DecodeULong(string encodedValue)
    {
        var result = ulongEncoder.Decode(encodedValue);
        if (result.Count == 0)
        {
            throw new ArgumentException($"Unable to decode '{encodedValue}' - invalid format.", nameof(encodedValue));
        }

        var decodedValue = result[0];
        var canonicalEncoding = ulongEncoder.Encode(decodedValue);
        return (decodedValue, canonicalEncoding);
    }

    private (object DecodedValue, string CanonicalEncoding) DecodeShort(string encodedValue)
    {
        var result = shortEncoder.Decode(encodedValue);
        if (result.Count == 0)
        {
            throw new ArgumentException($"Unable to decode '{encodedValue}' - invalid format.", nameof(encodedValue));
        }

        var decodedValue = result[0];
        var canonicalEncoding = shortEncoder.Encode(decodedValue);
        return (decodedValue, canonicalEncoding);
    }

    private (object DecodedValue, string CanonicalEncoding) DecodeUShort(string encodedValue)
    {
        var result = ushortEncoder.Decode(encodedValue);
        if (result.Count == 0)
        {
            throw new ArgumentException($"Unable to decode '{encodedValue}' - invalid format.", nameof(encodedValue));
        }

        var decodedValue = result[0];
        var canonicalEncoding = ushortEncoder.Encode(decodedValue);
        return (decodedValue, canonicalEncoding);
    }
    #endregion
}
