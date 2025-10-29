using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloakId.Abstractions;

namespace CloakId;

/// <summary>
/// JSON converter for properties marked with [Cloak].
/// </summary>
[Obsolete("Prefer ConcreteCloakIdPropertyConverterFactory for JSON serialization and deserialization")]
public class CloakIdPropertyConverter(Type propertyType, ICloakIdCodec codec) : JsonConverter<object>
{
    /// <summary>
    /// Reads and converts the JSON to the specified type.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">Options to control the conversion behavior.</param>
    /// <returns>The converted value.</returns>
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            if (Nullable.GetUnderlyingType(propertyType) != null) return null;
            throw new JsonException($"Cannot convert null to non-nullable type {propertyType}");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var encodedValue = reader.GetString()!;
            return codec.Decode(encodedValue, propertyType);
        }

        throw new JsonException($"Expected string token for CloakId property of type {propertyType}, but got {reader.TokenType}");
    }

    /// <summary>
    /// Writes the specified value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">Options to control the conversion behavior.</param>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var encoded = codec.Encode(value, propertyType);
        writer.WriteStringValue(encoded);
    }
}

/// <summary>
/// Concrete JSON Converter for properties marked with [Cloak]
/// </summary>
/// <typeparam name="T">Target <see cref="IBinaryInteger{TSelf}"/> of the attributed property.</typeparam>
public sealed class CloakIdPropertyConverter<T>(ICloakIdCodec codec) : JsonConverter<T> where T : IBinaryInteger<T>, IMinMaxValue<T>
{
    private readonly ICloakIdCodec _codec = codec;

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            if (Nullable.GetUnderlyingType(typeof(T)) != null) return default;
            throw new JsonException($"Cannot convert null to non-nullable type {typeof(T)}");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var encodedValue = reader.GetString()!;
            return _codec.Decode<T>(encodedValue);
        }

        throw new JsonException($"Expected string token for CloakId property of type {typeof(T)}, but got {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var encoded = _codec.Encode(value);
        writer.WriteStringValue(encoded);
    }
}
