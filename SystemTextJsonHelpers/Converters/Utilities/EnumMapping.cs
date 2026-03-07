using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemTextJsonHelpers.Converters.Utilities
{
    internal sealed class EnumMapping
    {
        private const ulong EmptyFlag = 0UL;

        // Cache per (enumType, namingPolicy) — policy compared by reference
        private static readonly ConcurrentDictionary<(Type enumType, JsonNamingPolicy? policy), Lazy<EnumMapping>> _mappingCache = new();

        private readonly Dictionary<string, object> _readMap; // name/alias(+policy) -> boxed enum (ignore-case)
        private readonly Dictionary<object, string> _writeMap; // boxed enum -> preferred string (alias or policy(name))

        private readonly object[] _enumValues;   // boxed enum values (sorted with masks)
        private readonly ulong[] _enumMasks;  // masks aligned to _definedValues
        private readonly ulong _allKnownBits;
        private readonly string _zeroPreferredName;

        public Type EnumType { get; }
        public JsonNamingPolicy? NamingPolicy { get; }
        public bool IsFlags { get; }


        private EnumMapping(Type enumType, JsonNamingPolicy? namingPolicy)
        {
            EnumType = enumType;
            NamingPolicy = namingPolicy;

            _readMap = new(StringComparer.OrdinalIgnoreCase);
            _writeMap = new();

            //Flags Support
            this.IsFlags = enumType.GetCustomAttribute<FlagsAttribute>() is not null;
            var enumValuesList = new List<object>();
            var enumMasksList = new List<ulong>();

            foreach (var f in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var name = f.Name;
                var enumValue = f.GetValue(null)!;

                enumValuesList.Add(enumValue);
                enumMasksList.Add(Convert.ToUInt64(enumValue));

                // Gather supported annotations (preserve declaration order)
                var annotationAliases = f.GetCustomAttributes(inherit: true)
                    .Select(a => a switch
                    {
                        EnumMemberAttribute enumMemberAttr => enumMemberAttr.Value,
                        JsonPropertyNameAttribute jsonPropAttr => jsonPropAttr.Name,
                        _ => null
                    })
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToImmutableArray();

                if (annotationAliases.Length == 0)
                {
                    //READ: accept original raw name
                    _readMap[name] = enumValue;
                    
                    //READ: accept policy(name) alias if different...
                    if(namingPolicy?.ConvertName(name) is string transformedName && !transformedName.Equals(name, StringComparison.Ordinal))
                        _readMap[transformedName] = enumValue;

                    //WRITE: prefer policy(name) if specified, otherwise original name
                    _writeMap[enumValue] = namingPolicy?.ConvertName(name) ?? name;
                }
                else
                {
                    //READ: all aliases are accepted as-is (policy is NOT applied to aliases)
                    foreach (var alias in annotationAliases)
                        _readMap[alias!] = enumValue;

                    //WRITE: prefer the FIRST declared alias exactly as defined
                    _writeMap[enumValue] = annotationAliases[0]!;
                }
            }

            _enumMasks = enumMasksList.ToArray();
            _enumValues = enumValuesList.ToArray();
            //SORT the Masks and Values together by Mask value ascending so that we can format flags from Large->Small
            //  and ensure we can deterministically process any potential combined/aggregate flags as priority over after their parts/components
            //  (e.g. "ReadWrite" should be used over "Read" and "Write")...
            Array.Sort(_enumMasks, _enumValues);

            _allKnownBits = EmptyFlag;
            foreach (var m in _enumMasks) _allKnownBits |= m;

            var preferredZeroNameIndex = Array.IndexOf(_enumMasks, EmptyFlag);
            _zeroPreferredName = preferredZeroNameIndex >= 0
                && _writeMap.TryGetValue(_enumValues[preferredZeroNameIndex], out var preferredZeroName)
                && !string.IsNullOrEmpty(preferredZeroName)
                ? preferredZeroName
                : "0";
        }

        public static EnumMapping FromCache(Type enumType, JsonNamingPolicy? namingPolicy)
            => _mappingCache.GetOrAdd((enumType, namingPolicy), key =>
                    new Lazy<EnumMapping>(() => new EnumMapping(key.enumType, key.policy))
               ).Value;

        public bool TryGetValue(string? text, out object? enumValue)
        {
            enumValue = text switch
            {
                _ when string.IsNullOrWhiteSpace(text) => null,
                _ when _readMap.TryGetValue(text!, out enumValue!) => enumValue,
                _ when Enum.TryParse(EnumType, text, ignoreCase: true, out var parsedEnum) => parsedEnum,
                _ => null
            };
            return enumValue is not null;
        }

        // If you kept TryGetValue(int?) previously, keep the wider versions you added for numerics:
        public object? GetValue(long value) => Enum.ToObject(EnumType, value);
        public object? GetValue(ulong value) => Enum.ToObject(EnumType, value);

        public string GetPreferredName(object enumValue) => _writeMap.TryGetValue(enumValue, out var preferredName) 
            ? preferredName 
            : enumValue?.ToString() ?? throw new ArgumentNullException(nameof(enumValue));

        //public bool TryParseFlagsFromString(string text, out object? enumValue, string? flagsSeparator = null)
        //{
        //    enumValue = null;
        //    if (!IsFlags) return false;

        //    ulong aggregateEnum = EmptyFlag;
        //    foreach (var enumPart in text.Split(_flagSeparatorChars, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        //    {
        //        if (_readMap.TryGetValue(enumPart, out var partBoxed))
        //            aggregateEnum |= Convert.ToUInt64(partBoxed);
        //        else if (Enum.TryParse(EnumType, enumPart, ignoreCase: true, out var parsedPart))
        //            aggregateEnum |= Convert.ToUInt64(parsedPart);
        //        else //READ FAIL - unknown token encountered
        //            return false;
        //    }

        //    enumValue = Enum.ToObject(EnumType, aggregateEnum);
        //    return true;
        //}

        /// <summary>
        /// To support custom configuration of Enum separator, as well as default values such as ',', & '|' we have to manually parse
        /// but we can still optimize by avoiding most allocations (e.g. for the split and trim operations), and only allocating for the
        /// final token if it is valid (vs. doing multiple allocations per token for split/trim).
        /// </summary>
        public bool TryParseFlags(string text, out object? enumValue, string? flagsSeparator = null)
        {
            enumValue = null;
            if (!IsFlags) return false;

            ulong aggregateEnum = EmptyFlag;

            ReadOnlySpan<char> span = text.AsSpan();
            ReadOnlySpan<char> commaSep = ",".AsSpan();
            ReadOnlySpan<char> pipeSep = "|".AsSpan();
            ReadOnlySpan<char> customSep = flagsSeparator is { Length: > 0 } ? flagsSeparator.AsSpan() : ReadOnlySpan<char>.Empty;

            static int ComputeRelativePostion(int pos, int nextIndex) => nextIndex < 0 ? -1 : pos + nextIndex;
            static int FindNearestSeparatorIndex(int a, int b) => a < 0 ? b : (b < 0 ? a : Math.Min(a, b));

            int i = 0;
            while (i <= span.Length)
            {
                var slice = span[i..];

                //Find next occurrence for each separator relative to current position
                int nextCustomSepIndex = customSep.IsEmpty ? -1 : slice.IndexOf(customSep);
                int nextCommaIndex = slice.IndexOf(commaSep);
                int nextPipeIndex = slice.IndexOf(pipeSep);

                //Convert to absolute positions
                int customSepPosition = ComputeRelativePostion(i, nextCustomSepIndex);
                int commaPosition = ComputeRelativePostion(i, nextCommaIndex);
                int pipePosition = ComputeRelativePostion(i, nextPipeIndex);

                //Nearest next separator among custom / ',' / '|'
                int nextSeparatorIndex = FindNearestSeparatorIndex(customSepPosition, FindNearestSeparatorIndex(commaPosition, pipePosition));

                //token => TrimEntries behavior; skip empty tokens (RemoveEmptyEntries)
                ReadOnlySpan<char> token = nextSeparatorIndex >= 0 
                    ? span.Slice(i, nextSeparatorIndex - i).Trim()
                    : span[i..].Trim();

                if (!token.IsEmpty)
                {
                    var enumPart = token.ToString(); //One allocation per non-empty token
                    if (_readMap.TryGetValue(enumPart, out var partBoxed))
                        aggregateEnum |= Convert.ToUInt64(partBoxed);
                    else if (Enum.TryParse(EnumType, enumPart, ignoreCase: true, out var parsedPart))
                        aggregateEnum |= Convert.ToUInt64(parsedPart);
                    else
                        return false; //PARSE FAIL: unknown fragment
                }

                if (nextSeparatorIndex < 0) break;

                //Advance past whichever separator matched (respect custom length)
                i = nextSeparatorIndex switch
                {
                    var pos when !customSep.IsEmpty && pos == customSepPosition => pos + customSep.Length,
                    var pos when pos == commaPosition => pos + commaSep.Length,
                    var pos when pos == pipePosition => pos + pipeSep.Length,
                    _ => span.Length //Should not happen, but just in case, move to end to exit loop
                };
            }

            enumValue = Enum.ToObject(EnumType, aggregateEnum);
            return true;
        }

        public bool TryFormatFlags(object enumValue, out string? formattedResult, string separator = JsonRelaxedConverterOptions.DefaultEnumFlagsStringOutputSeparator)
        {
            formattedResult = null;
            if (!IsFlags) return false;

            var inputFlags = Convert.ToUInt64(enumValue);
            if (inputFlags == EmptyFlag)
            {
                formattedResult = _zeroPreferredName;
            }
            else
            {
                var remainingParts = inputFlags;
                var parts = new List<string>();

                for (int i = _enumMasks.Length - 1; i >= 0 && remainingParts != EmptyFlag; i--)
                {
                    var mask = _enumMasks[i];
                    if (mask != EmptyFlag && (remainingParts & mask) == mask)
                    {
                        var maskValue = _enumValues.GetValue(i)!;
                        if (_writeMap.TryGetValue(maskValue, out var name)) parts.Add(name);
                        remainingParts &= ~mask;
                    }
                }

                //READ FAIL: Unknown bits are present
                if (remainingParts != EmptyFlag || (inputFlags & ~_allKnownBits) != EmptyFlag)
                    return false;

                //Format Bits Small->Large based on Flag values (not critical but nice for readability)...
                parts.Reverse();
                formattedResult = string.Join(separator, parts);
            }
            
            return formattedResult is not null;
        }
    }
}