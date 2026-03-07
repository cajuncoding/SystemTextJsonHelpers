namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class GuidAndUriTests
    {
        public TestContext TestContext { get; set; }

         public record GuidAndUriTest(
            Guid Guid,
            Guid? GuidNullableNull,
            Guid? GuidNullableInvalid,
            //NOTE: URIs are already HIGHLY relaxed by .Net's built-in Uri type which allows for a virtually unlimited range
            //      of valid and invalid URIs to be parsed without throwing exceptions (e.g. relative URIs, missing schemes, etc.)
            //      so we don't need to add any additional relaxed parsing logic for them like we do for Guids and Enums.
            //      But we include a few tests here just to ensure that the built-in relaxed parsing behavior of Uri is
            //      working as expected when deserializing from Json.
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
            }}".FromJsonTo<GuidAndUriTest>();

            Assert.AreEqual(guid, t.Guid);
            Assert.IsNull(t.GuidNullableNull);
            Assert.IsNull(t.GuidNullableInvalid);
            Assert.AreEqual("https://example.com/path?q=1", t.Uri.OriginalString);
            Assert.IsTrue(t.Uri.IsAbsoluteUri);
            Assert.IsNull(t.UriNullableNull);
            Assert.IsNotNull(t.UriNullableInvalidButStillOk);
            Assert.IsFalse(t.UriNullableInvalidButStillOk.IsAbsoluteUri);
        }
    }
}
