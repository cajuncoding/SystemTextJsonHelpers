namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class SystemTextJsonConverterTests
    {
        public TestContext TestContext { get; set; }

        public record BooleanStringTest(
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
            var booleanTest = @"{
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
            }".FromJsonTo<BooleanStringTest>();

            Assert.IsNotNull(booleanTest);
            Assert.IsTrue(booleanTest.BooleanTrue);
            Assert.IsTrue(booleanTest.BooleanTrueString);
            Assert.IsTrue(booleanTest.BooleanTrueCaseInsensitive);
            Assert.IsTrue(booleanTest.BooleanTrueNullable);
            Assert.IsTrue(booleanTest.BooleanTrueNullableString);
            Assert.IsTrue(booleanTest.BooleanTrueNullableCaseInsensitive);
            Assert.IsNull(booleanTest.BooleanTrueNullableNull);

            Assert.IsFalse(booleanTest.BooleanFalse);
            Assert.IsFalse(booleanTest.BooleanFalseString);
            Assert.IsFalse(booleanTest.BooleanFalseCaseInsensitive);
            Assert.IsFalse(booleanTest.BooleanFalseNullable);
            Assert.IsFalse(booleanTest.BooleanFalseNullableString);
            Assert.IsFalse(booleanTest.BooleanFalseNullableCaseInsensitive);
            Assert.IsNull(booleanTest.BooleanFalseNullableNull);
        }

        public record IntegerStringTest(
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
        public void TestRelaxedNullableIntConverter()
        {
            //TODO: Add Use Cases for Empty String or Whitespace testing...
            var integerTest = @"{
                ""integer"": 123,
                ""integerString"": ""123"",
                ""integerNullable"": 4567,
                ""integerNullableString"": ""4567"",
                ""integerNullableNull"": null,
                ""integerNullableEmptyString"" : """",
                ""integerNullableWhiteSpaceString"" : ""     "",
                ""integerNullableInvalidString"": ""12abe4""
            }".FromJsonTo<IntegerStringTest>();

            Assert.IsNotNull(integerTest);
            Assert.AreEqual(123, integerTest.Integer);
            Assert.AreEqual(123, integerTest.IntegerString);
            Assert.AreEqual(4567, integerTest.IntegerNullable);
            Assert.AreEqual(4567, integerTest.IntegerNullableString);
            Assert.IsNull(integerTest.IntegerNullableNull);
            Assert.IsNull(integerTest.IntegerNullableEmptyString);
            Assert.IsNull(integerTest.IntegerNullableWhiteSpaceString);
            Assert.IsNull(integerTest.IntegerNullableInvalidString);
        }

        [TestMethod]
        public void TestRelaxedNullableNumberConverter()
        {
            //TODO: Add Use Cases for Empty String or Whitespace testing...
            var integerTest = @"{
                ""integer"": 123,
                ""integerString"": ""123"",
                ""integerNullable"": 4567,
                ""integerNullableString"": ""4567"",
                ""integerNullableNull"": null,
                ""integerNullableEmptyString"" : """",
                ""integerNullableWhiteSpaceString"" : ""     "",
                ""integerNullableInvalidString"": ""12abe4""
            }".FromJsonTo<IntegerStringTest>();

            Assert.IsNotNull(integerTest);
            Assert.AreEqual(123, integerTest.Integer);
            Assert.AreEqual(123, integerTest.IntegerString);
            Assert.AreEqual(4567, integerTest.IntegerNullable);
            Assert.AreEqual(4567, integerTest.IntegerNullableString);
            Assert.IsNull(integerTest.IntegerNullableNull);
            Assert.IsNull(integerTest.IntegerNullableEmptyString);
            Assert.IsNull(integerTest.IntegerNullableWhiteSpaceString);
            Assert.IsNull(integerTest.IntegerNullableInvalidString);
        }
    }
}
