#nullable enable
using System;
using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTextJsonHelpers
{
    public sealed class RelaxedNullableConverterFactory : JsonConverterFactory
    {
        private static readonly Type NullableOpenType = typeof(Nullable<>);

        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != NullableOpenType)
                return false;

            var innerType = typeToConvert.GetGenericArguments()[0];

            return IsSupportedNumber(innerType)
                || innerType == typeof(bool)
                || innerType == typeof(Guid)
                || innerType == typeof(Uri)
                || innerType == typeof(DateTime)
                || innerType == typeof(DateTimeOffset)
                || innerType == typeof(TimeSpan)
                || innerType == typeof(DateOnly)
                || innerType == typeof(TimeOnly)
                || innerType.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var innerType = typeToConvert.GetGenericArguments()[0];

            return (JsonConverter)Activator.CreateInstance(IsSupportedNumber(innerType)
                ? typeof(RelaxedNullableNumberConverter<>).MakeGenericType(innerType)
                : typeof(RelaxedNullableGeneralConverter<>).MakeGenericType(innerType)
            )!;
        }

        private static bool IsSupportedNumber(Type t) =>
            t == typeof(byte) || t == typeof(sbyte)
            || t == typeof(short) || t == typeof(ushort)
            || t == typeof(int) || t == typeof(uint)
            || t == typeof(long) || t == typeof(ulong)
            || t == typeof(float) || t == typeof(double)
            || t == typeof(decimal);
    }

    /// <summary>
    /// High-performance relaxed converter for Nullable numeric primitives.
    /// </summary>
    public sealed class RelaxedNullableNumberConverter<TNumber> : JsonConverter<TNumber?>
        where TNumber : struct, INumber<TNumber>, IParsable<TNumber>
    {
        private static readonly IFormatProvider Invariant = CultureInfo.InvariantCulture;

        public override TNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;

            if (tokenType is JsonTokenType.Null)
                return null;

            if (tokenType is JsonTokenType.Number)
            {
                try
                {
                    static bool IsSignedIntegral(Type t) => t == typeof(long) || t == typeof(int) || t == typeof(short) || t == typeof(sbyte);
                    static bool IsUnsignedIntegral(Type t) => t == typeof(ulong) || t == typeof(uint) || t == typeof(ushort) || t == typeof(byte);

                    return typeof(TNumber) switch
                    {
                        var t when t == typeof(decimal) && reader.TryGetDecimal(out var dec) => dec.As<TNumber>(),
                        var t when t == typeof(double) && reader.TryGetDouble(out var dbl) => dbl.As<TNumber>(),
                        var t when t == typeof(float) && reader.TryGetDouble(out var fl) => TNumber.CreateChecked(fl),
                        var t when IsSignedIntegral(t) && reader.TryGetInt64(out var int64) => TNumber.CreateChecked(int64),
                        var t when IsUnsignedIntegral(t) && reader.TryGetUInt64(out var uint64) => TNumber.CreateChecked(uint64),
                        _ when reader.TryGetDecimal(out var dec2) => TNumber.CreateChecked(dec2),
                        _ when reader.TryGetDouble(out var dbl2) => TNumber.CreateChecked(dbl2),
                        _ => null
                    };
                }
                catch
                {
                    // overflow / invalid fractional for integrals → null
                    return null;
                }
            }

            if (tokenType is JsonTokenType.String)
            {
                ReadOnlySpan<byte> utf8 = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                var s = Encoding.UTF8.GetString(utf8);
                return TNumber.TryParse(s, Invariant, out var parsedNumber) ? parsedNumber : null;
            }

            return reader.SkipReturnNull<TNumber>();
        }

        public override void Write(Utf8JsonWriter writer, TNumber? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteRawValue(value.Value.ToString("G", Invariant), skipInputValidation: true);
            else
                writer.WriteNullValue();
        }
    }

    /// <summary>
    /// General relaxed converter for Nullable&lt;T&gt; using pattern matching (non-numeric T).
    /// </summary>
    public sealed class RelaxedNullableGeneralConverter<T> : JsonConverter<T?> where T : struct
    {
        private static readonly IFormatProvider Invariant = CultureInfo.InvariantCulture;

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch {
                JsonTokenType.Null => null,
                JsonTokenType.String => ParseFromStringSafely(reader.GetString()!),
                JsonTokenType.True when typeof(T) == typeof(bool) => true.As<T>(),
                JsonTokenType.False when typeof(T) == typeof(bool) => false.As<T>(),
                _ => reader.SkipReturnNull<T>()
            };
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
                writer.WriteNullValue();
            else
                switch ((object)value.Value) // Pattern match on boxed value for optimal Utf8JsonWriter overload selection
                {
                    case bool b: writer.WriteBooleanValue(b); break;
                    case Guid g: writer.WriteStringValue(g); break;
                    case Uri u: writer.WriteStringValue(u.OriginalString); break;
                    case DateTime dt: writer.WriteStringValue(dt); break;
                    case DateTimeOffset dto: writer.WriteStringValue(dto); break;
                    case TimeSpan ts: writer.WriteStringValue(ts.ToString()); break;
                    case DateOnly d: writer.WriteStringValue(d.ToString("O", Invariant)); break;
                    case TimeOnly t: writer.WriteStringValue(t.ToString("O", Invariant)); break;
                    default: writer.WriteStringValue(value.Value.ToString()); break; // enums & other structs
                }
        }

        private static T? ParseFromStringSafely(string stringValue)
        {
            var s = stringValue.Trim();
            return typeof(T) switch
            {
                var _ when string.IsNullOrWhiteSpace(s) => null, //FIRST: Always treat empty/whitespace strings as null for all types!
                var t when t == typeof(bool) => bool.TryParse(s, out var b) ? b.As<T>() : null,
                var t when t == typeof(Guid) => Guid.TryParse(s, out var g) ? g.As<T>() : null,
                var t when t == typeof(Uri) => Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var u) ? u.As<T>() : null,
                var t when t == typeof(DateTime) => DateTime.TryParse(s, Invariant, DateTimeStyles.RoundtripKind, out var dt) ? dt.As<T>() : null,
                var t when t == typeof(DateTimeOffset) => DateTimeOffset.TryParse(s, Invariant, DateTimeStyles.RoundtripKind, out var dto) ? dto.As<T>() : null,
                var t when t == typeof(TimeSpan) => TimeSpan.TryParse(s, Invariant, out var ts) ? ts.As<T>() : null,
                var t when t == typeof(DateOnly) => DateOnly.TryParse(s, Invariant, DateTimeStyles.AllowWhiteSpaces, out var d) ? d.As<T>() : null,
                var t when t == typeof(TimeOnly) => TimeOnly.TryParse(s, Invariant, DateTimeStyles.AllowWhiteSpaces, out var to) ? to.As<T>() : null,
                var t when t.IsEnum => Enum.TryParse(t, s, ignoreCase: true, out var e) ? e.As<T>() : null,
                _ => null
            };
        }
    }

    internal static class JsonConverterExtensions
    {
        public static T? SkipReturnNull<T>(this ref Utf8JsonReader reader) where T : struct
        {
            reader.Skip();
            return null;
        }

        public static T? As<T>(this object obj) => (T?)obj;
    }
}