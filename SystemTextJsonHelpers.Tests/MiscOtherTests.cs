namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class MiscOtherTests
    {
        public TestContext TestContext { get; set; }

        public record OddTokensTest(
            int? IntegerFromObject,
            int? IntegerFromArray,
            Guid? GuidFromNumber,
            DateTime? DateTimeFromArray
        );

        [TestMethod]
        public void TestOddTokensAreSkippedToNull()
        {
            var t = @"{
                ""integerFromObject"": { ""x"": 1 },
                ""integerFromArray"": [1,2,3],
                ""guidFromNumber"": 12345,
                ""dateTimeFromArray"": [ ""2024-01-01T00:00:00Z"" ]
            }".FromJsonTo<OddTokensTest>();

            Assert.IsNull(t.IntegerFromObject);
            Assert.IsNull(t.IntegerFromArray);
            Assert.IsNull(t.GuidFromNumber);
            Assert.IsNull(t.DateTimeFromArray);
        }

        public record RoundTripTest(
            bool? Bool,
            int? Int,
            long? Long,
            float? Float,
            double? Double,
            decimal? Decimal,
            DateTime? DateTime,
            DateTimeOffset? DateTimeOffset,
            TimeSpan? TimeSpan,
            DateOnly? DateOnly,
            TimeOnly? TimeOnly,
            Guid? Guid,
            Uri? Uri,
            EnumTests.Color? Enum
        );

        [TestMethod]
        public void TestRoundTripSerialization()
        {
            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);
            var t = new RoundTripTest(
                Bool: true,
                Int: 123,
                Long: 9999999999,
                Float: 1.5f,
                Double: 2.5,
                Decimal: 12345.6789m,
                DateTime: now,
                DateTimeOffset: new DateTimeOffset(now),
                TimeSpan: TimeSpan.FromHours(25) + TimeSpan.FromMilliseconds(123),
                DateOnly: new DateOnly(2026, 03, 05),
                TimeOnly: new TimeOnly(23, 59, 59, 123),
                Guid: Guid.NewGuid(),
                Uri: new Uri("https://example.com/a?b=1"),
                Enum: EnumTests.Color.Blue
            );

            var json = t.ToJson();
            var back = json.FromJsonTo<RoundTripTest>();

            Assert.AreEqual(t.Bool, back.Bool);
            Assert.AreEqual(t.Int, back.Int);
            Assert.AreEqual(t.Long, back.Long);
            Assert.AreEqual(t.Float, back.Float, "Float");
            Assert.AreEqual(t.Double, back.Double, "Double");
            Assert.AreEqual(t.Decimal, back.Decimal, "Decimal");
            Assert.AreEqual(t.DateTime, back.DateTime, "DateTime");
            Assert.AreEqual(t.DateTimeOffset, back.DateTimeOffset, "DateTimeOffset");
            Assert.AreEqual(t.TimeSpan, back.TimeSpan, "TimeSpan");
            Assert.AreEqual(t.DateOnly, back.DateOnly, "DateOnly");
            Assert.AreEqual(t.TimeOnly, back.TimeOnly, "TimeOnly");
            Assert.AreEqual(t.Guid, back.Guid, "Guid");
            Assert.AreEqual(t.Uri, back.Uri, "Uri");
            Assert.AreEqual(t.Enum, back.Enum, "Enum");
        }
    }
}
