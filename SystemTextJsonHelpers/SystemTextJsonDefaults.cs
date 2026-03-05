using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTextJsonHelpers
{
    public static class Json
    {
        public const string EmptyJsonObject = "{}";
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
        public static void ConfigureRelaxedWebDefaults()
            => ConfigureDefaults(RelaxedWebDefaults);

        public static void ConfigureDefaults(JsonSerializerDefaults defaultsEnum, Action<JsonSerializerOptions>? configureAction)
        {
            var jsonSerializerOptions = new JsonSerializerOptions(defaultsEnum);
            configureAction?.Invoke(jsonSerializerOptions);
            ConfigureDefaults(jsonSerializerOptions);
        }

        public static void ConfigureDefaults(JsonSerializerOptions jsonSerializerOptions)
        {
            ArgumentNullException.ThrowIfNull(jsonSerializerOptions);
            DefaultSerializerOptions = jsonSerializerOptions;
        }

        public static readonly JsonSerializerOptions RelaxedWebDefaults = new Func<JsonSerializerOptions>(() =>
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            //Add Converters that will help provide more relaxed parsing (similar to Newtonsoft.Json)...
            var converters = options.Converters;
            converters.Add(new JsonRelaxedDateTimeConverter());
            converters.Add(new JsonRelaxedDateTimeOffsetConverter());
            converters.Add(new JsonRelaxedBooleanConverter());
            converters.Add(new JsonStringEnumConverter());
            //converters.Add(new JsonRelaxedNullableIntConverter());
            converters.Add(new RelaxedNullableConverterFactory());

            return options;
        }).Invoke();
    }
}
