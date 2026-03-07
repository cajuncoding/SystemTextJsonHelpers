namespace SystemTextJsonHelpers.Converters.Utilities
{
    internal class JsonTypeCache
    {
        public static readonly Type NullableOpenType = typeof(Nullable<>);
        public static readonly Type BoolType = typeof(bool);
        public static readonly Type GuidType = typeof(Guid);
        public static readonly Type DateTimeType = typeof(DateTime);
        public static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);
        public static readonly Type TimeSpanType = typeof(TimeSpan);
        public static readonly Type DateOnlyType = typeof(DateOnly);
        public static readonly Type TimeOnlyType = typeof(TimeOnly);

        public static readonly Type LongType = typeof(long);
        public static readonly Type IntType = typeof(int);
        public static readonly Type ShortType = typeof(short);
        public static readonly Type SByteType = typeof(sbyte);

        public static readonly Type ULongType = typeof(ulong);
        public static readonly Type UIntType = typeof(uint);
        public static readonly Type UShortType = typeof(ushort);
        public static readonly Type ByteType = typeof(byte);

        public static readonly Type FloatType = typeof(float);
        public static readonly Type DoubleType = typeof(double);
        public static readonly Type DecimalType = typeof(decimal);
    }
}
