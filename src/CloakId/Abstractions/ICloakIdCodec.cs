using System.Numerics;

namespace CloakId.Abstractions;

/// <summary>
/// Provides encoding and decoding functionality for numeric values.
/// </summary>
public interface ICloakIdCodec
{
    /// <summary>
    /// Encodes a <typeparamref name="T"/> value to a string.
    /// </summary>
    /// <param name="value">The <typeparamref name="T"/> value to encode.</param>
    /// <typeparam name="T">The target <see cref="INumber{TSelf}"/> type.</typeparam>
    /// <returns>The encoded string.</returns>
    string Encode<T>(T value) where T : IBinaryInteger<T>, IMinMaxValue<T>;

    /// <summary>
    /// Decodes a string back to a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="encodedValue">The encoded string.</param>
    /// <typeparam name="T">The target <see cref="INumber{TSelf}"/> type.</typeparam>
    /// <returns>The decoded numeric value.</returns>
    T Decode<T>(string value) where T : IBinaryInteger<T>, IMinMaxValue<T>;

    /// <summary>
    /// Encodes an arbitrary collection of <typeparamref name="T"/> values to a string.
    /// </summary>
    /// <param name="value">The <typeparamref name="T"/> values to encode.</param>
    /// <typeparam name="T">The target <see cref="INumber{TSelf}"/> type.</typeparam>
    /// <returns>The encoded string.</returns>
    string EncodeAll<T, U>(U value) where T : IBinaryInteger<T>, IMinMaxValue<T> where U : IEnumerable<T>;

    /// <summary>
    /// Decodes a string back to an arbitrary collection of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="encodedValue">The encoded string.</param>
    /// <typeparam name="T">The target <see cref="INumber{TSelf}"/> type.</typeparam>
    /// <returns>The decoded numeric values.</returns>
    IReadOnlyList<T> DecodeAll<T>(string value) where T : IBinaryInteger<T>, IMinMaxValue<T>;

    /// <summary>
    /// Encodes a numeric value to a string.
    /// </summary>
    /// <param name="value">The numeric value to encode.</param>
    /// <param name="valueType">The type of the numeric value.</param>
    /// <returns>The encoded string.</returns>
    [Obsolete("Prefer generic Encode* methods instead of the boxing version")]
    string Encode(object value, Type valueType);

    /// <summary>
    /// Decodes a string back to a numeric value of the specified type.
    /// </summary>
    /// <param name="encodedValue">The encoded string.</param>
    /// <param name="targetType">The target numeric type.</param>
    /// <returns>The decoded numeric value.</returns>
    [Obsolete("Prefer generic Decode* methods instead of the boxing version")]
    object Decode(string encodedValue, Type targetType);
}
