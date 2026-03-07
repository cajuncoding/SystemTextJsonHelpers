using System.Text.Json;

namespace SystemTextJsonHelpers.Converters.Utilities
{
    internal static class JsonConverterExtensions
    {
        public static T? SkipReturnNull<T>(this ref Utf8JsonReader reader) where T : struct
        {
            reader.Skip();
            return null;
        }

        public static T? As<T>(this object? obj) where T : struct
            => obj is T result ? result : null;

        public static bool TryGetNullableUnderlyingType(this Type? type, out Type? nullableUnderlyingType)
        {
            nullableUnderlyingType = null;
            if (type is not null && type.IsGenericType && type.GetGenericTypeDefinition() == JsonTypeCache.NullableOpenType)
                nullableUnderlyingType = Nullable.GetUnderlyingType(type);

            return nullableUnderlyingType is not null;
        }

        public static bool IsNullableType(this Type? type)
            => type.TryGetNullableUnderlyingType(out _);
    }
}
