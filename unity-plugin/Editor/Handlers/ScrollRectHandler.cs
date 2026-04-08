using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace KarnelLabs.MCP
{
    public static class ScrollRectHandler
    {
        public static void Register()
        {
            CommandRouter.Register("scrollRect.create", Create);
            CommandRouter.Register("scrollRect.set", Set);
            CommandRouter.Register("scrollRect.getInfo", GetInfo);
            CommandRouter.Register("scrollRect.find", Find);
        }

        private static object Create(JToken p)
        {
            string goName = (string)p["name"] ?? "ScrollView";
            var go = new GameObject(goName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create ScrollRect");

            if (p["parent"] != null)
            {
                var parent = GameObjectFinder.FindByName((string)p["parent"]);
                go.transform.SetParent(parent.transform, false);
            }

            var sr = go.AddComponent<ScrollRect>();
            var img = go.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.39f);
            go.AddComponent<Mask>().showMaskGraphic = false;

            // Create content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(go.transform, false);
            var contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = new Vector2(0, 300);
            sr.content = contentRT;

            if (p["horizontal"] != null) sr.horizontal = (bool)p["horizontal"];
            if (p["vertical"] != null) sr.vertical = (bool)p["vertical"];
            if (p["movementType"] != null)
            {
                sr.movementType = (string)p["movementType"] switch
                {
                    "Unrestricted" => ScrollRect.MovementType.Unrestricted,
                    "Elastic" => ScrollRect.MovementType.Elastic,
                    "Clamped" => ScrollRect.MovementType.Clamped,
                    _ => sr.movementType
                };
            }
            if (p["elasticity"] != null) sr.elasticity = (float)p["elasticity"];
            if (p["inertia"] != null) sr.inertia = (bool)p["inertia"];
            if (p["decelerationRate"] != null) sr.decelerationRate = (float)p["decelerationRate"];
            if (p["scrollSensitivity"] != null) sr.scrollSensitivity = (float)p["scrollSensitivity"];

            return new { success = true, gameObject = go.name, instanceId = go.GetInstanceID() };
        }

        private static object Set(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var sr = go.GetComponent<ScrollRect>();
            if (sr == null) throw new McpException(-32000, $"No ScrollRect on '{go.name}'");

            Undo.RecordObject(sr, "Set ScrollRect");
            if (p["horizontal"] != null) sr.horizontal = (bool)p["horizontal"];
            if (p["vertical"] != null) sr.vertical = (bool)p["vertical"];
            if (p["movementType"] != null)
            {
                sr.movementType = (string)p["movementType"] switch
                {
                    "Unrestricted" => ScrollRect.MovementType.Unrestricted,
                    "Elastic" => ScrollRect.MovementType.Elastic,
                    "Clamped" => ScrollRect.MovementType.Clamped,
                    _ => sr.movementType
                };
            }
            if (p["elasticity"] != null) sr.elasticity = (float)p["elasticity"];
            if (p["inertia"] != null) sr.inertia = (bool)p["inertia"];
            if (p["decelerationRate"] != null) sr.decelerationRate = (float)p["decelerationRate"];
            if (p["scrollSensitivity"] != null) sr.scrollSensitivity = (float)p["scrollSensitivity"];
            if (p["normalizedPosition"] != null)
                sr.normalizedPosition = new Vector2((float)p["normalizedPosition"]["x"], (float)p["normalizedPosition"]["y"]);

            EditorUtility.SetDirty(sr);
            return new { success = true, gameObject = go.name };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var sr = go.GetComponent<ScrollRect>();
            if (sr == null) throw new McpException(-32000, $"No ScrollRect on '{go.name}'");

            return new
            {
                gameObject = go.name,
                horizontal = sr.horizontal,
                vertical = sr.vertical,
                movementType = sr.movementType.ToString(),
                elasticity = sr.elasticity,
                inertia = sr.inertia,
                decelerationRate = sr.decelerationRate,
                scrollSensitivity = sr.scrollSensitivity,
                normalizedPosition = new { x = sr.normalizedPosition.x, y = sr.normalizedPosition.y },
                hasContent = sr.content != null,
            };
        }

        private static object Find(JToken p)
        {
            var scrollRects = Object.FindObjectsByType<ScrollRect>(FindObjectsSortMode.None);
            var result = scrollRects.Select(sr => new
            {
                gameObject = sr.gameObject.name,
                path = GameObjectFinder.GetPath(sr.gameObject),
                horizontal = sr.horizontal,
                vertical = sr.vertical,
            }).ToArray();

            return new { count = result.Length, scrollRects = result };
        }
    }
}
