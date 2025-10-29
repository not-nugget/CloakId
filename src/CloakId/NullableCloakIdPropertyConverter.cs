using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloakId.Abstractions;

namespace CloakId;

/// <summary>
/// <see cref="CloakIdPropertyConverter{T}"/> decorator for handling <see cref="Nullable{T}"/> properties in a type-safe way
/// </summary>
/// <typeparam name="N"><see cref="Nullable{T}"/> of <typeparamref name="T"/></typeparam>
/// <typeparam name="T">Target <see cref="IBinaryInteger{TSelf}"/> of the attributed property.</typeparam>
public sealed class NullableCloakIdPropertyConverter<T>(ICloakIdCodec codec) : JsonConverter<T?> where T : struct, IBinaryInteger<T>, IMinMaxValue<T>
{
    private readonly CloakIdPropertyConverter<T> _subconverter = new(codec);

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if(reader.TokenType is JsonTokenType.Null)
        {
            return default;
        }

        return _subconverter.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if(value is null)
        {
            writer.WriteNullValue();
            return; 
        }

        _subconverter.Write(writer, value.Value, options);
    }
}
