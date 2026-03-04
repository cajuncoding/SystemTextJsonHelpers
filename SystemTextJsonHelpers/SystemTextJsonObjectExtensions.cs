using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SystemTextJsonHelpers
{
    public static class SystemTextJsonObjectExtensions
    {
        public static T? FromJsonTo<T>(this string jsonText, JsonSerializerOptions? options = null)
            => !string.IsNullOrWhiteSpace(jsonText)
                ? JsonSerializer.Deserialize<T>(jsonText, options ?? SystemTextJsonDefaults.DefaultSerializerOptions)
                : default;

        public static string ToJson(this object value, JsonSerializerOptions? options = null)
            => JsonSerializer.Serialize(value, options ?? SystemTextJsonDefaults.DefaultSerializerOptions);

        public static string ToJson<T>(this T value, JsonSerializerOptions? options = null)
            => JsonSerializer.Serialize<T>(value, options ?? SystemTextJsonDefaults.DefaultSerializerOptions);

        public static JsonNode? ToJsonNode(this object obj, JsonSerializerOptions? options = null) => obj is not null
            ? JsonSerializer.SerializeToNode(obj, options ?? SystemTextJsonDefaults.DefaultSerializerOptions)
            : null;

        public static T? FromJsonTo<T>(this JsonNode jsonNode, JsonSerializerOptions? options = null)
            => jsonNode.Deserialize<T>(options ?? SystemTextJsonDefaults.DefaultSerializerOptions);

        public static string? ToJsonIndented(this object obj, JsonSerializerOptions? options = null)
        {
            if (obj is null) return null;

            //NOTE: This will use the SystemTextJsonDefaults.DefaultSerializerOptions as defined in the Application Root Startup!
            var jsonWriteIndentedOptions = new JsonSerializerOptions(options ?? SystemTextJsonDefaults.DefaultSerializerOptions) {
                WriteIndented = true
            };

            return obj.ToJson(jsonWriteIndentedOptions);
        }

        public static string EscapeJsonForLogging(this string jsonText)
            => jsonText.IsDuckTypedJson()
                ? jsonText.Replace("{", "{{").Replace("}", "}}")
                : jsonText;
    }
}
