using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SystemTextJsonHelpers.Converters.Utilities;

namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public sealed class EnumTests
    {
        public TestContext TestContext { get; set; }

        public enum Color
        {
            Red,
            [JsonPropertyName("green-color")]
            Green,
            [JsonPropertyName("MY-BLUE")]
            [EnumMember(Value = "blue-color")]
            Blue
        }
        public enum Status
        {
            Draft = 1,
            Staged = 2,
            PendingReview = 3,
            Approved = 4
        }

        //Test Flags support!
        [Flags]
        public enum AccessRights
        {
            [EnumMember(Value = "none")]
            None = 0,

            //Test Combined/aggregate flags with an Alias to ensure they are preferred over their individual parts when writing...
            [EnumMember(Value = "read-and-write-and-exec")]
            ReadWriteExecute = Read | Write | Execute,

            [EnumMember(Value = "read-access")]
            Read = 1,
            [JsonPropertyName("write-access")]
            Write = 2,

            // No alias on Execute to test JsonNamingPolicy application
            Execute = 4,
            [EnumMember(Value = "delete-access")]
            Delete = 8,
            //Test Combined/aggregate flags to ensure they are preferred over their individual parts when writing...
            ReadAndWrite = Read | Write
        }


        public record EnumTest(
            Color Color,
            Color ColorCaseInsensitive,
            Color ColorByEnumMemberAnnotation,
            Color ColorByJsonPropertyAnnotation,
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
                ""colorByEnumMemberAnnotation"": ""blue-color"",
                ""colorByJsonPropertyAnnotation"": ""green-color"",
                ""colorNullable"": ""Blue"",
                ""colorNullableCaseInsensitive"": ""rEd"",
                ""colorNullableNull"": null,
                ""colorNullableInvalid"": ""NotAColor""
            }".FromJsonTo<EnumTest>();

            Assert.AreEqual(Color.Red, t.Color);
            Assert.AreEqual(Color.Green, t.ColorCaseInsensitive);
            Assert.AreEqual(Color.Blue, t.ColorByEnumMemberAnnotation);
            Assert.AreEqual(Color.Green, t.ColorByJsonPropertyAnnotation);
            Assert.AreEqual(Color.Blue, t.ColorNullable);
            Assert.AreEqual(Color.Red, t.ColorNullableCaseInsensitive);
            Assert.IsNull(t.ColorNullableNull);
            Assert.IsNull(t.ColorNullableInvalid);
        }

        [TestMethod]
        public void TestEnumReadingAndWritingWithNamingPolicy()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                enumNamingPolicy: JsonNamingPolicy.KebabCaseUpper
            );

            // Write: should use camelCase for non-aliased names
            var json = Status.PendingReview.ToJson(options);
            StringAssert.Contains(json, "\"PENDING-REVIEW\"");

            // Should NOT be "pendingReview" (camelCase)
            Assert.IsFalse(json.Contains("\"pendingReview\""), "Naming policy output unexpectedly replaced alias.");

            // Read: policy(camelCase) is accepted
            var v1 = " \"PENDING-REVIEW\" ".FromJsonTo<Status?>(options);
            Assert.AreEqual(Status.PendingReview, v1);

            // Read: raw name (case-insensitive) is also accepted
            var v2 = " \"pending-review\" ".FromJsonTo<Status?>(options);
            Assert.AreEqual(Status.PendingReview, v2);

            // Read: original name (case-insensitive) is also accepted; testing with inverted casing...
            var v3 = " \"pENDINGrEVIEW\" ".FromJsonTo<Status?>(options);
            Assert.AreEqual(Status.PendingReview, v3);
        }

        [TestMethod]
        public void TestEnumReadNumberThenWriteNumberWhendNumberStyleEnabled()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                enumJsonWriteStyle: EnumWriteStyle.NumberOutput
            );

            var staged = " 2 ".FromJsonTo<Status?>();
            Assert.AreEqual(Status.Staged, staged);

            var json = staged.ToJson(options);
            StringAssert.Contains(json, "2");
        }


        [TestMethod]
        public void TestNullableEnumAliasCaseInsensitiveAndNullValue()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                allowNumericEnums: true
            );

            var c1 = @"""MY-BLUE""".FromJsonTo<Color?>(options);
            Assert.AreEqual(Color.Blue, c1);

            var json = c1.ToJson(options);
            StringAssert.Contains(json, "\"MY-BLUE\"");

            var c2 = @" null ".FromJsonTo<Color?>(options);
            Assert.IsNull(c2);
        }

        [TestMethod]
        public void TestEnumAliasOverridesNamingPolicyOnWrite()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                enumNamingPolicy: JsonNamingPolicy.CamelCase
            );

            var json = Color.Blue.ToJson(options);

            // Should NOT be "blue" (camelCase)
            Assert.IsFalse(json.Contains("\"blue\""), "Naming policy output unexpectedly replaced alias.");

            // Should be the alias
            StringAssert.Contains(json, "\"MY-BLUE\"");
        }

        [TestMethod]
        public void TestFlagsReadAliasesNamesAndWriteString()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                allowNumericEnums: true,
                enumJsonWriteStyle: EnumWriteStyle.StringOutput,
                enumNamingPolicy: JsonNamingPolicy.KebabCaseUpper // Execute (no alias) -> "EXECUTE"
            );

            // Read: mixed aliases/names, mixed separators and casing
            var rights = @"""read-access, WRITE-ACCESS | execute""".FromJsonTo<AccessRights?>(options);
            Assert.AreEqual(AccessRights.Read | AccessRights.Write | AccessRights.Execute, rights);

            // Write: aliases preferred ("read-access", "write-access"), policy applied for Execute -> "EXECUTE"
            var json = rights.ToJson(options);
            StringAssert.Contains(json, "read-and-write-and-exec");

            // Read: mixed aliases/names, mixed separators and casing
            var rights2 = @"""read-access, DELETE-ACCESS | execute""".FromJsonTo<AccessRights?>(options);
            Assert.AreEqual(AccessRights.Read | AccessRights.Execute | AccessRights.Delete, rights2);

            // Write: aliases preferred ("read-access", "write-access"), policy applied for Execute -> "EXECUTE"
            var json2 = rights2.ToJson(options);
            StringAssert.Contains(json2, "read-access");
            StringAssert.Contains(json2, "delete-access");
            StringAssert.Contains(json2, "EXECUTE");
        }

        [TestMethod]
        public void TestFlagsUnknownBitsFallbackToNumeric()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                allowNumericEnums: true,
                enumJsonWriteStyle: EnumWriteStyle.StringOutput
            );

            // Unknown single bit -> numeric fallback
            var unknown = ((AccessRights)16).ToJson(options);
            StringAssert.Contains(unknown, "16");

            // Known + unknown -> numeric fallback (1 | 16 = 17)
            var mixed = (AccessRights.Read | (AccessRights)16).ToJson(options);
            StringAssert.Contains(mixed, "17");
        }

        [TestMethod]
        public void TestFlagsZeroWritesPreferredZeroAlias()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                allowNumericEnums: true,
                enumJsonWriteStyle: EnumWriteStyle.StringOutput
            );

            // Zero value should use the defined alias "none"
            var json = AccessRights.None.ToJson(options);
            StringAssert.Contains(json, "\"none\"");
        }

        [TestMethod]
        public void TestFlagsWritingCustomizedSeparator()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                enumFlagsStringOutputSeparator: " :: "
            );

            // Zero value should use the defined alias "none"
            var json = (AccessRights.Read | AccessRights.Write | AccessRights.Delete).ToJson(options);
            StringAssert.Contains(json, "\"ReadAndWrite :: delete-access\"");
        }

        [TestMethod]
        public void TestFlagsReadNumberThenWriteStringWhenStringStyleEnabled()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                allowNumericEnums: true,
                enumJsonWriteStyle: EnumWriteStyle.StringOutput
            );

            // 3 = Read (1) | Write (2)
            var rights = " 3 ".FromJsonTo<AccessRights?>(options);
            Assert.AreEqual(AccessRights.Read | AccessRights.Write, rights);

            // Should write aliases as a single string
            var json = rights.ToJson(options);
            StringAssert.Contains(json, "\"ReadAndWrite\"");
        }

        [TestMethod]
        public void TestFlagsReadAndWritePrefersCombinedNameOnWriteNoPolicy()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                enumJsonWriteStyle: EnumWriteStyle.StringOutput
            );

            var json = AccessRights.ReadAndWrite.ToJson(options);

            // Expect the single combined member token, not the decomposed flags
            StringAssert.Contains(json, "\"ReadAndWrite\"");
            Assert.IsFalse(json.Contains("read-access"), "Should not decompose to alias parts.");
            Assert.IsFalse(json.Contains("write-access"), "Should not decompose to alias parts.");
        }

        [TestMethod]
        public void FlagsReadAndWritePrefersCombinedNameOnWriteWithPolicy()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                enumNamingPolicy: JsonNamingPolicy.CamelCase,
                enumJsonWriteStyle: EnumWriteStyle.StringOutput
            );

            var json = AccessRights.ReadAndWrite.ToJson(options);

            // Combined member should be policy-transformed (camelCase) and preferred over decomposed aliases
            StringAssert.Contains(json, "\"readAndWrite\"");
            Assert.IsFalse(json.Contains("read-access"), "Should not decompose when a combined member exists.");
            Assert.IsFalse(json.Contains("write-access"), "Should not decompose when a combined member exists.");
        }

        [TestMethod]
        public void TestFlagsCompositeWithAliasIsPreferredOverConstituentsOnWrite()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                allowNumericEnums: true,
                enumJsonWriteStyle: EnumWriteStyle.StringOutput,
                enumNamingPolicy: JsonNamingPolicy.KebabCaseUpper // will affect non-aliased names like Execute
            );

            var json = AccessRights.ReadWriteExecute.ToJson(options);

            // Combined member should use Alias preferred over decomposed aliases
            StringAssert.Contains(json, "\"read-and-write-and-exec\"");
            Assert.IsFalse(json.Contains("read-access"), "Should not decompose when a combined member exists.");
            Assert.IsFalse(json.Contains("write-access"), "Should not decompose when a combined member exists.");
            Assert.IsFalse(json.Contains("execute"), "Should not decompose when a combined member exists.");
        }

        [TestMethod]
        public void TestFlagsReadAndWriteDeserializesFromCombinedOrDecomposedOrNumeric()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                allowNumericEnums: true
            );

            // Combined name (exact)
            var v1 = " \"ReadAndWrite\" ".FromJsonTo<AccessRights?>(options);
            Assert.AreEqual(AccessRights.ReadAndWrite, v1);

            // Combined name (case-insensitive)
            var v2 = " \"readandwrite\" ".FromJsonTo<AccessRights?>(options);
            Assert.AreEqual(AccessRights.ReadAndWrite, v2);

            // Decomposed: aliases
            var v3 = " \"read-access,write-access\" ".FromJsonTo<AccessRights?>(options);
            Assert.AreEqual(AccessRights.ReadAndWrite, v3);

            // Decomposed: names with pipe
            var v4 = " \"Read | Write\" ".FromJsonTo<AccessRights?>(options);
            Assert.AreEqual(AccessRights.ReadAndWrite, v4);

            // Numeric composite
            var v5 = " 3 ".FromJsonTo<AccessRights?>(options); // 1 | 2
            Assert.AreEqual(AccessRights.ReadAndWrite, v5);
        }

        [TestMethod]
        public void TestFlagsReadAndWriteWriteNumericWhenNumericStyleSelected()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                allowNumericEnums: true,
                enumJsonWriteStyle: EnumWriteStyle.NumberOutput
            );

            var json = AccessRights.ReadAndWrite.ToJson(options);

            // 1 | 2 = 3
            StringAssert.Contains(json, "3");
            Assert.IsFalse(json.Contains("ReadAndWrite"), "Numeric style should not output the combined name.");
        }

        [TestMethod]
        public void TestFlagsReadAndWritePolicyTransformedTokenIsAcceptedOnRead()
        {
            var options = SystemTextJsonDefaults.CreateRelaxedJsonSerializerOptions(
                enumNamingPolicy: JsonNamingPolicy.KebabCaseUpper
            );

            // "ReadAndWrite" -> "READ-AND-WRITE" under KebabCaseUpper
            var v = " \"READ-AND-WRITE\" ".FromJsonTo<AccessRights?>(options);
            Assert.AreEqual(AccessRights.ReadAndWrite, v);
        }
    }
}
