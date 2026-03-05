namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class SystemTextJsonConverterTests
    {
        public TestContext TestContext { get; set; }

        public record BooleanTest(
            bool BooleanTrue,
            bool BooleanTrueString,
            bool BooleanTrueCaseInsensitive,
            bool? BooleanTrueNullable,
            bool? BooleanTrueNullableString,
            bool? BooleanTrueNullableCaseInsensitive,
            bool? BooleanTrueNullableNull,
            bool BooleanFalse,
            bool BooleanFalseString,
            bool BooleanFalseCaseInsensitive,
            bool? BooleanFalseNullable,
            bool? BooleanFalseNullableString,
            bool? BooleanFalseNullableCaseInsensitive,
            bool? BooleanFalseNullableNull
        );

        [TestMethod]
        public void TestRelaxedBooleanConverter()
        {
            var t = @"{
                ""booleanTrue"": true,
                ""booleanTrueString"": ""true"",
                ""booleanTrueCaseInsensitive"": ""TRuE"",
                ""booleanTrueNullable"": true,
                ""booleanTrueNullableString"": ""true"",
                ""booleanTrueNullableCaseInsensitive"": ""TRuE"",
                ""booleanTrueNullableNull"": null,
                ""booleanFalse"": false,
                ""booleanFalseString"": ""false"",
                ""booleanFalseCaseInsensitive"": ""FALsE"",
                ""booleanFalseNullable"": false,
                ""booleanFalseNullableString"": ""false"",
                ""booleanFalseNullableCaseInsensitive"": ""FALsE"",
                ""booleanFalseNullableNull"": null,
            }".FromJsonTo<BooleanTest>();

            Assert.IsNotNull(t);
            Assert.IsTrue(t.BooleanTrue);
            Assert.IsTrue(t.BooleanTrueString);
            Assert.IsTrue(t.BooleanTrueCaseInsensitive);
            Assert.IsTrue(t.BooleanTrueNullable);
            Assert.IsTrue(t.BooleanTrueNullableString);
            Assert.IsTrue(t.BooleanTrueNullableCaseInsensitive);
            Assert.IsNull(t.BooleanTrueNullableNull);

            Assert.IsFalse(t.BooleanFalse);
            Assert.IsFalse(t.BooleanFalseString);
            Assert.IsFalse(t.BooleanFalseCaseInsensitive);
            Assert.IsFalse(t.BooleanFalseNullable);
            Assert.IsFalse(t.BooleanFalseNullableString);
            Assert.IsFalse(t.BooleanFalseNullableCaseInsensitive);
            Assert.IsNull(t.BooleanFalseNullableNull);
        }

        public record IntegerTest(
             int Integer,
             int IntegerString,
             int? IntegerNullable,
             int? IntegerNullableString,
             int? IntegerNullableNull,
             int? IntegerNullableEmptyString,
             int? IntegerNullableWhiteSpaceString,
             int? IntegerNullableInvalidString
         );

        [TestMethod]
        public void TestRelaxedNullableIntegerConverter()
        {
            //TODO: Add Use Cases for Empty String or Whitespace testing...
            var t = @"{
                ""integer"": 123,
                ""integerString"": ""123"",
                ""integerNullable"": 4567,
                ""integerNullableString"": ""4567"",
                ""integerNullableNull"": null,
                ""integerNullableEmptyString"" : """",
                ""integerNullableWhiteSpaceString"" : ""     "",
                ""integerNullableInvalidString"": ""12abe4""
            }".FromJsonTo<IntegerTest>();

            Assert.IsNotNull(t);
            Assert.AreEqual(123, t.Integer);
            Assert.AreEqual(123, t.IntegerString);
            Assert.AreEqual(4567, t.IntegerNullable);
            Assert.AreEqual(4567, t.IntegerNullableString);
            Assert.IsNull(t.IntegerNullableNull);
            Assert.IsNull(t.IntegerNullableEmptyString);
            Assert.IsNull(t.IntegerNullableWhiteSpaceString);
            Assert.IsNull(t.IntegerNullableInvalidString);
        }

        public record NumericMatrixTest(
            float Float,
            float FloatString,
            float? FloatNullableNull,
            double Double,
            double DoubleString,
            decimal Decimal,
            decimal DecimalString,
            uint UInt,
            ulong ULong
        );

        [TestMethod]
        public void TestRelaxedNullableNumberConverterNumericMatrix()
        {
            var t = @"{
                ""float"": 1.5,
                ""floatString"": ""1.5"",
                ""floatNullableNull"": null,
                ""double"": 3.14159,
                ""doubleString"": ""2.71828"",
                ""decimal"": 12345.6789,
                ""decimalString"": ""98765.4321"",
                ""uInt"": 4294967295,
                ""uLong"": 18446744073709551615
            }".FromJsonTo<NumericMatrixTest>();

            Assert.AreEqual(1.5f, t.Float);
            Assert.AreEqual(1.5f, t.FloatString);
            Assert.IsNull(t.FloatNullableNull);
            Assert.AreEqual(3.14159, t.Double, 1e-12);
            Assert.AreEqual(2.71828, t.DoubleString, 1e-12);
            Assert.AreEqual(12345.6789m, t.Decimal);
            Assert.AreEqual(98765.4321m, t.DecimalString);
            Assert.AreEqual(uint.MaxValue, t.UInt);
            Assert.AreEqual(ulong.MaxValue, t.ULong);
        }

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

        public record GuidUriTest(
            Guid Guid,
            Guid? GuidNullableNull,
            Guid? GuidNullableInvalid,
            Uri Uri,
            Uri? UriNullableNull,
            Uri? UriNullableInvalidButStillOk
        );

        [TestMethod]
        public void TestRelaxedGuidUriConverters()
        {
            var guid = Guid.NewGuid();
            var t = $@"{{
                ""guid"": ""{guid:D}"",
                ""guidNullableNull"": null,
                ""guidNullableInvalid"": ""not-a-guid"",
                ""uri"": ""https://example.com/path?q=1"",
                ""uriNullableNull"": null,
                ""uriNullableInvalidButStillOk"": ""::// invalid uri but is still ok by .Net permissive relative uri standards //::""
            }}".FromJsonTo<GuidUriTest>();

            Assert.AreEqual(guid, t.Guid);
            Assert.IsNull(t.GuidNullableNull);
            Assert.IsNull(t.GuidNullableInvalid);
            Assert.AreEqual("https://example.com/path?q=1", t.Uri.OriginalString);
            Assert.IsNull(t.UriNullableNull);
            Assert.IsNotNull(t.UriNullableInvalidButStillOk);
        }

        public enum Color { Red, Green, Blue }

        public record EnumTest(
            Color Color,
            Color ColorCaseInsensitive,
            Color? ColorNullable,
            Color? ColorNullableCaseInsensitive,
            Color? ColorNullableNull,
            Color? ColorNullableInvalid
        );

        [TestMethod]
        public void TestRelaxedEnumConverter()
        {
            var t = @"{
                ""color"": ""Red"",
                ""colorCaseInsensitive"": ""gReEn"",
                ""colorNullable"": ""Blue"",
                ""colorNullableCaseInsensitive"": ""rEd"",
                ""colorNullableNull"": null,
                ""colorNullableInvalid"": ""NotAColor""
            }".FromJsonTo<EnumTest>();

            Assert.AreEqual(Color.Red, t.Color);
            Assert.AreEqual(Color.Green, t.ColorCaseInsensitive);
            Assert.AreEqual(Color.Blue, t.ColorNullable);
            Assert.AreEqual(Color.Red, t.ColorNullableCaseInsensitive);
            Assert.IsNull(t.ColorNullableNull);
            Assert.IsNull(t.ColorNullableInvalid);
        }

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
            Color? Enum
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
                Enum: Color.Blue
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
