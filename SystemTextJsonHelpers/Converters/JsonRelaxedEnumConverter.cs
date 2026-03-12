using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers.Converters
{
    /// <summary>
    /// Replaces the built-in JsonStringEnumConverter behavior by adding the following in addition to the default case-insensitive matching:
    /// - Supports one or more [EnumMember(Value="alias")] annotations (case-insensitive on read; exact alias preferred on write).
    /// - Supports one or more [JsonPropertyName("alias")] annotations (case-insensitive on read; alias preferred on write).
    /// - NOTE: Supports any combination of annotations and in any order however the first defined item will be the Preferred Write alias!
    /// </summary>
    public sealed class JsonRelaxedEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public static Type EnumType { get; } = typeof(TEnum);
        
        public JsonRelaxedConverterOptions Options { get; }
        
        private readonly EnumMapping _enumMapping;

        public JsonRelaxedEnumConverter(JsonRelaxedConverterOptions? options = null)
        {
            Options = options ?? JsonRelaxedConverterOptions.Default;
            _enumMapping = EnumMapping.FromCache(EnumType, Options.EnumNamingPolicy);
        }

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //For Non-nullable Enums enforce validation and throw exceptions for invalid token types or values that cannot be parsed
            //  to ensure the caller is aware of the issue and can handle it appropriately (vs using Nullable Enum we coerce all failures to null).
            if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.Number))
                throw new JsonException($"A valid Json String or Number token type is expected for enum parsing to '{EnumType}'.");

            return TryRead(ref reader, typeToConvert, options, out var enumResult)
                ? enumResult!.Value
                : throw new JsonException($"Value is invalid and cannot be parsed for enum '{EnumType}'.");
        }

        /// <summary>
        /// Helper method that handles parsing safely for use between both the non-nullable and nullable converters so that the 
        ///     nullable converter can return null instead of throwing an exception when parsing fails.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <param name="enumResult"></param>
        /// <returns></returns>
        public bool TryRead(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, out TEnum? enumResult)
        {
            enumResult = reader.TokenType switch
            {
                // FLAGS strings like "Read, Write" or "read-alias | WRITE-ALIAS"
                JsonTokenType.String when _enumMapping.IsFlags && _enumMapping.TryParseFlags(reader.GetString()!, out var flags, Options.EnumFlagsStringOutputSeparator) => flags.As<TEnum>(),
                // Single token: alias/policy/name
                JsonTokenType.String when _enumMapping.TryGetValue(reader.GetString(), out var result) => result.As<TEnum>(),
                // Numeric (signed underlying)
                JsonTokenType.Number when Options.AllowNumericEnumValues && reader.TryGetInt64(out var intValue) => _enumMapping.GetValue(intValue).As<TEnum>(),
                // Numeric (unsigned underlying)
                JsonTokenType.Number when Options.AllowNumericEnumValues && reader.TryGetUInt64(out var u64Value) => _enumMapping.GetValue(u64Value).As<TEnum>(),
                _ => null
            };

            return enumResult is not null;
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            if (Options.EnumJsonWriteStyle is EnumWriteStyle.NumberOutput)
                writer.WriteNumberValue(Convert.ToUInt64(value));
            else if (_enumMapping.IsFlags && _enumMapping.TryFormatFlags(value, out var formattedResult, Options.EnumFlagsStringOutputSeparator))
                writer.WriteStringValue(formattedResult);
            else
                writer.WriteStringValue(_enumMapping.GetPreferredName(value));
        }
    }
}
