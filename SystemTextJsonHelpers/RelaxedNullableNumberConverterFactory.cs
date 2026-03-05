//using System;
//using System.Text;
//using System.Buffers;
//using System.Globalization;
//using System.Numerics;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace SystemTextJsonHelpers
//{
//    public sealed class RelaxedNullableNumberConverterFactory : JsonConverterFactory
//    {
//        private static readonly Type NullableTypeCache = typeof(Nullable<>);

//        public override bool CanConvert(Type typeToConvert)
//        {
//            if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != NullableTypeCache)
//                return false;

//            var t = typeToConvert.GetGenericArguments()[0];

//            // Fast Check to ensure we only process supported number types, which prevents us from accidentally trying to
//            //  process something like Nullable<DateTime> which would pass the INumber<T> constraint but what we want to handle here!
//            return t == typeof(byte) || t == typeof(sbyte)
//                    || t == typeof(short) || t == typeof(ushort)
//                    || t == typeof(int) || t == typeof(uint)
//                    || t == typeof(long) || t == typeof(ulong)
//                    || t == typeof(float) || t == typeof(double)
//                    || t == typeof(decimal);
//        }

//        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
//        {
//            var innerType = typeToConvert.GetGenericArguments()[0];
//            var converterType = typeof(RelaxedNullableNumberConverter<>).MakeGenericType(innerType);
//            return (JsonConverter)Activator.CreateInstance(converterType)!;
//        }
//    }

//    public sealed class RelaxedNullableNumberConverter<TNumber> : JsonConverter<TNumber?>
//        where TNumber : struct, INumber<TNumber>, IParsable<TNumber>
//    {
//        private static readonly IFormatProvider Invariant = CultureInfo.InvariantCulture;

//        public override TNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//        {
//            var tokenType = reader.TokenType;

//            // JTokenType.Null ==> null
//            if (tokenType is JsonTokenType.Null)
//                return null;

//            // If the token is a JSON number, prefer converting from decimal/double for accuracy:
//            if (tokenType is JsonTokenType.Number)
//            {
//                try
//                {
//                    // Try decimal first (exact for many cases, especially integrals and decimals)
//                    // NOTE: Convert to TNumber (throws on overflow/invalid fractional for integrals)
//                    return reader.TryGetDecimal(out var dec) ? TNumber.CreateChecked(dec)
//                        : reader.TryGetDouble(out var dbl) ? TNumber.CreateChecked(dbl)
//                        : null; // If we can't get as decimal or double, treat as failure
//                }
//                catch { /*Overflow/Invalid Exception ==> DO NOTHING ==> null*/ }

//                // If neither decimal nor double worked, treat as failure
//                return null;
//            }

//            // If the token is a string, parse it using TNumber.TryParse
//            if (tokenType is JsonTokenType.String)
//            {
//                // Avoid allocation when possible
//                ReadOnlySpan<byte> utf8 = reader.HasValueSequence
//                    ? reader.ValueSequence.ToArray()
//                    : reader.ValueSpan;

//                // Convert UTF-8 to string (we could also use Span-based parsing if available)
//                var s = Encoding.UTF8.GetString(utf8);

//                return TNumber.TryParse(s, Invariant, out var parsedNumber)
//                    ? parsedNumber
//                    : null;
//            }

//            // Anything else (objects, arrays, booleans) -> null
//            reader.Skip(); // consume token to keep reader consistent
//            return null;
//        }

//        public override void Write(Utf8JsonWriter writer, TNumber? value, JsonSerializerOptions options)
//        {
//            if (value.HasValue) // Write as number with invariant formatting
//                writer.WriteRawValue(value.Value.ToString("G", Invariant), skipInputValidation: true);
//            else
//                writer.WriteNullValue();
//        }
//    }
//}