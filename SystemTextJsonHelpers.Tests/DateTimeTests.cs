namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class DateTimeTests
    {
        public TestContext TestContext { get; set; }

        public record DateTimeTest(
             DateTime DateAndTime,
             DateTime? DateAndTimeNullableNull,
             DateTime? DateAndTimeNullableInvalid,
             DateTimeOffset DateAndTimeOffset,
             DateTimeOffset? DateAndTimeOffsetNullableNull,
             DateTimeOffset? DateAndTimeOffsetNullableInvalid
         );

        [TestMethod]
        public void TestRelaxedDateTimeConverter()
        {
            var now = DateTime.Now;

            //TODO: Add Use Cases for Empty String or Whitespace testing...
            var t = $@"{{
                ""dateAndTime"": ""{now:O}"",
                ""dateAndTimeNullableNull"": null,
                ""dateAndTimeNullableInvalid"": ""123:abcdef"",
                ""dateAndTimeOffset"": ""{new DateTimeOffset(now):O}"",
                ""dateAndTimeOffsetNullableNull"": null,
                ""dateAndTimeOffsetNullableInvalid"": ""12:99:abcdef"",
            }}".FromJsonTo<DateTimeTest>();

            Assert.IsNotNull(t);
            Assert.AreEqual(now, t.DateAndTime);
            Assert.IsNull(t.DateAndTimeNullableNull);
            Assert.AreEqual(new DateTimeOffset(now), t.DateAndTimeOffset);
            Assert.IsNull(t.DateAndTimeOffsetNullableNull);
        }

        public record DateOnlyTimeOnlyTest(
            DateOnly Date,
            DateOnly? DateNullableNull,
            DateOnly? DateNullableInvalid,
            TimeOnly Time,
            TimeOnly? TimeNullableNull,
            TimeOnly? TimeNullableInvalid
        );

        [TestMethod]
        public void TestRelaxedDateOnlyTimeOnlyConverters()
        {
            var date = new DateOnly(2025, 12, 31);
            var time = new TimeOnly(23, 59, 58, 123);

            var t = $@"{{
                ""date"": ""{date:O}"",
                ""dateNullableNull"": null,
                ""dateNullableInvalid"": ""abc"",
                ""time"": ""{time:O}"",
                ""timeNullableNull"": null,
                ""timeNullableInvalid"": ""25:61:99""
            }}".FromJsonTo<DateOnlyTimeOnlyTest>();

            Assert.AreEqual(date, t.Date);
            Assert.IsNull(t.DateNullableNull);
            Assert.IsNull(t.DateNullableInvalid);
            Assert.AreEqual(time, t.Time);
            Assert.IsNull(t.TimeNullableNull);
            Assert.IsNull(t.TimeNullableInvalid);
        } 
    }
}
