using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class OcclusionCullingHandler
    {
        public static void Register()
        {
            CommandRouter.Register("occlusionCulling.bake", Bake);
            CommandRouter.Register("occlusionCulling.clear", Clear);
            CommandRouter.Register("occlusionCulling.getSettings", GetSettings);
            CommandRouter.Register("occlusionCulling.setArea", SetArea);
        }

        private static object Bake(JToken p)
        {
            if (p["smallestOccluder"] != null)
                StaticOcclusionCulling.smallestOccluder = (float)p["smallestOccluder"];
            if (p["smallestHole"] != null)
                StaticOcclusionCulling.smallestHole = (float)p["smallestHole"];
            if (p["backfaceThreshold"] != null)
                StaticOcclusionCulling.backfaceThreshold = (float)p["backfaceThreshold"];

            StaticOcclusionCulling.GenerateInBackground();
            return new { success = true, message = "Occlusion culling bake started" };
        }

        private static object Clear(JToken p)
        {
            StaticOcclusionCulling.Clear();
            return new { success = true, message = "Occlusion culling data cleared" };
        }

        private static object GetSettings(JToken p)
        {
            return new
            {
                smallestOccluder = StaticOcclusionCulling.smallestOccluder,
                smallestHole = StaticOcclusionCulling.smallestHole,
                backfaceThreshold = StaticOcclusionCulling.backfaceThreshold,
                isRunning = StaticOcclusionCulling.isRunning,
                umbraDataSize = StaticOcclusionCulling.umbraDataSize,
            };
        }

        private static object SetArea(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var area = go.GetComponent<OcclusionArea>();

            if (area == null)
            {
                Undo.RecordObject(go, "Add OcclusionArea");
                area = Undo.AddComponent<OcclusionArea>(go);
            }

            Undo.RecordObject(area, "Set OcclusionArea");
            if (p["center"] != null) area.center = JsonHelper.ToVector3(p["center"]);
            if (p["size"] != null) area.size = JsonHelper.ToVector3(p["size"]);

            EditorUtility.SetDirty(area);
            return new { success = true, gameObject = go.name };
        }
    }
}
