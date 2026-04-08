#if UNITY_2D_SPRITESHAPE
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace KarnelLabs.MCP
{
    public static class SpriteShapeHandler
    {
        public static void Register()
        {
            CommandRouter.Register("spriteShape.create", Create);
            CommandRouter.Register("spriteShape.addPoint", AddPoint);
            CommandRouter.Register("spriteShape.setPoint", SetPoint);
            CommandRouter.Register("spriteShape.getInfo", GetInfo);
            CommandRouter.Register("spriteShape.find", Find);
        }

        private static object Create(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add SpriteShapeController");
            var ssc = Undo.AddComponent<SpriteShapeController>(go);

            if (p["profilePath"] != null)
            {
                var profile = AssetDatabase.LoadAssetAtPath<SpriteShape>((string)p["profilePath"]);
                if (profile != null) ssc.spriteShape = profile;
            }
            if (p["fillPixelsPerUnit"] != null) ssc.fillPixelsPerUnit = (float)p["fillPixelsPerUnit"];
            if (p["stretchTiling"] != null) ssc.stretchTiling = (float)p["stretchTiling"];
            if (p["splineDetail"] != null) ssc.splineDetail = (int)p["splineDetail"];
            // adaptiveUV removed in newer SpriteShape versions

            return new { success = true, gameObject = go.name, component = "SpriteShapeController" };
        }

        private static object AddPoint(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var ssc = go.GetComponent<SpriteShapeController>();
            if (ssc == null) throw new McpException(-32000, $"No SpriteShapeController on '{go.name}'");

            Undo.RecordObject(ssc, "Add SpriteShape Point");
            var spline = ssc.spline;
            var pos = JsonHelper.ToVector3(p["position"]);
            int index = p["index"]?.Value<int>() ?? spline.GetPointCount();

            spline.InsertPointAt(index, pos);

            if (p["height"] != null) spline.SetHeight(index, (float)p["height"]);
            if (p["corner"] != null) spline.SetCorner(index, (bool)p["corner"]);
            if (p["spriteIndex"] != null) spline.SetSpriteIndex(index, (int)p["spriteIndex"]);

            EditorUtility.SetDirty(ssc);
            return new { success = true, gameObject = go.name, pointIndex = index, totalPoints = spline.GetPointCount() };
        }

        private static object SetPoint(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var ssc = go.GetComponent<SpriteShapeController>();
            if (ssc == null) throw new McpException(-32000, $"No SpriteShapeController on '{go.name}'");

            Undo.RecordObject(ssc, "Set SpriteShape Point");
            int index = (int)p["index"];
            var spline = ssc.spline;

            if (p["position"] != null) spline.SetPosition(index, JsonHelper.ToVector3(p["position"]));
            if (p["height"] != null) spline.SetHeight(index, (float)p["height"]);
            if (p["spriteIndex"] != null) spline.SetSpriteIndex(index, (int)p["spriteIndex"]);

            EditorUtility.SetDirty(ssc);
            return new { success = true, gameObject = go.name, pointIndex = index };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var ssc = go.GetComponent<SpriteShapeController>();
            if (ssc == null) throw new McpException(-32000, $"No SpriteShapeController on '{go.name}'");

            var spline = ssc.spline;
            var points = Enumerable.Range(0, spline.GetPointCount()).Select(i =>
            {
                var pos = spline.GetPosition(i);
                return new { index = i, x = pos.x, y = pos.y, z = pos.z, height = spline.GetHeight(i) };
            }).ToArray();

            return new
            {
                gameObject = go.name,
                pointCount = spline.GetPointCount(),
                isOpenEnded = spline.isOpenEnded,
                points,
                fillPixelsPerUnit = ssc.fillPixelsPerUnit,
                stretchTiling = ssc.stretchTiling,
            };
        }

        private static object Find(JToken p)
        {
            var controllers = Object.FindObjectsByType<SpriteShapeController>(FindObjectsSortMode.None);
            var result = controllers.Select(ssc => new
            {
                gameObject = ssc.gameObject.name,
                path = GameObjectFinder.GetPath(ssc.gameObject),
                pointCount = ssc.spline.GetPointCount(),
            }).ToArray();

            return new { count = result.Length, spriteShapes = result };
        }
    }
}
#endif
