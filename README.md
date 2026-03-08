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

