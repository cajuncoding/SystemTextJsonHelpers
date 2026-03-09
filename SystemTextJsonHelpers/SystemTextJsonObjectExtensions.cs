using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SystemTextJsonHelpers
{
    /// <summary>
    /// Basic core JSON Data Types enumeration for use with simplifed JsonNode Property extensions, etc.
    /// to providea morestrongly-typed way to filter and work with JSON data.
    /// NOTE: Enum is not a Flags enum but the values are Atomic (single-bit) values to make
    /// easy mapping/casting to JsonDataTypeFilter for filtering purposes.
    /// </summary>
    public enum JsonDataType : byte
    {
        String = 1 << 0, // 1
        Number = 1 << 1, // 2
        Boolean = 1 << 2, // 4
        Object = 1 << 3, // 8
        Array = 1 << 4, // 16
        Null = 1 << 5, // 32
    }

    [Flags]
    /// <summary>
    /// Basic core JSON Data Types enumeration for use with simplifed JsonNode Property extensions, etc.
    /// to providea morestrongly-typed way to filter and work with JSON data.
    /// NOTE: Enum is not a Flags enum but the values are Atomic (single-bit) values to make
    /// easy mapping/casting to JsonDataTypeFilter for filtering purposes.
    /// </summary>
    public enum JsonDataTypeFilter : byte
    {
        None = 0,

        // Atomic (single-bit) flags
        String = 1 << 0, // 1
        Number = 1 << 1, // 2
        Boolean = 1 << 2, // 4
        Object = 1 << 3, // 8
        Array = 1 << 4, // 16
        Null = 1 << 5, // 32

        // Composite flags
        PrimitiveDataTypes = String | Number | Boolean | Null,
        AllDataTypes = PrimitiveDataTypes | Object | Array
    }

    public static class SystemTextJsonObjectExtensions
    {
        /// <summary>
        /// Converts the given JSON string to an instance of the specified type T using System.Text.Json deserialization and the 
        /// provided JsonSerializerOptions or the globally configured SystemTextJsonDefaults.DefaultSerializerOptions if none are provided.
        /// </summary>
        public static T? FromJsonTo<T>(this string jsonText, JsonSerializerOptions? options = null)
            => !string.IsNullOrWhiteSpace(jsonText)
                ? JsonSerializer.Deserialize<T>(jsonText, options ?? SystemTextJsonDefaults.DefaultSerializerOptions)
                : default;

        /// <summary>
        /// Converts the given JsonNode to an instance of the specified type T using System.Text.Json deserialization and the 
        /// provided JsonSerializerOptions or the globally configured SystemTextJsonDefaults.DefaultSerializerOptions if none are provided.
        /// </summary>
        public static T? FromJsonTo<T>(this JsonNode jsonNode, JsonSerializerOptions? options = null)
            => jsonNode.Deserialize<T>(options ?? SystemTextJsonDefaults.DefaultSerializerOptions);

        /// <summary>
        /// Converts the given object to a JSON string using System.Text.Json serialization and the provided JsonSerializerOptions or the 
        /// globally configured SystemTextJsonDefaults.DefaultSerializerOptions if none are provided.
        /// </summary>
        public static string ToJson(this object value, JsonSerializerOptions? options = null)
            => JsonSerializer.Serialize(value, options ?? SystemTextJsonDefaults.DefaultSerializerOptions);

        /// <summary>
        /// Converts the given object to a JSON string using System.Text.Json serialization and the provided JsonSerializerOptions or the 
        /// globally configured SystemTextJsonDefaults.DefaultSerializerOptions if none are provided.
        /// </summary>
        public static string ToJson<T>(this T value, JsonSerializerOptions? options = null)
            => JsonSerializer.Serialize<T>(value, options ?? SystemTextJsonDefaults.DefaultSerializerOptions);

        /// <summary>
        /// Converts the given object to a JsonNode using System.Text.Json serialization and the provided JsonSerializerOptions or the 
        /// globally configured SystemTextJsonDefaults.DefaultSerializerOptions if none are provided.
        /// NOTE: If the object is already a JsonNode it will be returned as-is, and if it's a string that appears to be JSON it will be 
        ///         deserialized to a JsonNode, otherwise the object will be serialized to a JsonNode.
        /// </summary>
        public static JsonNode? ToJsonNode(this object obj, JsonSerializerOptions? options = null) 
            => obj switch
            {
                null => null,
                JsonNode jsonNode => jsonNode,
                string jsonText when jsonText.IsDuckTypedJson() => jsonText.FromJsonTo<JsonNode>(options ?? SystemTextJsonDefaults.DefaultSerializerOptions),
                _ => JsonSerializer.SerializeToNode(obj, options ?? SystemTextJsonDefaults.DefaultSerializerOptions)
            };

        /// <summary>
        /// Serilizes the given object to a JSON string with indented formatting for improved readability, 
        ///     using the specified JsonSerializerOptions or the default options if none are provided.
        /// </summary>
        public static string? ToJsonIndented(this object obj, JsonSerializerOptions? options = null)
        {
            if (obj is null) return null;

            //NOTE: This will use the SystemTextJsonDefaults.DefaultSerializerOptions as defined in the Application Root Startup!
            var jsonWriteIndentedOptions = new JsonSerializerOptions(options ?? SystemTextJsonDefaults.DefaultSerializerOptions) {
                WriteIndented = true
            };

            return obj.ToJson(jsonWriteIndentedOptions);
        }

        /// <summary>
        /// Escapes all braces in the JSON with double braces to prevent issues when logging JSON strings with structured logging frameworks 
        ///     that use braces for property placeholders (e.g. Serilog, Microsoft.Extensions.Logging, etc.).
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public static string EscapeJsonForLogging(this string jsonText)
            => jsonText.IsDuckTypedJson()
                ? jsonText.Replace("{", "{{").Replace("}", "}}")
                : jsonText;

        /// <summary>
        /// Safely get the value of the specified property field name from the JsonObject, 
        /// returning the default value if the field is not found or if any error occurs during retrieval or conversion.
        /// </summary>
        public static TValue PropertyValueSafely<TValue>(this JsonObject? jsonObject, string propertyName, TValue defaultValue = default, JsonSerializerOptions? options = null)
            => jsonObject?[propertyName] is JsonNode propertyNode && propertyNode.TryGetValueSafely<TValue>(out var value, options) ? value : defaultValue;

        /// <summary>
        /// Safely get the value of the given JsonNode if possible returning the default value if if any error occurs during retrieval or conversion.
        /// NOTE: This is only valid for JsonVlaue nodes or JsonNodes that can be deserialized to the target type, otherwise it will return the default value.
        /// For JsonObject nodes use the overload that takes in a `fieldName` to retrieve.
        /// </summary>
        public static TValue ValueSafely<TValue>(this JsonNode? jsonNode, TValue defaultValue = default!, JsonSerializerOptions? options = null)
            => jsonNode is not null && jsonNode.TryGetValueSafely<TValue>(out var value, options) ? value : defaultValue;

        /// <summary>
        /// Safely attempts to get the value of the specified property field name from the JsonObject, 
        /// returning true based on if the property exists, and it's value is successfully retrieved & parsed. 
        /// Otherwise if any error occurs retreiving the property or its' value then false is returned.
        /// </summary>
        public static bool TryGetPropertyValueSafely<TValue>(this JsonObject? jsonObject, string propertyName, out TValue value, JsonSerializerOptions? options = null)
        {
            value = default!;
            return jsonObject?[propertyName] is JsonNode propNode && propNode.TryGetValueSafely<TValue>(out value, options);
        }

        /// <summary>
        /// Safely attempts to get the value of the specified property field name from the JsonObject, 
        /// returning true based on if the property exists, and it's value is successfully retrieved & parsed. 
        /// Otherwise if any error occurs retreiving the property or its' value then false is returned.
        /// </summary>
        public static bool TryGetValueSafely<TValue>(this JsonNode? jsonNode, out TValue value, JsonSerializerOptions? options = null)
        {
            value = default!;
            try
            {
                (bool mappedStatus, TValue mappedValue) = jsonNode switch
                {
                    //Short circuit for null case to avoid unnecessary allocations & processing...
                    null => (false, default!),
                    //For primitives JsonValue.TryGetValue<TValue> is fast...
                    JsonValue valueNode when valueNode.TryGetValue<TValue>(out TValue? primitiveValue) => (true, primitiveValue),
                    //Fallback to System.Text.Json deserialization
                    JsonNode node when node.Deserialize<TValue>(options ?? SystemTextJsonDefaults.DefaultSerializerOptions) is TValue parsedValue => (true, parsedValue),
                    _ => (false, default(TValue)!)
                };

                value = mappedValue!;
                return mappedStatus;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieve the actual JSON Data Type of the current JsonNode based on the underlying JsonValueKind, 
        /// returning null if the JsonNode is null or if the JsonValueKind is not a valid JSON data type (e.g. JsonValueKind.Undefined).
        /// A null JsonDataType result indicates the current JsonNode is not a JsonValue, JsonObject, and/or is an invalid or unsupported JSON data type,
        /// allowing for more robust handling of unexpected JSON structures without throwing exceptions.
        /// </summary>
        public static JsonDataType? GetJsonDataType(this JsonNode jsonNode)
            => jsonNode?.GetValueKind() switch
            {
                JsonValueKind.String => JsonDataType.String,
                JsonValueKind.Number => JsonDataType.Number,
                JsonValueKind.True or JsonValueKind.False => JsonDataType.Boolean,
                JsonValueKind.Object => JsonDataType.Object,
                JsonValueKind.Array => JsonDataType.Array,
                null or JsonValueKind.Null => JsonDataType.Null,
                //If not valid (e.g. JsonValueKind.Undefined or other) then we return null Enum result...
                _ => null
            };

        /// <summary>
        /// Retrieves the property names (keys) of the given JsonObject, safely returning an empty enumerable if the JsonObject is null or has no properties.
        /// </summary>
        public static IEnumerable<string> GetPropertyNames(this JsonObject json)
            => json?.Select(kvp => kvp.Key) ?? Enumerable.Empty<string>();

        /// <summary>
        /// Retrieves the properties (key-value pairs) of the given JsonObject, safely returning an empty enumerable if the JsonObject is null or has no properties.
        /// Properties returned can be filtered by their JSON data type using the optional `dataTypeFilter` parameter, allowing for more targeted retrieval of 
        ///     properties based on their underlying JSON data types.
        /// Filters may be combined using bitwise OR (|) to include multiple data types in the results, or the `JsonDataTypeFilter.AllDataTypes` composite 
        ///     filter can be used to include all properties regardless of their JSON data type.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, JsonNode?>> GetProperties(this JsonObject jsonObject, JsonDataTypeFilter dataTypeFilter = JsonDataTypeFilter.AllDataTypes)
            => jsonObject?.Where(kv =>
                kv.Value?.GetJsonDataType() is JsonDataType jsonDataType
                && (((JsonDataTypeFilter)jsonDataType) & dataTypeFilter) != 0
            ) ?? Enumerable.Empty<KeyValuePair<string, JsonNode?>>();

    }
}
