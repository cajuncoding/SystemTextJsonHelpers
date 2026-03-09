using System.Globalization;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers.Converters
{
    public class JsonRelaxedDateTimeConverter(JsonRelaxedConverterOptions? options = null) : BaseJsonStringDelegateConverter<DateTime>(
        (value, options) => DateTime.Parse(value),
        (value, options) => value.ToString(options.DateTimeFormatString, options.CultureInfo),
        options
    );

    public class JsonRelaxedDateTimeOffsetConverter(JsonRelaxedConverterOptions? options = null) : BaseJsonStringDelegateConverter<DateTimeOffset>(
        (value, options) => DateTimeOffset.Parse(value),
        (value, options) => value.ToString(options.DateTimeOffsetFormatString, options.CultureInfo),
        options
    );

    public class JsonRelaxedDateOnlyConverter(JsonRelaxedConverterOptions? options = null) : BaseJsonStringDelegateConverter<DateOnly>(
        (value, options) => DateOnly.Parse(value),
        (value, options) => value.ToString(options.DateOnlyFormatString, options.CultureInfo),
        options
    );

    public class JsonRelaxedTimeOnlyConverter(JsonRelaxedConverterOptions? options = null) : BaseJsonStringDelegateConverter<TimeOnly>(
        (value, options) => TimeOnly.Parse(value),
        (value, options) => value.ToString(options.TimeOnlyFormatString, options.CultureInfo),
        options
    );
}
