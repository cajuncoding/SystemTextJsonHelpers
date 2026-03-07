#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers.Converters
{
    public sealed class JsonRelaxedNullableConverterFactory(JsonRelaxedConverterOptions? options) : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.TryGetNullableUnderlyingType(out var underlyingType) && (
                IsSupportedNumericType(underlyingType!)
                || underlyingType == typeof(bool)
                || underlyingType == typeof(Guid)
                || underlyingType == typeof(DateTime)
                || underlyingType == typeof(DateTimeOffset)
                || underlyingType == typeof(TimeSpan)
                || underlyingType == typeof(DateOnly)
                || underlyingType == typeof(TimeOnly)
            );

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
            => typeToConvert.TryGetNullableUnderlyingType(out var underlyingType)
                ? (JsonConverter)Activator.CreateInstance(MaketJsonConverterGenericType(underlyingType!), options)!
                : throw new InvalidOperationException($"Unsupported type for {nameof(JsonRelaxedNullableConverterFactory)}; only Nullable<> types are supported.");

        private static Type MaketJsonConverterGenericType(Type underlyingType) => IsSupportedNumericType(underlyingType!)
            ? typeof(JsonRelaxedNullableNumberConverter<>).MakeGenericType(underlyingType!)
            : typeof(JsonRelaxedNullableGeneralConverter<>).MakeGenericType(underlyingType!);

        private static bool IsSupportedNumericType(Type t) =>
            t == typeof(byte) || t == typeof(sbyte)
            || t == typeof(short) || t == typeof(ushort)
            || t == typeof(int) || t == typeof(uint)
            || t == typeof(long) || t == typeof(ulong)
            || t == typeof(float) || t == typeof(double)
            || t == typeof(decimal);
    }
}