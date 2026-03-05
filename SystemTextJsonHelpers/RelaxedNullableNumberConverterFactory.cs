using System;
using System.Text;
using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class RelaxedNullableNumberConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Only handle Nullable<T> where T is a number type supporting INumber<T>.
        if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(Nullable<>))
            return false;

        var t = typeToConvert.GetGenericArguments()[0];
        // We only want to support known numeric primitives (optional guard) or any INumber<T>
        var numberInterface = typeof(INumber<>).MakeGenericType(t);
        return numberInterface.IsAssignableFrom(t);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var innerType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(RelaxedNullableNumberConverter<>).MakeGenericType(innerType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public sealed class RelaxedNullableNumberConverter<TNumber> : JsonConverter<TNumber?>
    where TNumber : struct, INumber<TNumber>, IParsable<TNumber>
{
    private static readonly IFormatProvider Invariant = CultureInfo.InvariantCulture;

    public override TNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tokenType = reader.TokenType;

        // null -> null
        if (tokenType is JsonTokenType.Null)
            return null;

        // If the token is a JSON number, prefer converting from decimal/double for accuracy:
        if (tokenType is JsonTokenType.Number)
        {
            // Try decimal first (exact for many cases, especially integrals and decimals)
            if (reader.TryGetDecimal(out var dec))
            {
                try
                {
                    // Convert to TNumber (throws on overflow/invalid fractional for integrals)
                    return TNumber.CreateChecked(dec);
                }
                catch
                {
                    return null; // overflow/invalid -> null
                }
            }

            // Fallback: double (covers exponent notation etc.)
            if (reader.TryGetDouble(out var dbl))
            {
                try
                {
                    return TNumber.CreateChecked(dbl);
                }
                catch
                {
                    return null;
                }
            }

            // If neither decimal nor double worked, treat as failure
            return null;
        }

        // If the token is a string, parse it using TNumber.TryParse
        if (tokenType is JsonTokenType.String)
        {
            // Avoid allocation when possible
            ReadOnlySpan<byte> utf8 = reader.HasValueSequence
                ? reader.ValueSequence.ToArray()
                : reader.ValueSpan;

            // Convert UTF-8 to string (we could also use Span-based parsing if available)
            var s = Encoding.UTF8.GetString(utf8);

            if (TNumber.TryParse(s, Invariant, out var parsedNumber))
                return parsedNumber;

            return null;
        }

        // Anything else (objects, arrays, booleans) -> null
        reader.Skip(); // consume token to keep reader consistent
        return null;
    }

    public override void Write(Utf8JsonWriter writer, TNumber? value, JsonSerializerOptions options)
    {
        if (value.HasValue) // Write as number with invariant formatting
            writer.WriteRawValue(value.Value.ToString("G", Invariant), skipInputValidation: true);
        else
            writer.WriteNullValue();
    }
}