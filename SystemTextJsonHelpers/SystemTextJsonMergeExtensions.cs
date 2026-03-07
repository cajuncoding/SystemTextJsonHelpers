using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SystemTextJsonHelpers
{
    public static class SystemTextJsonMergeExtensions
    {
        /// <summary>
        /// Merges the specified Json Node into the base JsonNode for which this method is called.
        /// It is null safe and can be easily used with null-check & null coalesce operators for fluent calls.
        /// NOTE: JsonNodes are context aware and track their parent relationships therefore to merge the values both JsonNode objects
        ///         specified are mutated. The Base is mutated with new data while the source is mutated to remove reverences to all
        ///         fields so that they can be added to the base. If you need to avoid this you can call DeepClone() prior to merging!
        ///
        /// Source taken directly from the open-source Gist here:
        /// https://gist.github.com/cajuncoding/bf78bdcf790782090d231590cbc2438f
        ///
        /// </summary>
        /// <param name="jsonBase"></param>
        /// <param name="jsonMerge"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static JsonNode? Merge(this JsonNode jsonBase, JsonNode? jsonMerge, bool mergeIfAlreadyExists = true)
        {
            if (jsonBase == null || jsonMerge == null)
                return jsonBase;

            switch (jsonBase)
            {
                case JsonObject jsonBaseObj when jsonMerge is JsonObject jsonMergeObj:
                    {
                        //NOTE: We must materialize the set (e.g. to an Array), and then clear the merge array so the node can then be 
                        //      re-assigned to the target/base Json; clearing the Object seems to be the most efficient approach...
                        var mergeNodesArray = jsonMergeObj.ToArray();
                        jsonMergeObj.Clear();

                        foreach (var prop in mergeNodesArray)
                        {
                            if (mergeIfAlreadyExists || !jsonBaseObj.ContainsKey(prop.Key))
                                jsonBaseObj[prop.Key] = jsonBaseObj[prop.Key] switch
                                {
                                    JsonObject jsonBaseChildObj when prop.Value is JsonObject jsonMergeChildObj => jsonBaseChildObj.Merge(jsonMergeChildObj),
                                    JsonArray jsonBaseChildArray when prop.Value is JsonArray jsonMergeChildArray => jsonBaseChildArray.Merge(jsonMergeChildArray),
                                    _ => prop.Value
                                };
                        }
                        break;
                    }
                case JsonArray jsonBaseArray when jsonMerge is JsonArray jsonMergeArray:
                    {
                        //NOTE: We must materialize the set (e.g. to an Array), and then clear the merge array,
                        //      so they can then be re-assigned to the target/base Json...
                        var mergeNodesArray = jsonMergeArray.ToArray();
                        jsonMergeArray.Clear();
                        foreach (var mergeNode in mergeNodesArray) jsonBaseArray.Add(mergeNode);
                        break;
                    }
                default:
                    throw new ArgumentException(
                        $"The JsonNode type [{jsonBase.GetType().Name}] is incompatible for merging with the target/base " +
                        $"type [{jsonMerge.GetType().Name}]; merge requires the types to be the same."
                    );

            }

            return jsonBase;
        }

        /// <summary>
        /// Mergees all the specified Json Nodes into the base JsonNode for which this method is called.
        /// </summary>
        /// <param name="firstJsonNode"></param>
        /// <param name="mergeJsonNodeParams"></param>
        /// <returns></returns>
        public static JsonNode? MergeMany(this JsonNode firstJsonNode, params JsonNode?[] mergeJsonNodeParams)
        {
            JsonNode? resultJsonNode = null;
            foreach (var mergeJsonNode in mergeJsonNodeParams)
                resultJsonNode = (resultJsonNode ?? firstJsonNode).Merge(mergeJsonNode);

            return resultJsonNode;
        }

        /// <summary>
        /// Merges the specified Dictionary of values into the base JsonNode for which this method is called.
        ///
        /// Source taken directly from the open-source Gist here:
        /// https://gist.github.com/cajuncoding/bf78bdcf790782090d231590cbc2438f
        ///
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="jsonBase"></param>
        /// <param name="dictionary"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JsonNode? MergeDictionary<TKey, TValue>(this JsonNode jsonBase, IDictionary<TKey, TValue> dictionary, JsonSerializerOptions? options = null, bool mergeIfAlreadyExists = true)
            => jsonBase.Merge(dictionary.ToJsonNode(options), mergeIfAlreadyExists);
    }
}
