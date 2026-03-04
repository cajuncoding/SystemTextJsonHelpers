namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class SystemTextJsonObjectTests
    {
        public record BooleanStringTest(
            bool TestTrueValue1,
            bool TestTrueValue2,
            bool TestTrueValue3,
            bool TestFalseValue1,
            bool TestFalseValue2,
            bool TestFalseValue3
        );

        [TestMethod]
        public void TestRelaxedBooleanConverters()
        {
            var booleanTest = @"{
                ""testTrueValue1"": true,
                ""testTrueValue2"": ""TRuE"",
                ""testTrueValue3"": ""true"",
                ""testFalseValue1"": false,
                ""testFalseValue1"": ""FALsE"",
                ""testFalseValue1"": ""false""
            }".FromJsonTo<BooleanStringTest>();

            Assert.IsNotNull(booleanTest);
            Assert.IsTrue(booleanTest.TestTrueValue1);
            Assert.IsTrue(booleanTest.TestTrueValue2);
            Assert.IsTrue(booleanTest.TestTrueValue3);
            Assert.IsFalse(booleanTest.TestFalseValue1);
            Assert.IsFalse(booleanTest.TestFalseValue2);
            Assert.IsFalse(booleanTest.TestFalseValue3);
        }
    }
}
