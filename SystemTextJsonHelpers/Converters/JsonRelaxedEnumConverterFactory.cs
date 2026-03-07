using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers.Converters
{
    public sealed class JsonRelaxedEnumConverterFactory(JsonRelaxedConverterOptions? options = null) : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsEnum || (typeToConvert.TryGetNullableUnderlyingType(out var underlyingType) && underlyingType!.IsEnum);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            var jsonConverterType = typeToConvert switch
            {
                Type t when t.IsEnum => typeof(JsonRelaxedEnumConverter<>).MakeGenericType(t)!,
                Type t when t.TryGetNullableUnderlyingType(out var underlyingType) => typeof(JsonRelaxedEnumNullableConverter<>).MakeGenericType(underlyingType!),
                _ => throw new InvalidOperationException("Unsupported type for JsonRelaxedEnumConverterFactory. Can only convert Enum types and Nullable<Enum> types.")
            };
            
            return (JsonConverter)Activator.CreateInstance(jsonConverterType, options)!;
        }
    }
}
