using System;
using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Text.Json;
using SystemTextJsonHelpers.Converters.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.CompilerServices;

namespace SystemTextJsonHelpers.Converters
{
    /// <summary>
    /// High-performance relaxed converter for nullable numeric primitives.
    /// Supported types include: byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal.
    /// </summary>
    public sealed class JsonRelaxedNullableNumberConverter<TNumber>(JsonRelaxedConverterOptions? options = null) : JsonConverter<TNumber?>
        where TNumber : struct, INumber<TNumber>, IParsable<TNumber>
    {
        public JsonRelaxedConverterOptions Options { get; } = options ?? JsonRelaxedConverterOptions.Default;

        private static bool IsSignedIntegral(Type t)
            => t == JsonTypeCache.LongType
            || t == JsonTypeCache.IntType
            || t == JsonTypeCache.ShortType
            || t == JsonTypeCache.SByteType;

        private static bool IsUnsignedIntegral(Type t)
            => t == JsonTypeCache.ULongType
            || t == JsonTypeCache.UIntType
            || t == JsonTypeCache.UShortType
            || t == JsonTypeCache.ByteType;

        public override TNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;

            if (tokenType is JsonTokenType.Null)
                return null;

            if (tokenType is JsonTokenType.Number)
            {
                try
                {
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
                return reader.GetString() switch
                {
                    var s when string.IsNullOrWhiteSpace(s) => null, //FIRST: Always treat empty/whitespace strings as null for all types!
                    //If ThousandsSeparators are allowed then we try to process it with fallback to manual cleaning (if absolutely necessary)...
                    var s when Options.AllowNumericParsingThousandsSeparators => TryParseRelaxedWithThousandsSeparators(s, out var parsedValue) ? parsedValue: null,
                    //Otherwise parse with normal invariant parsing expecting no formatting characters (e.g. commas).
                    var s when TNumber.TryParse(s, Options.CultureInfo, out var parsedNumber) => parsedNumber,
                    _ => null
                };
            }

            return reader.SkipReturnNull<TNumber>();
        }

        public override void Write(Utf8JsonWriter writer, TNumber? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteRawValue(value.Value.ToString(Options.NumberFormatString, Options.CultureInfo), skipInputValidation: true);
            else
                writer.WriteNullValue();
        }

        public static NumberStyles RelaxedIntegralNumberStyles { get; } = NumberStyles.Integer | NumberStyles.AllowThousands;
        public static NumberStyles RelaxedRationalNumberStyles { get; } = NumberStyles.Float | NumberStyles.AllowThousands;

        private bool TryParseRelaxedWithThousandsSeparators(string numberString, out TNumber? value)
            => numberString switch
            {
                //NOTE: IsNullOrWhiteSpace() should have already been checked prior to attempting to Parse at all so it's not necessary here!
                _ when TryParseInternal(numberString, out var parsedNumber) => (value = parsedNumber) != null,
                //Fallback to try again after cleaning common formatting characters (e.g. commas, spaces, non-breaking spaces, apostrophes)
                //  that may be present in some cultures or user input but are not valid for parsing directly.
                _ when TryParseInternal(numberString.SanitizeFormattingCharsFromNumberString(), out var parsedNumber) => (value = parsedNumber) != null,
                _ => (value = null) == null
            };


        private bool TryParseInternal(string input, out TNumber? result)
        {
            object? parsedValue = typeof(TNumber) switch
            {
                var t when t == JsonTypeCache.LongType && long.TryParse(input, RelaxedIntegralNumberStyles, Options.CultureInfo, out var l) => l,
                var t when t == JsonTypeCache.IntType && int.TryParse(input, RelaxedIntegralNumberStyles, Options.CultureInfo, out var i) => i,
                var t when t == JsonTypeCache.ShortType && short.TryParse(input, RelaxedIntegralNumberStyles, Options.CultureInfo, out var s) => s,
                var t when t == JsonTypeCache.SByteType && sbyte.TryParse(input, RelaxedIntegralNumberStyles, Options.CultureInfo, out var sb) => sb,
                var t when t == JsonTypeCache.ULongType && ulong.TryParse(input, RelaxedIntegralNumberStyles, Options.CultureInfo, out var ul) => ul,
                var t when t == JsonTypeCache.UIntType && uint.TryParse(input, RelaxedIntegralNumberStyles, Options.CultureInfo, out var ui) => ui,
                var t when t == JsonTypeCache.UShortType && ushort.TryParse(input, RelaxedIntegralNumberStyles, Options.CultureInfo, out var us) => us,
                var t when t == JsonTypeCache.ByteType && byte.TryParse(input, RelaxedIntegralNumberStyles, Options.CultureInfo, out var b) => b,
                var t when t == JsonTypeCache.DoubleType && double.TryParse(input, RelaxedRationalNumberStyles, Options.CultureInfo, out var d) => d,
                var t when t == JsonTypeCache.FloatType && float.TryParse(input, RelaxedRationalNumberStyles, Options.CultureInfo, out var f) => f,
                var t when t == JsonTypeCache.DecimalType && decimal.TryParse(input, NumberStyles.Number, Options.CultureInfo, out var dec) => dec,
                _ => null
            };

            result = parsedValue.As<TNumber>();
            return result != null;
        }
    }
}
