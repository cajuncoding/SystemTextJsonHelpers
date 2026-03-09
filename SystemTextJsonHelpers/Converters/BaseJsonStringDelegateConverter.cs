using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers.Converters
{
    public abstract class BaseJsonStringDelegateConverter<T> : JsonConverter<T>
    {
        protected BaseJsonStringDelegateConverter(
            Func<string, JsonRelaxedConverterOptions, T> convertValueFromStringFunc,
            Func<T, JsonRelaxedConverterOptions, string> convertValueToStringFunc,
            JsonRelaxedConverterOptions? options = null
        )
        {
            _convertFromStringWithOptionsFunc = convertValueFromStringFunc ?? throw new ArgumentNullException(nameof(convertValueFromStringFunc));
            _convertToStringWithOptionsFunc = convertValueToStringFunc ?? throw new ArgumentNullException(nameof(convertValueToStringFunc));
            _convertFromStringFunc = null;
            _convertToStringFunc = null;
            
            Options = options ?? JsonRelaxedConverterOptions.Default;
        }

        protected BaseJsonStringDelegateConverter(
            Func<string, T> convertValueFromStringFunc,
            Func<T, string> convertValueToStringFunc
        )
        {
            _convertFromStringWithOptionsFunc = null;
            _convertToStringWithOptionsFunc = null;
            _convertFromStringFunc = convertValueFromStringFunc ?? throw new ArgumentNullException(nameof(convertValueFromStringFunc));
            _convertToStringFunc = convertValueToStringFunc ?? throw new ArgumentNullException(nameof(convertValueToStringFunc));
            
            Options = JsonRelaxedConverterOptions.Default;
        }

        private bool _enableOptionsParam = false;

        public JsonRelaxedConverterOptions Options { get; }

        private readonly Func<string, JsonRelaxedConverterOptions, T>? _convertFromStringWithOptionsFunc;
        private readonly Func<T, JsonRelaxedConverterOptions, string>? _convertToStringWithOptionsFunc;
        private readonly Func<string,  T>? _convertFromStringFunc;
        private readonly Func<T, string>? _convertToStringFunc;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType switch
            {
                JsonTokenType.String when _convertFromStringWithOptionsFunc is not null => _convertFromStringWithOptionsFunc(reader.GetString()!, Options),
                JsonTokenType.String when _convertFromStringFunc is not null => _convertFromStringFunc(reader.GetString()!),
                _ => throw new JsonException($"A valid Json String token type is required for [{this.GetType().Name}] as an implementations of [{nameof(BaseJsonStringDelegateConverter<T>)}].")
            };

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if(_convertToStringWithOptionsFunc?.Invoke(value, Options) is string outputWithOptionsString)
                writer.WriteStringValue(outputWithOptionsString);
            else if (_convertToStringFunc?.Invoke(value) is string outputString)
                writer.WriteStringValue(outputString);
            else
                writer.WriteNullValue();
        }
    }
}
