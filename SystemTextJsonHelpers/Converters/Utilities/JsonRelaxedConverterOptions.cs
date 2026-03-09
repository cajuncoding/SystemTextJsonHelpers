using System.Globalization;
using System.Text.Json;

namespace SystemTextJsonHelpers.Converters.Utilities
{
    public class JsonRelaxedConverterOptions
    {
        public const string DefaultEnumFlagsStringOutputSeparator = ", ";

        public static JsonRelaxedConverterOptions Default { get; } = new JsonRelaxedConverterOptions();

        public JsonRelaxedConverterOptions(
            bool allowNumericEnumValues = true,
            EnumWriteStyle enumJsonWriteStyle = EnumWriteStyle.StringOutput,
            JsonNamingPolicy? enumNamingPolicy = null,
            bool allowNumericParsingThousandsSeparators = true,
            string numberFormatString = "G", // General format specifier => '123456789.0123456789'
            string enumFlagsStringOutputSeparator = DefaultEnumFlagsStringOutputSeparator,
            string dateTimeFormatString = "O", // ISO 8601 Round-trip ("O"/"o") => '2024-07-16T14:33:12.4570000-05:00'
            string dateTimeOffsetFormatString = "O", // ISO 8601 Round-trip ("O"/"o") => '2024-07-16T14:33:12.4570000-05:00'
            string dateOnlyFormatString = "O", // ISO 8601 Date ("O"/"o") => '2024-07-16'
            string timeOnlyFormatString = "O", // ISO 8601 Time ("O"/"o") => '14:33:12.4570000'
            string timeSpanFormatString = "c", // TimeSpan => '[-][d.]hh:mm:ss.fffffff',
            CultureInfo? cultureInfo = null
        )
        {
            AllowNumericEnumValues = allowNumericEnumValues;
            EnumJsonWriteStyle = enumJsonWriteStyle;
            EnumNamingPolicy = enumNamingPolicy;
            EnumFlagsStringOutputSeparator = enumFlagsStringOutputSeparator;
            AllowNumericParsingThousandsSeparators = allowNumericParsingThousandsSeparators;
            NumberFormatString = numberFormatString;
            DateTimeFormatString = dateTimeFormatString;
            DateTimeOffsetFormatString = dateTimeOffsetFormatString;
            DateOnlyFormatString = dateOnlyFormatString;
            TimeOnlyFormatString = timeOnlyFormatString;
            TimeSpanFormatString = timeSpanFormatString;
            //Default to Invariant for consistent parsing/formatting behavior regardless of the
            //  environment the code is running in, but allow overriding if desired.
            CultureInfo = cultureInfo ?? CultureInfo.InvariantCulture;
        }

        public bool AllowNumericEnumValues { get; }
        public EnumWriteStyle EnumJsonWriteStyle { get; }
        public JsonNamingPolicy? EnumNamingPolicy { get; }
        public bool AllowNumericParsingThousandsSeparators { get; }
        public string NumberFormatString { get; }
        public string EnumFlagsStringOutputSeparator { get; }
        public string DateTimeFormatString { get; }
        public string DateTimeOffsetFormatString { get; }
        public string DateOnlyFormatString { get; }
        public string TimeOnlyFormatString { get; }
        public string TimeSpanFormatString { get; }
        public CultureInfo CultureInfo { get; }
    }
}
