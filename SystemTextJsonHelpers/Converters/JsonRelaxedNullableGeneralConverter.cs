using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers.Converters
{
    /// <summary>
    /// General relaxed converter for Nullable&lt;T&gt; using pattern matching (non-numeric T).
    /// Supported types include: bool, Guid, DateTime, DateTimeOffset, TimeSpan, DateOnly, TimeOnly.
    /// </summary>
    public sealed class JsonRelaxedNullableGeneralConverter<T>(JsonRelaxedConverterOptions? options = null) : JsonConverter<T?> where T : struct
    {
        private static readonly IFormatProvider Invariant = CultureInfo.InvariantCulture;

        public JsonRelaxedConverterOptions Options { get; } = options ?? JsonRelaxedConverterOptions.Default;

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.String => ParseFromStringSafely(reader.GetString()!),
                JsonTokenType.True when typeof(T) == JsonTypeCache.BoolType => true.As<T>(),
                JsonTokenType.False when typeof(T) == JsonTypeCache.BoolType => false.As<T>(),
                _ => reader.SkipReturnNull<T>()
            };
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
                writer.WriteNullValue();
            else switch ((object)value.Value)
            {
                case bool b: writer.WriteBooleanValue(b); break;
                case Guid g: writer.WriteStringValue(g); break;
                case DateTime dt: writer.WriteStringValue(dt); break;  // DateTime => ISO 8601 Round-trip ("O"/"o") => '2024-07-16T14:33:12.4570000-05:00'
                case DateTimeOffset dto: writer.WriteStringValue(dto); break; // DateTimeOffset => ISO 8601 Round-trip ("O"/"o") => '2024-07-16T14:33:12.4570000-05:00'
                case TimeSpan ts: writer.WriteStringValue(ts.ToString("c", Invariant)); break; // TimeSpan => '[-][d.]hh:mm:ss.fffffff'
                case DateOnly d: writer.WriteStringValue(d.ToString("O", Invariant)); break; // DateOnly => ISO 8601 Date ("O"/"o") => '2024-07-16'
                case TimeOnly t: writer.WriteStringValue(t.ToString("O", Invariant)); break; // TimeOnly => ISO 8601 Time ("O"/"o") => '14:33:12.4570000'
                default: writer.WriteStringValue(value.Value.ToString()); break;
            }
        }

        private static T? ParseFromStringSafely(string stringValue)
        {
            var s = stringValue.Trim();
            return typeof(T) switch
            {
                var _ when string.IsNullOrWhiteSpace(s) => null, //FIRST: Always treat empty/whitespace strings as null for all types!
                var t when t == JsonTypeCache.BoolType && bool.TryParse(s, out var b) => b.As<T>(),
                var t when t == JsonTypeCache.GuidType && Guid.TryParse(s, out var g) => g.As<T>(),
                var t when t == JsonTypeCache.DateTimeType && DateTime.TryParse(s, Invariant, DateTimeStyles.RoundtripKind, out var dt) => dt.As<T>(),
                var t when t == JsonTypeCache.DateTimeOffsetType && DateTimeOffset.TryParse(s, Invariant, DateTimeStyles.RoundtripKind, out var dto) => dto.As<T>(),
                var t when t == JsonTypeCache.TimeSpanType && TimeSpan.TryParse(s, Invariant, out var ts) => ts.As<T>(),
                var t when t == JsonTypeCache.DateOnlyType && DateOnly.TryParse(s, Invariant, DateTimeStyles.AllowWhiteSpaces, out var d) => d.As<T>(),
                var t when t == JsonTypeCache.TimeOnlyType && TimeOnly.TryParse(s, Invariant, DateTimeStyles.AllowWhiteSpaces, out var to) => to.As<T>(),
                _ => null
            };
        }
    }
}
