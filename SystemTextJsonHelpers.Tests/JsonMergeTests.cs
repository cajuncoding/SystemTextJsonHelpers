namespace SystemTextJsonHelpers.Tests
{
    [TestClass]
    public class SystemTextJsonMergeExtensionTests
    {
        private enum StarWarsCharacterType
        {
            Civilian,
            Jedi,
            StormTrooper,
            Droid,
            SithLord
        }

        private enum LightsaberColor { Blue, Green, Yellow }

        [TestMethod]
        public void TestSystemTextJsonObjectMerge()
        {
            var jedi = new
            {
                Name = "Luke Skywalker",
                CharacterType = StarWarsCharacterType.Jedi,
                LightsaberCount = -1, //Should get overwritten...
                //Test merging of nested objects...
                DroidFriends = new[]
                {
                    new { Name = "R2D2", CharacterType = StarWarsCharacterType.Droid }
                },
                Enemies = new
                {
                    DarthVader = StarWarsCharacterType.SithLord
                }
            };

            var jediAugmentingData = new
            {
                LightsaberOriginalOwner = "Anikan Skywalker",
                LightsaberCount = 3,
                OriginalLightsaberColor = LightsaberColor.Blue,
                ReplacementLightsaberColor = LightsaberColor.Green,
                FoundLighSaberColor = LightsaberColor.Yellow,
                DroidFriends = new[]
                {
                    new { Name = "C-3PO", CharacterType = StarWarsCharacterType.Droid }
                },
                Enemies = new
                {
                    SenatorPalpatine = StarWarsCharacterType.SithLord
                }
            };

            var jediJson = jedi.ToJsonNode();
            var jediAugmentingDataJson = jediAugmentingData.ToJsonNode();
            var mergedJson = jediJson!.Merge(jediAugmentingDataJson);

            //NOTE: This assumes that the Macross.JsonExtensions StringEnumConverter is used on the Enums...
            Assert.IsNotNull(mergedJson);
            Assert.AreEqual("Jedi", mergedJson["characterType"]?.GetValue<string>());
            Assert.AreEqual("Anikan Skywalker", mergedJson["lightsaberOriginalOwner"]?.GetValue<string>());
            Assert.AreEqual(3, mergedJson["lightsaberCount"]?.GetValue<int>());
            Assert.AreEqual("Blue", mergedJson["originalLightsaberColor"]?.GetValue<string>());
            Assert.AreEqual("Green", mergedJson["replacementLightsaberColor"]?.GetValue<string>());

            Assert.AreEqual(2, mergedJson["droidFriends"]!.AsArray().Count);
            foreach (var droidFriend in mergedJson!["droidFriends"]!.AsArray()!)
                Assert.AreEqual("Droid", droidFriend!["characterType"]!.GetValue<string>());

            Assert.AreEqual(2, mergedJson["enemies"]!.AsObject().ToArray().Length);
            foreach (var enemyProp in mergedJson["enemies"]!.AsObject()!)
                Assert.AreEqual("SithLord", enemyProp.Value!.GetValue<string>());
        }

        [TestMethod]
        public void TestSystemTextJsonObjectMergeMany()
        {
            var one = new
            {
                Name = "One",
                ID = 1
            };

            var two = new
            {
                NickName = "Two",
                ID = 2
            };

            var three = new
            {
                MiddleName = "Three",
                ID = 4
            };

            var four = new
            {
                LastName = "Four",
                ID = 4
            };

            var mergedJson = one.ToJsonNode()!.MergeMany(two.ToJsonNode(), three.ToJsonNode(), four.ToJsonNode());

            one.ToJsonNode().DeepClone();

            //NOTE: This assumes that the Macross.JsonExtensions StringEnumConverter is used on the Enums...
            Assert.IsNotNull(mergedJson);
            Assert.AreEqual("One", mergedJson["name"]?.GetValue<string>());
            Assert.AreEqual("Two", mergedJson["nickName"]?.GetValue<string>());
            Assert.AreEqual("Three", mergedJson["middleName"]?.GetValue<string>());
            Assert.AreEqual("Four", mergedJson["lastName"]?.GetValue<string>());
            Assert.AreEqual(4, mergedJson["id"]?.GetValue<int>());
        }
    }
}