namespace SystemTextJsonHelpers.Converters.Utilities
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class JsonStringEnumMemberMultiMapAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class JsonPrimaryStringEnumMemberMultiMapAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

}
