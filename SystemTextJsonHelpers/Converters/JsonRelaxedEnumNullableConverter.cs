using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers.Converters
{
    /// <summary>
    /// Nullable wrapper so TEnum? also gets alias + numeric support but with relaxed handling so an invalid value will be treated as null 
    ///     instead of throwing an exception (e.g. for forward compatibility when new enum values are added but old clients are still deserializing).
    ///     
    /// Replaces the built-in JsonStringEnumConverter behavior by adding the following in addition to the default case-insensitive matching:
    /// - Supports one or more [EnumMember(Value="alias")] annotations (case-insensitive on read; exact alias preferred on write).
    /// - Supports one or more [JsonPropertyName("alias")] annotations (case-insensitive on read; alias preferred on write).
    /// - NOTE: Supports any combination of annotations and in any order however the first defined item will be the Preferred Write alias!
    /// </summary>
    public sealed class JsonRelaxedEnumNullableConverter<TEnum>(JsonRelaxedConverterOptions? options = null)
        : JsonConverter<TEnum?> where TEnum : struct, Enum
    {
        private readonly JsonRelaxedEnumConverter<TEnum> _enumConverter = new(options);

        public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.String or JsonTokenType.Number when _enumConverter.TryRead(ref reader, typeToConvert, options, out var enumResult) => enumResult,
                _ => reader.SkipReturnNull<TEnum>()
            };

        public override void Write(Utf8JsonWriter writer, TEnum? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                _enumConverter.Write(writer, value.Value, options);
            else
                writer.WriteNullValue();
        }
    }

}
