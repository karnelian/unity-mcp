using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace KarnelLabs.MCP
{
    public static class UIMaskHandler
    {
        public static void Register()
        {
            CommandRouter.Register("uiMask.addMask", AddMask);
            CommandRouter.Register("uiMask.addRectMask2D", AddRectMask2D);
            CommandRouter.Register("uiMask.set", Set);
        }

        private static object AddMask(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add Mask");
            var mask = Undo.AddComponent<Mask>(go);

            if (p["showMaskGraphic"] != null) mask.showMaskGraphic = (bool)p["showMaskGraphic"];

            // Ensure Image component exists
            if (go.GetComponent<Image>() == null)
                Undo.AddComponent<Image>(go);

            return new { success = true, gameObject = go.name, component = "Mask" };
        }

        private static object AddRectMask2D(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add RectMask2D");
            var mask = Undo.AddComponent<RectMask2D>(go);

            if (p["softness"] != null)
                mask.softness = new Vector2Int((int)(float)p["softness"]["x"], (int)(float)p["softness"]["y"]);
            if (p["padding"] != null)
                mask.padding = new Vector4((float)p["padding"]["x"], (float)p["padding"]["y"], (float)p["padding"]["z"], (float)p["padding"]["w"]);

            return new { success = true, gameObject = go.name, component = "RectMask2D" };
        }

        private static object Set(JToken p)
        {
            var go = GameObjectFinder.Find(p);

            var mask = go.GetComponent<Mask>();
            if (mask != null)
            {
                Undo.RecordObject(mask, "Set Mask");
                if (p["showMaskGraphic"] != null) mask.showMaskGraphic = (bool)p["showMaskGraphic"];
                EditorUtility.SetDirty(mask);
                return new { success = true, gameObject = go.name, maskType = "Mask" };
            }

            var rectMask = go.GetComponent<RectMask2D>();
            if (rectMask != null)
            {
                Undo.RecordObject(rectMask, "Set RectMask2D");
                if (p["softness"] != null)
                    rectMask.softness = new Vector2Int((int)(float)p["softness"]["x"], (int)(float)p["softness"]["y"]);
                EditorUtility.SetDirty(rectMask);
                return new { success = true, gameObject = go.name, maskType = "RectMask2D" };
            }

            throw new McpException(-32000, $"No Mask or RectMask2D on '{go.name}'");
        }
    }
}
