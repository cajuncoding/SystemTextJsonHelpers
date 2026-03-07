namespace SystemTextJsonHelpers
{
    public static class StringCustomExtensions
    {
        /// <summary>
        /// Determines if the string appears to be valid Json using 'duck' typing:
        ///     'if it looks like a duck and quacks like a duck then it's probably a duck!'
        /// </summary>
        /// <returns></returns>
        public static bool IsDuckTypedJson(this string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                return false;

            var text = jsonText.Trim();
            return 
                (text.StartsWith("{") && text.EndsWith("}")) //For object
                || (text.StartsWith("[") && text.EndsWith("]")); //For array

        }

        // Single source of truth for all grouping/separator characters we strip.
        private const string NumberFormattingSeparatorCharactersString = ", \u00A0\u202F’'";

        internal static string SanitizeFormattingCharsFromNumberString(this string numberString)
        {
            if (string.IsNullOrEmpty(numberString))
                return numberString;

            ReadOnlySpan<char> span = numberString.AsSpan();
            ReadOnlySpan<char> seps = NumberFormattingSeparatorCharactersString.AsSpan();

            // Fast path: nothing to remove → return original reference (no allocation)
            if (span.IndexOfAny(seps) < 0) return numberString;

            // Copy & filter in a single pass; and small input values will stay on the stack!
            Span<char> buffer = span.Length <= 256
                ? stackalloc char[span.Length]
                : new char[span.Length];

            var count = 0;
            foreach (var c in span)
                if (seps.IndexOf(c) < 0)
                    buffer[count++] = c;

            return new string(buffer[..count]);
        }
    }
}
