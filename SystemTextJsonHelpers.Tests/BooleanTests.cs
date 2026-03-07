namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class BooleanTests
    {
        public TestContext TestContext { get; set; }

        public record BooleanTest(
            bool BooleanTrue,
            bool BooleanTrueString,
            bool BooleanTrueCaseInsensitive,
            bool? BooleanTrueNullable,
            bool? BooleanTrueNullableString,
            bool? BooleanTrueNullableCaseInsensitive,
            bool BooleanFalse,
            bool BooleanFalseString,
            bool BooleanFalseCaseInsensitive,
            bool? BooleanFalseNullable,
            bool? BooleanFalseNullableString,
            bool? BooleanFalseNullableCaseInsensitive,
            bool? BooleanFalseNullableNull,

            bool? BooleanNullableInvalid,
            bool? BooleanNullableWhiteSpace
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
                ""booleanFalse"": false,
                ""booleanFalseString"": ""false"",
                ""booleanFalseCaseInsensitive"": ""FALsE"",
                ""booleanFalseNullable"": false,
                ""booleanFalseNullableString"": ""false"",
                ""booleanFalseNullableCaseInsensitive"": ""FALsE"",
                ""booleanFalseNullableNull"": null,

                ""booleanNullableInvalid"": ""not-a-boolean"",
                ""booleanNullableWhiteSpace"": ""     ""
            }".FromJsonTo<BooleanTest>();

            Assert.IsNotNull(t);
            Assert.IsTrue(t.BooleanTrue);
            Assert.IsTrue(t.BooleanTrueString);
            Assert.IsTrue(t.BooleanTrueCaseInsensitive);
            Assert.IsTrue(t.BooleanTrueNullable);
            Assert.IsTrue(t.BooleanTrueNullableString);
            Assert.IsTrue(t.BooleanTrueNullableCaseInsensitive);
            
            Assert.IsFalse(t.BooleanFalse);
            Assert.IsFalse(t.BooleanFalseString);
            Assert.IsFalse(t.BooleanFalseCaseInsensitive);
            Assert.IsFalse(t.BooleanFalseNullable);
            Assert.IsFalse(t.BooleanFalseNullableString);
            Assert.IsFalse(t.BooleanFalseNullableCaseInsensitive);
            Assert.IsNull(t.BooleanFalseNullableNull);

            Assert.IsNull(t.BooleanNullableInvalid);
            Assert.IsNull(t.BooleanNullableWhiteSpace);
        }
    }
}
