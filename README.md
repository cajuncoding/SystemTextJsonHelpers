# SystemTextJsonHelpers
A streamlined ultra-lightweight set of helpful extensions, converters, and global configuration for working with System.Text.Json

## Overview
One key goal this library provides a simple global configuration mechanism to easily apply Json Serializer settings across an entire application anytime
the provided extension methods are used (e.g. obj.ToJson(), jsonText.FromJsonTo&lt;T&gt;()). This allows for consistent application of settings such as
custom converters, naming policies, and other options without having to specify them in each individual serialization or deserialization call.
The library also provides other helpful extensions for common serialization and deserialization scenarios, in particular a support for mering json
which is (surprisingly) missing from System.Text.Json (e.g. jsonNode.Merge(jsonNode)).

Finally, the other big goal of this library was to also provide a a set of relaxed converters to enable support for working with odd/non-strict/non-standard/bad JSON formats
that are often encountered, in the real world, when working with bad exteranl APIs (e.g. Salesforce CloudPage where all-the-things-are-strings) or legacy systems,
without having to write custom converters for each unique case. With the use of nullable types (e.g. int?, DateTime?, etc.) the relaxed converters can return null
on parse failures instead of throwing exceptions. In addition automatic string conversions from various types (e.g int, bool, Enums, etc.) are also supported.

### Nuget Package (>=netstandard2.1)
To use this in your project, add the [SystemTextJsonHelpers](https://www.nuget.org/packages/SystemTextJsonHelpers/) package.

### Give Star 🌟
**If you like this project and/or use it the please give it a Star 🌟 (c'mon it's free, and it'll help others find the project)!**

### [Buy me a Coffee ☕](https://www.buymeacoffee.com/cajuncoding)
*I'm happy to share with the community, but if you find this useful (e.g for professional use), and are so inclinded,
then I do love-me-some-coffee!*

<a href="https://www.buymeacoffee.com/cajuncoding" target="_blank">
<img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174">
</a>

## Release Notes
### v1.3
- Add support for Relaxed Date and Time parsing (when non-nullable) which delegates the the default Parse() method for DateTime, DateTimeOffset, DateOnly, & TimeOnly.
- Added a BaseJsonStringDelegateConverter&lt;T&gt; (inspired by the Macross.Json.Extensions library) for easier conversion of string based values.
- Greatly improved configurable options for string outputs of formatted items like DateTime, DateTimeOffset, Numbers, etc.
- Options now include support for custom format strings, and overriding the culture info (Invariant is default), for better control over the output formatting of these types when serialized to JSON.

### v1.2
- Added support for new custom attributes to enable multi-mapping of Enum values via [JsonStringEnumMemberMultiMap].
- Added support to now explicitly define the primary mapping for output/writing of json via [JsonPrimaryStringEnumMemberMultiMap]; which also serves as a read mapping.
- Added new targeting for .Net 9+ and enabled support for the new [JsonStringEnumMemberName] attribute that is now built-in/default with .Net 9+.

### v1.1
- Added a number of very helpful convenience methods for processing JsonObject, JsonNode, JsonValue.
- These extension methods make it easier to safely retrieve values, enumerate properties, etc. without exceptions
- These convenience methods include support for the Globally configured SystemTextJsonDefaults.DefaultSerializerOptions (e.g. relaxed parsing, custom converters, etc.)
for consistent handling of JSON across the application.

### v1.0
- Initial release of SystemTextJsonHelpers package.
- Supports global configuration of System.Text.Json via `SystemTextJsonDefaults.ConfigureDefaults()` static class configuration method.
- Supports object extensions for common json operations such as `obj.ToJson()` and `text.FromJson&lt;T&gt;()` with relaxed parsing and handling of null values (as configured in `SystemTextJsonDefaults`).
- Supports advanced custom converters for handling of common scenarios such as relaxed parsing of numbers, dates, enums, and more.
- Supports string enum relaxed handling that can handle string values, numeric values, and flags for enums as well as case-insensitive matching, null handling, and annotations (e.g. [EnumMember(Name="")] &amp; [JsonPropertyName("")]).
- Supports relaxed/safe parsing and handling of JSON via nullable types (e.g. int?, bool?, DateTime?, etc.) whereby null is returned on parsing errors instead of throwing exceptions.
