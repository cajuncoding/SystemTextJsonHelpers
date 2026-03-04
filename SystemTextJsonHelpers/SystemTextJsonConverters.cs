using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTextJsonHelpers
{
    //NOTE: We use Macross.Json.Extensions to greatly simplify providing simple value converters such as this...
    public class JsonRelaxedDateTimeConverter() : JsonDelegatedStringConverter<DateTime>(
        value => DateTime.Parse(value),
        value => value.ToString("s", CultureInfo.InvariantCulture)
    );

    //NOTE: We use Macross.Json.Extensions to greatly simplify providing simple value converters such as this...
    public class JsonRelaxedDateTimeOffsetConverter() : JsonDelegatedStringConverter<DateTimeOffset>(
        value => DateTimeOffset.Parse(value),
        //Render our DateTimeOffset with support for Timezone but without the unnecessary 7 fractional seconds digits like "o" outputs...
        value => value.ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture)
    );

    /// <summary>
    /// A relaxed Boolean value converter for System.Text.Json that works with bool or string bool (e.g. 'true', 'false) case-insensitive values.
    /// Inspired and adapted from original StackOverflow source here: https://stackoverflow.com/a/75089641/7293142
    /// Enhanced to have streamlined code, support case-insensitive matching, and improved exception messages.
    ///
    /// Taken directly from Original @CajunCoding's Gist Source: https://gist.github.com/cajuncoding/00896396fdeddabdd661aca8524165d1
    /// 
    /// </summary>
    public class JsonRelaxedBooleanStringConverter() : JsonConverter<bool>
    {
        private static readonly JsonException BooleanParsingException = new("The boolean property could not be read as a valid boolean" +
                                                                            " json value or parsed from boolean string value (e.g. 'true'/'false').");
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => reader.GetString() switch
            {
                //NOTE: string.Equals() is null safe...
                var value when string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) => true,
                var value when string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase) => false,
                _ => throw BooleanParsingException
            },
            _ => throw BooleanParsingException
        };

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            => writer.WriteBooleanValue(value);
    }

    public class JsonRelaxedNullableIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.Number => reader.GetInt32(),
                JsonTokenType.String => reader.GetString()!.ParseAsNullableInt(),
                _ => null
            };

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
