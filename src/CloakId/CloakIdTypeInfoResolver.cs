using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CloakId.Abstractions;

namespace CloakId;

/// <summary>
/// JSON converter modifier that handles properties marked with [Cloak] attribute.
/// </summary>
public class CloakIdTypeInfoResolver(ICloakIdCodec codec) : DefaultJsonTypeInfoResolver
{
    private static readonly Type _openConverter = typeof(CloakIdPropertyConverter<>);
    private static readonly Type _openNullableConverter = typeof(NullableCloakIdPropertyConverter<>);
    private static readonly Type _openNullable = typeof(Nullable<>);

    private readonly ICloakIdCodec _codec = codec;

    /// <summary>
    /// Gets the type information for the specified type, adding CloakId custom converters for properties marked with [Cloak].
    /// </summary>
    /// <param name="type">The type to get information for.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The JSON type information with CloakId converters applied where appropriate.</returns>
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            Dictionary<Type, JsonConverter> converterCache = [];
            object[] ctorArr = [ _codec ];
            foreach (var propertyInfo in jsonTypeInfo.Properties)
            {
                var propertyType = propertyInfo.PropertyType;
                var property = type.GetProperty(propertyInfo.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property?.GetCustomAttribute<CloakAttribute>() != null &&
                    propertyType.IsNumericType())
                {
                    var converter = CollectionsMarshal.GetValueRefOrAddDefault(converterCache, propertyType, out var exists);
                    if (exists)
                    {
                        propertyInfo.CustomConverter = converter;
                        continue;
                    }

                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == _openNullable)
                    {
                        converter = (JsonConverter)Activator.CreateInstance(_openNullableConverter.MakeGenericType(Nullable.GetUnderlyingType(propertyType)!), ctorArr)!;
                        propertyInfo.CustomConverter = converter;
                        continue;
                    }

                    converter = (JsonConverter)Activator.CreateInstance(_openConverter.MakeGenericType(propertyType), ctorArr)!;
                    propertyInfo.CustomConverter = converter;
                }
            }
        }

        return jsonTypeInfo;
    }
}
