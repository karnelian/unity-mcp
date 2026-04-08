using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class LODHandler
    {
        public static void Register()
        {
            CommandRouter.Register("lod.add", Add);
            CommandRouter.Register("lod.getInfo", GetInfo);
            CommandRouter.Register("lod.setLevels", SetLevels);
            CommandRouter.Register("lod.setTransition", SetTransition);
            CommandRouter.Register("lod.find", Find);
            CommandRouter.Register("lod.remove", Remove);
        }

        private static object Add(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add LODGroup");
            var lodGroup = Undo.AddComponent<LODGroup>(go);

            if (p["fadeMode"] != null)
            {
                lodGroup.fadeMode = (string)p["fadeMode"] switch
                {
                    "None" => LODFadeMode.None,
                    "CrossFade" => LODFadeMode.CrossFade,
                    "SpeedTree" => LODFadeMode.SpeedTree,
                    _ => LODFadeMode.None
                };
            }

            return new { success = true, gameObject = go.name, component = "LODGroup" };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null) throw new McpException(-32000, $"No LODGroup on '{go.name}'");

            var lods = lodGroup.GetLODs();
            var lodInfo = lods.Select((lod, i) => new
            {
                level = i,
                screenRelativeTransitionHeight = lod.screenRelativeTransitionHeight,
                fadeTransitionWidth = lod.fadeTransitionWidth,
                rendererCount = lod.renderers?.Length ?? 0,
            }).ToArray();

            return new
            {
                gameObject = go.name,
                lodCount = lods.Length,
                fadeMode = lodGroup.fadeMode.ToString(),
                animateCrossFading = lodGroup.animateCrossFading,
                size = lodGroup.size,
                lods = lodInfo
            };
        }

        private static object SetLevels(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null) throw new McpException(-32000, $"No LODGroup on '{go.name}'");

            Undo.RecordObject(lodGroup, "Set LOD Levels");
            var levelsToken = p["levels"] as JArray;
            if (levelsToken == null) throw new McpException(-32000, "Missing 'levels' array");

            var existingLods = lodGroup.GetLODs();
            var newLods = new LOD[levelsToken.Count];

            for (int i = 0; i < levelsToken.Count; i++)
            {
                var lt = levelsToken[i];
                float threshold = lt["screenRelativeTransitionHeight"]?.Value<float>() ?? (1f - (float)(i + 1) / levelsToken.Count);
                float fadeWidth = lt["fadeTransitionWidth"]?.Value<float>() ?? 0f;
                Renderer[] renderers = i < existingLods.Length ? existingLods[i].renderers : new Renderer[0];
                newLods[i] = new LOD(threshold, renderers) { fadeTransitionWidth = fadeWidth };
            }

            lodGroup.SetLODs(newLods);
            EditorUtility.SetDirty(lodGroup);
            return new { success = true, gameObject = go.name, lodCount = newLods.Length };
        }

        private static object SetTransition(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null) throw new McpException(-32000, $"No LODGroup on '{go.name}'");

            Undo.RecordObject(lodGroup, "Set LOD Transition");

            if (p["fadeMode"] != null)
            {
                lodGroup.fadeMode = (string)p["fadeMode"] switch
                {
                    "None" => LODFadeMode.None,
                    "CrossFade" => LODFadeMode.CrossFade,
                    "SpeedTree" => LODFadeMode.SpeedTree,
                    _ => lodGroup.fadeMode
                };
            }
            if (p["animateCrossFading"] != null) lodGroup.animateCrossFading = (bool)p["animateCrossFading"];

            EditorUtility.SetDirty(lodGroup);
            return new { success = true, gameObject = go.name };
        }

        private static object Find(JToken p)
        {
            var lodGroups = Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None);
            var result = lodGroups.Select(lg => new
            {
                gameObject = lg.gameObject.name,
                path = GameObjectFinder.GetPath(lg.gameObject),
                lodCount = lg.lodCount,
                fadeMode = lg.fadeMode.ToString(),
            }).ToArray();

            return new { count = result.Length, lodGroups = result };
        }

        private static object Remove(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup == null) throw new McpException(-32000, $"No LODGroup on '{go.name}'");

            Undo.DestroyObjectImmediate(lodGroup);
            return new { success = true, gameObject = go.name };
        }
    }
}
