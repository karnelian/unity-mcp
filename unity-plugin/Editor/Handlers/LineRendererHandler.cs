using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class LineRendererHandler
    {
        public static void Register()
        {
            CommandRouter.Register("lineRenderer.add", Add);
            CommandRouter.Register("lineRenderer.setPositions", SetPositions);
            CommandRouter.Register("lineRenderer.setProperties", SetProperties);
            CommandRouter.Register("lineRenderer.getInfo", GetInfo);
            CommandRouter.Register("lineRenderer.addTrail", AddTrail);
            CommandRouter.Register("lineRenderer.setTrailProperties", SetTrailProperties);
            CommandRouter.Register("lineRenderer.find", Find);
        }

        private static object Add(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add LineRenderer");
            var lr = Undo.AddComponent<LineRenderer>(go);

            if (p["positions"] is JArray positions)
            {
                lr.positionCount = positions.Count;
                for (int i = 0; i < positions.Count; i++)
                    lr.SetPosition(i, JsonHelper.ToVector3(positions[i]));
            }
            if (p["startWidth"] != null) lr.startWidth = (float)p["startWidth"];
            if (p["endWidth"] != null) lr.endWidth = (float)p["endWidth"];
            if (p["startColor"] != null) lr.startColor = JsonHelper.ToColor(p["startColor"]);
            if (p["endColor"] != null) lr.endColor = JsonHelper.ToColor(p["endColor"]);
            if (p["materialPath"] != null)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>((string)p["materialPath"]);
                if (mat != null) lr.material = mat;
            }
            if (p["useWorldSpace"] != null) lr.useWorldSpace = (bool)p["useWorldSpace"];
            if (p["loop"] != null) lr.loop = (bool)p["loop"];

            return new { success = true, gameObject = go.name, component = "LineRenderer" };
        }

        private static object SetPositions(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var lr = go.GetComponent<LineRenderer>();
            if (lr == null) throw new McpException(-32000, $"No LineRenderer on '{go.name}'");

            Undo.RecordObject(lr, "Set LineRenderer Positions");
            var positions = p["positions"] as JArray;
            if (positions == null) throw new McpException(-32000, "Missing 'positions' array");

            lr.positionCount = positions.Count;
            for (int i = 0; i < positions.Count; i++)
                lr.SetPosition(i, JsonHelper.ToVector3(positions[i]));

            EditorUtility.SetDirty(lr);
            return new { success = true, gameObject = go.name, positionCount = positions.Count };
        }

        private static object SetProperties(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var lr = go.GetComponent<LineRenderer>();
            if (lr == null) throw new McpException(-32000, $"No LineRenderer on '{go.name}'");

            Undo.RecordObject(lr, "Set LineRenderer Properties");
            if (p["startWidth"] != null) lr.startWidth = (float)p["startWidth"];
            if (p["endWidth"] != null) lr.endWidth = (float)p["endWidth"];
            if (p["startColor"] != null) lr.startColor = JsonHelper.ToColor(p["startColor"]);
            if (p["endColor"] != null) lr.endColor = JsonHelper.ToColor(p["endColor"]);
            if (p["materialPath"] != null)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>((string)p["materialPath"]);
                if (mat != null) lr.material = mat;
            }
            if (p["useWorldSpace"] != null) lr.useWorldSpace = (bool)p["useWorldSpace"];
            if (p["loop"] != null) lr.loop = (bool)p["loop"];
            if (p["numCornerVertices"] != null) lr.numCornerVertices = (int)p["numCornerVertices"];
            if (p["numCapVertices"] != null) lr.numCapVertices = (int)p["numCapVertices"];
            if (p["textureMode"] != null)
            {
                lr.textureMode = (string)p["textureMode"] switch
                {
                    "Stretch" => LineTextureMode.Stretch,
                    "Tile" => LineTextureMode.Tile,
                    "DistributePerSegment" => LineTextureMode.DistributePerSegment,
                    "RepeatPerSegment" => LineTextureMode.RepeatPerSegment,
                    _ => lr.textureMode
                };
            }
            if (p["shadowCastingMode"] != null)
            {
                lr.shadowCastingMode = (string)p["shadowCastingMode"] switch
                {
                    "Off" => UnityEngine.Rendering.ShadowCastingMode.Off,
                    "On" => UnityEngine.Rendering.ShadowCastingMode.On,
                    "TwoSided" => UnityEngine.Rendering.ShadowCastingMode.TwoSided,
                    "ShadowsOnly" => UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly,
                    _ => lr.shadowCastingMode
                };
            }

            EditorUtility.SetDirty(lr);
            return new { success = true, gameObject = go.name };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var lr = go.GetComponent<LineRenderer>();
            if (lr == null) throw new McpException(-32000, $"No LineRenderer on '{go.name}'");

            var positions = new Vector3[lr.positionCount];
            lr.GetPositions(positions);

            return new
            {
                gameObject = go.name,
                positionCount = lr.positionCount,
                positions = positions.Select(v => new { x = v.x, y = v.y, z = v.z }).ToArray(),
                startWidth = lr.startWidth,
                endWidth = lr.endWidth,
                startColor = new { r = lr.startColor.r, g = lr.startColor.g, b = lr.startColor.b, a = lr.startColor.a },
                endColor = new { r = lr.endColor.r, g = lr.endColor.g, b = lr.endColor.b, a = lr.endColor.a },
                useWorldSpace = lr.useWorldSpace,
                loop = lr.loop,
            };
        }

        private static object AddTrail(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add TrailRenderer");
            var tr = Undo.AddComponent<TrailRenderer>(go);

            if (p["time"] != null) tr.time = (float)p["time"];
            if (p["startWidth"] != null) tr.startWidth = (float)p["startWidth"];
            if (p["endWidth"] != null) tr.endWidth = (float)p["endWidth"];
            if (p["startColor"] != null) tr.startColor = JsonHelper.ToColor(p["startColor"]);
            if (p["endColor"] != null) tr.endColor = JsonHelper.ToColor(p["endColor"]);
            if (p["materialPath"] != null)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>((string)p["materialPath"]);
                if (mat != null) tr.material = mat;
            }
            if (p["minVertexDistance"] != null) tr.minVertexDistance = (float)p["minVertexDistance"];

            return new { success = true, gameObject = go.name, component = "TrailRenderer" };
        }

        private static object SetTrailProperties(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var tr = go.GetComponent<TrailRenderer>();
            if (tr == null) throw new McpException(-32000, $"No TrailRenderer on '{go.name}'");

            Undo.RecordObject(tr, "Set TrailRenderer Properties");
            if (p["time"] != null) tr.time = (float)p["time"];
            if (p["startWidth"] != null) tr.startWidth = (float)p["startWidth"];
            if (p["endWidth"] != null) tr.endWidth = (float)p["endWidth"];
            if (p["startColor"] != null) tr.startColor = JsonHelper.ToColor(p["startColor"]);
            if (p["endColor"] != null) tr.endColor = JsonHelper.ToColor(p["endColor"]);
            if (p["minVertexDistance"] != null) tr.minVertexDistance = (float)p["minVertexDistance"];
            if (p["autodestruct"] != null) tr.autodestruct = (bool)p["autodestruct"];
            if (p["emitting"] != null) tr.emitting = (bool)p["emitting"];

            EditorUtility.SetDirty(tr);
            return new { success = true, gameObject = go.name };
        }

        private static object Find(JToken p)
        {
            string type = (string)p?["type"] ?? "All";

            var results = new System.Collections.Generic.List<object>();

            if (type == "Line" || type == "All")
            {
                var lines = Object.FindObjectsByType<LineRenderer>(FindObjectsSortMode.None);
                foreach (var lr in lines)
                    results.Add(new { gameObject = lr.gameObject.name, path = GameObjectFinder.GetPath(lr.gameObject), type = "LineRenderer", positionCount = lr.positionCount });
            }
            if (type == "Trail" || type == "All")
            {
                var trails = Object.FindObjectsByType<TrailRenderer>(FindObjectsSortMode.None);
                foreach (var tr in trails)
                    results.Add(new { gameObject = tr.gameObject.name, path = GameObjectFinder.GetPath(tr.gameObject), type = "TrailRenderer", positionCount = 0 });
            }

            return new { count = results.Count, renderers = results };
        }
    }
}
