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
        // Cache per (enumType, namingPolicy) — policy compared by reference
        private static readonly ConcurrentDictionary<(Type enumType, JsonNamingPolicy? policy), Lazy<EnumMapping>> _mappingCache = new();

        private readonly Dictionary<string, object> _readMap; // name/alias(+policy) -> boxed enum (ignore-case)
        private readonly Dictionary<object, string> _writeMap; // boxed enum -> preferred string (alias or policy(name))

        private readonly object[] _enumValues;   // boxed enum values (sorted with masks)
        private readonly ulong[] _enumMasks;  // masks aligned to _definedValues
        private readonly ulong _allKnownBits;
        private readonly string _zeroPreferredName;
        private static readonly char[] _flagSeparatorChars = [ ',', '|' ];

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

        public bool TryParseFlagsFromString(string text, out object? enumValue)
        {
            enumValue = null;
            if (!IsFlags) return false;

            ulong aggregateEnum = 0UL;
            foreach (var enumPart in text.Split(_flagSeparatorChars, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (_readMap.TryGetValue(enumPart, out var partBoxed))
                    aggregateEnum |= Convert.ToUInt64(partBoxed);
                else if (Enum.TryParse(EnumType, enumPart, ignoreCase: true, out var parsedPart))
                    aggregateEnum |= Convert.ToUInt64(parsedPart);
                else //READ FAIL - unknown token encountered
                    return false;
            }

            enumValue = Enum.ToObject(EnumType, aggregateEnum);
            return true;
        }

        private const ulong EmptyFlag = 0UL;

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