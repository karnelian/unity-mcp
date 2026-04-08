#if UNITY_RENDER_PIPELINES_UNIVERSAL
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace KarnelLabs.MCP
{
    public static class RenderFeatureHandler
    {
        public static void Register()
        {
            CommandRouter.Register("renderFeature.list", List);
            CommandRouter.Register("renderFeature.add", Add);
            CommandRouter.Register("renderFeature.remove", Remove);
            CommandRouter.Register("renderFeature.setActive", SetActive);
        }

        private static UniversalRendererData GetRendererData()
        {
            var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
                throw new McpException(-32000, "URP is not the active render pipeline");

            // Get the default renderer
            var so = new SerializedObject(pipeline);
            var rendererDataList = so.FindProperty("m_RendererDataList");
            if (rendererDataList == null || rendererDataList.arraySize == 0)
                throw new McpException(-32000, "No renderer data found in URP asset");

            var rendererData = rendererDataList.GetArrayElementAtIndex(0).objectReferenceValue as UniversalRendererData;
            if (rendererData == null)
                throw new McpException(-32000, "Cannot access Universal Renderer Data");

            return rendererData;
        }

        private static object List(JToken p)
        {
            var rendererData = GetRendererData();
            var features = rendererData.rendererFeatures.Select((f, i) => new
            {
                index = i,
                name = f != null ? f.name : "null",
                type = f != null ? f.GetType().Name : "null",
                isActive = f != null ? f.isActive : false,
            }).ToArray();

            return new { count = features.Length, features };
        }

        private static object Add(JToken p)
        {
            string featureType = (string)p["featureType"];
            string featureName = (string)p["name"];

            // Find the feature type
            var type = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == featureType && typeof(ScriptableRendererFeature).IsAssignableFrom(t));

            if (type == null)
                throw new McpException(-32000, $"Renderer Feature type '{featureType}' not found");

            var rendererData = GetRendererData();
            Undo.RecordObject(rendererData, "Add Renderer Feature");

            var feature = ScriptableObject.CreateInstance(type) as ScriptableRendererFeature;
            if (!string.IsNullOrEmpty(featureName)) feature.name = featureName;

            AssetDatabase.AddObjectToAsset(feature, rendererData);
            rendererData.rendererFeatures.Add(feature);
            rendererData.SetDirty();
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();

            return new { success = true, featureType, name = feature.name };
        }

        private static object Remove(JToken p)
        {
            var rendererData = GetRendererData();
            string featureName = (string)p["name"];
            int? index = p["index"]?.Value<int>();

            ScriptableRendererFeature feature = null;
            int featureIndex = -1;

            if (index.HasValue)
            {
                featureIndex = index.Value;
                if (featureIndex >= rendererData.rendererFeatures.Count)
                    throw new McpException(-32000, $"Feature index {featureIndex} out of range");
                feature = rendererData.rendererFeatures[featureIndex];
            }
            else if (featureName != null)
            {
                for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
                {
                    if (rendererData.rendererFeatures[i]?.name == featureName)
                    {
                        feature = rendererData.rendererFeatures[i];
                        featureIndex = i;
                        break;
                    }
                }
            }

            if (feature == null)
                throw new McpException(-32000, "Renderer Feature not found");

            Undo.RecordObject(rendererData, "Remove Renderer Feature");
            rendererData.rendererFeatures.RemoveAt(featureIndex);
            Object.DestroyImmediate(feature, true);
            rendererData.SetDirty();
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();

            return new { success = true };
        }

        private static object SetActive(JToken p)
        {
            var rendererData = GetRendererData();
            string featureName = (string)p["name"];
            int? index = p["index"]?.Value<int>();
            bool active = (bool)p["active"];

            ScriptableRendererFeature feature = null;

            if (index.HasValue)
            {
                if (index.Value >= rendererData.rendererFeatures.Count)
                    throw new McpException(-32000, $"Feature index {index.Value} out of range");
                feature = rendererData.rendererFeatures[index.Value];
            }
            else if (featureName != null)
            {
                feature = rendererData.rendererFeatures.FirstOrDefault(f => f?.name == featureName);
            }

            if (feature == null)
                throw new McpException(-32000, "Renderer Feature not found");

            Undo.RecordObject(feature, "Set Renderer Feature Active");
            feature.SetActive(active);
            EditorUtility.SetDirty(feature);
            EditorUtility.SetDirty(rendererData);

            return new { success = true, name = feature.name, active };
        }
    }
}
#endif
