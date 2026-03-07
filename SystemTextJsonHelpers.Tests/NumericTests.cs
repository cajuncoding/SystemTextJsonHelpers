using System;

namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class NumericTests
    {
        public TestContext TestContext { get; set; }

        public record IntegralNumericTest(
            byte TinyInt,
            byte TinyIntString,
            byte? TinyIntNullable,
            byte? TinyIntNullableString,
            byte? TinyIntNullableNull,
            byte? TinyIntNullableEmptyString,
            byte? TinyIntNullableWhiteSpaceString,
            byte? TinyIntNullableInvalidString,
            byte? TinyIntNullableTooBigtring,
            int Integer,
            int IntegerString,
            int? IntegerNullable,
            int? IntegerNullableString,
            int? IntegerNullableNull,
            int? IntegerNullableEmptyString,
            int? IntegerNullableWhiteSpaceString,
            int? IntegerNullableInvalidString,
            int? IntegerNullableTooBigString,
            long BigInt,
            long BigIntString,
            long? BigIntNullable,
            long? BigIntNullableString,
            long? BigIntNullableNull,
            long? BigIntNullableEmptyString,
            long? BigIntNullableWhiteSpaceString,
            long? BigIntNullableInvalidString,
            long? BigIntNullableTooBigString
        );

        [TestMethod]
        public void TestRelaxedNullableIntegerConverter()
        {
            //TODO: Add Use Cases for Empty String or Whitespace testing...
            var t = @"{
                ""tinyInt"": 16,
                ""tinyIntString"": ""16"",
                ""tinyIntNullable"": 255,
                ""tinyIntNullableString"": ""255"",
                ""tinyIntNullableNull"": null,
                ""tinyIntNullableEmptyString"" : """",
                ""tinyIntNullableWhiteSpaceString"" : ""     "",
                ""tinyIntNullableInvalidString"": ""1a"",
                ""tinyIntNullableTooBigString"": ""256"",
                ""integer"": 123456,
                ""integerString"": ""123456"",
                ""integerNullable"": 2147483647,
                ""integerNullableString"": ""2,147,483,647"",
                ""integerNullableNull"": null,
                ""integerNullableEmptyString"" : """",
                ""integerNullableWhiteSpaceString"" : ""     "",
                ""integerNullableInvalidString"": ""12abe456"",
                ""integerNullableTooBigString"": ""2,147,483,648"",
                ""bigInt"": 123456789012,
                ""bigIntString"": ""123456789012"",
                ""bigIntNullable"": 9223372036854775807,
                ""bigIntNullableString"": ""9223372036854775807"",
                ""bigIntNullableNull"": null,
                ""bigIntNullableEmptyString"" : """",
                ""bigIntNullableWhiteSpaceString"" : ""                        "",
                ""bigIntNullableInvalidString"": ""12542abe412"",
                ""bigIntIntNullableTooBigString"": ""9,223,372,036,854,775,808""
            }".FromJsonTo<IntegralNumericTest>();

            Assert.IsNotNull(t);

            //Tiny Int
            Assert.AreEqual(16, t.TinyInt);
            Assert.AreEqual(16, t.TinyIntString);
            Assert.AreEqual(byte.MaxValue, t.TinyIntNullable);
            Assert.AreEqual(byte.MaxValue, t.TinyIntNullableString);
            Assert.IsNull(t.TinyIntNullableNull);
            Assert.IsNull(t.TinyIntNullableEmptyString);
            Assert.IsNull(t.TinyIntNullableWhiteSpaceString);
            Assert.IsNull(t.TinyIntNullableInvalidString);
            Assert.IsNull(t.TinyIntNullableTooBigtring);

            //Int
            Assert.AreEqual(123456, t.Integer);
            Assert.AreEqual(123456, t.IntegerString);
            Assert.AreEqual(int.MaxValue, t.IntegerNullable);
            Assert.AreEqual(int.MaxValue, t.IntegerNullableString);
            Assert.IsNull(t.IntegerNullableNull);
            Assert.IsNull(t.IntegerNullableEmptyString);
            Assert.IsNull(t.IntegerNullableWhiteSpaceString);
            Assert.IsNull(t.IntegerNullableInvalidString);
            Assert.IsNull(t.IntegerNullableTooBigString);

            //BigInt
            Assert.AreEqual(123456789012, t.BigInt);
            Assert.AreEqual(123456789012, t.BigIntString);
            Assert.AreEqual(long.MaxValue, t.BigIntNullable);
            Assert.AreEqual(long.MaxValue, t.BigIntNullableString);
            Assert.IsNull(t.BigIntNullableNull);
            Assert.IsNull(t.BigIntNullableEmptyString);
            Assert.IsNull(t.BigIntNullableWhiteSpaceString);
            Assert.IsNull(t.BigIntNullableInvalidString);
            Assert.IsNull(t.BigIntNullableTooBigString);

        }

        public record RationalNumericTest(
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
            }".FromJsonTo<RationalNumericTest>();

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
    }
}
