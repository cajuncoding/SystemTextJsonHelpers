using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SystemTextJsonHelpers.Converters;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers
{
    public static class JsonConstants
    {
        public const string EmptyJsonObject = "{}";
        public const string EmptyJsonArray = "[]";
    }

    public static class SystemTextJsonDefaults
    {
        static SystemTextJsonDefaults()
        {
            ConfigureWebDefaults();
        }

        public static JsonSerializerOptions DefaultSerializerOptions { get; internal set; } = new JsonSerializerOptions();

        public static void ConfigureGeneralDefaults(Action<JsonSerializerOptions>? configureAction = null)
           => ConfigureDefaults(JsonSerializerDefaults.General, configureAction);

        
        public static void ConfigureWebDefaults(Action<JsonSerializerOptions>? configureAction = null)
           => ConfigureDefaults(JsonSerializerDefaults.Web, configureAction);

        /// <summary>
        /// Configures the standard Web defaults (camelCase, case-insensitive, numbers-as-strings reading, etc.) along with
        /// a number of other relaxed converters to make working with non-strict Json sources much easier
        /// by providing more relaxed parsing of DateTime, DateTimeOffset, String Enums, and String Boolean values.
        /// </summary>
        public static void ConfigureRelaxedWebDefaults(Action<JsonSerializerOptions>? configureAction = null)
            => ConfigureDefaultsInternal(configureAction, CreateRelaxedJsonSerializerOptions());

        public static void ConfigureDefaults(JsonSerializerDefaults defaultsEnum, Action<JsonSerializerOptions>? configureAction)
            => ConfigureDefaultsInternal(configureAction, new JsonSerializerOptions(defaultsEnum));

        public static void ConfigureDefaultsInternal(Action<JsonSerializerOptions>? configureAction, JsonSerializerOptions jsonSerializerOptions)
        {
            configureAction?.Invoke(jsonSerializerOptions);
            ConfigureDefaults(jsonSerializerOptions);
        }

        public static void ConfigureDefaults(JsonSerializerOptions jsonSerializerOptions)
        {
            ArgumentNullException.ThrowIfNull(jsonSerializerOptions);
            DefaultSerializerOptions = jsonSerializerOptions;
        }

        public static JsonSerializerOptions CreateRelaxedJsonSerializerOptions(
            bool allowStringEnums = true,
            JsonNamingPolicy? enumNamingPolicy = null,
            string enumFlagsStringOutputSeparator = JsonRelaxedConverterOptions.DefaultEnumFlagsStringOutputSeparator,
            bool allowNumericEnums = true,
            EnumWriteStyle enumJsonWriteStyle = EnumWriteStyle.StringOutput,
            bool allowReadingBooleanValuesFromStrings = true,
            bool allowWritingNullValues = true,
            bool allowNumericParsingThousandsSeparators = true,
            string numberFormatString = "G", // General format specifier => '123456789.0123456789'
            bool allowRelaxedDateAndTimeParsing = true,
            string dateTimeFormatString = "O", // ISO 8601 Round-trip ("O"/"o") => '2024-07-16T14:33:12.4570000-05:00'
            string dateTimeOffsetFormatString = "O", // ISO 8601 Round-trip ("O"/"o") => '2024-07-16T14:33:12.4570000-05:00'
            string dateOnlyFormatString = "O", // ISO 8601 Date ("O"/"o") => '2024-07-16'
            string timeOnlyFormatString = "O", // ISO 8601 Time ("O"/"o") => '14:33:12.4570000'
            string timeSpanFormatString = "c", // TimeSpan => '[-][d.]hh:mm:ss.fffffff',
            CultureInfo? cultureInfo = null
        )
        {
            var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = allowWritingNullValues ? JsonIgnoreCondition.Never : JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,                
            };

            var relaxedConverterOptions = new JsonRelaxedConverterOptions(
                allowNumericEnumValues: allowNumericEnums,
                enumJsonWriteStyle: enumJsonWriteStyle,
                enumNamingPolicy: enumNamingPolicy,
                enumFlagsStringOutputSeparator: enumFlagsStringOutputSeparator,
                allowNumericParsingThousandsSeparators: allowNumericParsingThousandsSeparators,
                numberFormatString: numberFormatString,
                dateTimeFormatString: dateTimeFormatString,
                dateTimeOffsetFormatString: dateTimeOffsetFormatString,
                dateOnlyFormatString: dateOnlyFormatString,
                timeOnlyFormatString: timeOnlyFormatString,
                timeSpanFormatString: timeSpanFormatString
            );

            //Add Converters that will help provide more relaxed parsing (similar to Newtonsoft.Json)...
            var converters = jsonSerializerOptions.Converters;

            //Optionally handle relaxed parsing of non-Nullable boolean values!
            if (allowReadingBooleanValuesFromStrings)
                converters.Add(new JsonRelaxedBooleanConverter());

            if(allowRelaxedDateAndTimeParsing)
            {
                converters.Add(new JsonRelaxedDateTimeConverter(relaxedConverterOptions));
                converters.Add(new JsonRelaxedDateTimeOffsetConverter(relaxedConverterOptions));
                converters.Add(new JsonRelaxedDateOnlyConverter(relaxedConverterOptions));
                converters.Add(new JsonRelaxedTimeOnlyConverter(relaxedConverterOptions));
            }

            //Optionally handle relaxed parsing of Enums with full support for case-insensitive parsing
            //  and annotations (e.g. [EnumMember(Name="")] & [JsonPropertyName("")])...
            if (allowStringEnums)
                converters.Add(new JsonRelaxedEnumConverterFactory(relaxedConverterOptions));

            //REQUIRED to handle relaxed parsing of Nullable numeric and other supported values!
            //NOTE: Configuraiton options are provided to the Nullable converter factory, so that it can
            //  tailor the relaxed parsing rules as specified...
            converters.Add(new JsonRelaxedNullableConverterFactory(relaxedConverterOptions));

            return jsonSerializerOptions;
        }
    }
}
