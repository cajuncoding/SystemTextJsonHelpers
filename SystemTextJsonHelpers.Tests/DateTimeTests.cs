using static System.FormattableString;

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

            Assert.IsNotNull(t);
            Assert.AreEqual(date, t.Date);
            Assert.IsNull(t.DateNullableNull);
            Assert.IsNull(t.DateNullableInvalid);
            Assert.AreEqual(time, t.Time);
            Assert.IsNull(t.TimeNullableNull);
            Assert.IsNull(t.TimeNullableInvalid);
        }

        [TestMethod]
        public void TestRelaxedDateAndTimeNonNullableConverters()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                dateTimeFormatString: "F", //Human readable to test the options!
                dateTimeOffsetFormatString: "R" //Human readable WITH Offset to test the option!
            );

            var dateTime = new DateTime(2025, 12, 01, 7, 45, 25, 0, 0);
            var dateTimeOffset = new DateTimeOffset(2025, 12, 01, 7, 45, 25, 0, DateTimeOffset.Now.Offset);
            var date = new DateOnly(2025, 12, 31);
            var time = new TimeOnly(23, 59, 58, 123);

            var dateTimeJson = dateTime.ToJson(options);
            var dateTimeOffsetJson = dateTimeOffset.ToJson(options);
            var dateJson = date.ToJson(options);
            var timeJson = time.ToJson(options);

            Assert.IsNotNull(dateTimeJson);
            Assert.IsNotNull(dateTimeOffsetJson);
            Assert.IsNotNull(dateJson);
            Assert.IsNotNull(timeJson);

            Assert.AreEqual(Invariant($"\"{dateTime:F}\""), dateTimeJson);
            Assert.AreEqual(Invariant($"\"{dateTimeOffset:R}\""), dateTimeOffsetJson);
            Assert.AreEqual(Invariant($"\"{date:O}\""), dateJson);
            Assert.AreEqual(Invariant($"\"{time:O}\""), timeJson);

            var parsedDateTime = dateTimeJson.FromJsonTo<DateTime>(options);
            var parsedDateTimeOffset = dateTimeOffsetJson.FromJsonTo<DateTimeOffset>(options);
            var parsedDate = dateJson.FromJsonTo<DateOnly>(options);
            var parsedTime = timeJson.FromJsonTo<TimeOnly>(options);

            Assert.AreEqual(dateTime, parsedDateTime);
            Assert.AreEqual(dateTimeOffset, parsedDateTimeOffset);
            Assert.AreEqual(date, parsedDate);
            Assert.AreEqual(time, parsedTime);
        }

        [TestMethod]
        public void TestRelaxedDateAndTimeParsingOfCustomFormat()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions();

            var dateTime = DateTime.Now;
            var dateTimeJson = $@"""{dateTime.ToString("yyyy-MM-dd HH:mm:ss")}"""; //YYYY-MM-DD HH:MM:SS

            var parsedDateTime = dateTimeJson.FromJsonTo<DateTime?>(options);

            Assert.IsNotNull(parsedDateTime);
            Assert.AreEqual($"{dateTime:s}", $"{parsedDateTime:s}");
        }

    }
}
