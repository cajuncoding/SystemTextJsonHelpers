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
            string enumFlagsStringOutputSeparator = DefaultEnumFlagsStringOutputSeparator
        )
        {
            AllowNumericEnumValues = allowNumericEnumValues;
            EnumJsonWriteStyle = enumJsonWriteStyle;
            EnumNamingPolicy = enumNamingPolicy;
            EnumFlagsStringOutputSeparator = enumFlagsStringOutputSeparator;
            AllowNumericParsingThousandsSeparators = allowNumericParsingThousandsSeparators;
        }

        public bool AllowNumericEnumValues { get; }
        public EnumWriteStyle EnumJsonWriteStyle { get; }
        public JsonNamingPolicy? EnumNamingPolicy { get; }
        public bool AllowNumericParsingThousandsSeparators { get; }
        public string EnumFlagsStringOutputSeparator { get; }
    }
}
