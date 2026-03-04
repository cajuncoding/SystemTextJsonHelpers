namespace SystemTextJsonHelpers
{
    public static class StringCustomExtensions
    {
        public static int? ParseAsNullableInt(this string stringValue)
        {
            // Handle empty strings by returning null
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;
            // Try to parse the string as an integer
            else if (int.TryParse(stringValue, out var result))
                return result;
            // If parsing fails, return null instead of throwing
            else
                return null;
        }

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
    }
}
