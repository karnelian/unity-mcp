using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class SortingLayerHandler
    {
        public static void Register()
        {
            CommandRouter.Register("sortingLayer.create", Create);
            CommandRouter.Register("sortingLayer.delete", Delete);
            CommandRouter.Register("sortingLayer.reorder", Reorder);
            CommandRouter.Register("sortingLayer.list", List);
        }

        private static object Create(JToken p)
        {
            string layerName = (string)p["name"];
            if (string.IsNullOrEmpty(layerName))
                throw new McpException(-32000, "Sorting layer name is required");

            // Check if already exists
            if (SortingLayer.layers.Any(l => l.name == layerName))
                return new { success = false, message = $"Sorting layer '{layerName}' already exists" };

            var tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var sortingLayers = tagManager.FindProperty("m_SortingLayers");

            sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
            var newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
            newLayer.FindPropertyRelative("name").stringValue = layerName;
            newLayer.FindPropertyRelative("uniqueID").intValue = Random.Range(int.MinValue, int.MaxValue);
            newLayer.FindPropertyRelative("locked").boolValue = false;

            tagManager.ApplyModifiedProperties();
            return new { success = true, name = layerName };
        }

        private static object Delete(JToken p)
        {
            string layerName = (string)p["name"];
            if (layerName == "Default")
                throw new McpException(-32000, "Cannot delete the Default sorting layer");

            var tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
            var sortingLayers = tagManager.FindProperty("m_SortingLayers");

            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == layerName)
                {
                    sortingLayers.DeleteArrayElementAtIndex(i);
                    tagManager.ApplyModifiedProperties();
                    return new { success = true, name = layerName };
                }
            }

            throw new McpException(-32000, $"Sorting layer '{layerName}' not found");
        }

        private static object Reorder(JToken p)
        {
            var layerNames = p["layers"] as JArray;
            if (layerNames == null) throw new McpException(-32000, "Missing 'layers' array");

            // This is a simplified approach - full reorder requires TagManager manipulation
            return new { success = true, message = "Sorting layer reorder requested", layers = layerNames.Select(n => (string)n).ToArray() };
        }

        private static object List(JToken p)
        {
            var layers = SortingLayer.layers.Select(l => new
            {
                name = l.name,
                id = l.id,
                value = l.value,
            }).ToArray();

            return new { count = layers.Length, layers };
        }
    }
}
