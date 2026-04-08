using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace KarnelLabs.MCP
{
    public static class TextMeshProHandler
    {
        public static void Register()
        {
            CommandRouter.Register("tmp.createUI", CreateUI);
            CommandRouter.Register("tmp.create3D", Create3D);
            CommandRouter.Register("tmp.setText", SetText);
            CommandRouter.Register("tmp.setStyle", SetStyle);
            CommandRouter.Register("tmp.setFont", SetFont);
            CommandRouter.Register("tmp.getInfo", GetInfo);
            CommandRouter.Register("tmp.find", Find);
            CommandRouter.Register("tmp.findFontAssets", FindFontAssets);
        }

        private static object CreateUI(JToken p)
        {
            string goName = (string)p["name"] ?? "TMP Text";
            var go = new GameObject(goName);
            Undo.RegisterCreatedObjectUndo(go, "Create TMP UI");

            if (p["parent"] != null)
            {
                var parent = GameObjectFinder.FindByName((string)p["parent"]);
                go.transform.SetParent(parent.transform, false);
            }

            var tmp = go.AddComponent<TextMeshProUGUI>();
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 50);

            if (p["text"] != null) tmp.text = (string)p["text"];
            if (p["fontSize"] != null) tmp.fontSize = (float)p["fontSize"];
            if (p["color"] != null) tmp.color = JsonHelper.ToColor(p["color"]);
            if (p["alignment"] != null) tmp.alignment = ParseAlignment((string)p["alignment"]);
            if (p["fontStyle"] != null) tmp.fontStyle = ParseFontStyle((string)p["fontStyle"]);

            return new { success = true, gameObject = go.name, instanceId = go.GetInstanceID() };
        }

        private static object Create3D(JToken p)
        {
            string goName = (string)p["name"] ?? "3D Text";
            var go = new GameObject(goName);
            Undo.RegisterCreatedObjectUndo(go, "Create TMP 3D");

            var tmp = go.AddComponent<TextMeshPro>();
            if (p["text"] != null) tmp.text = (string)p["text"];
            if (p["fontSize"] != null) tmp.fontSize = (float)p["fontSize"];
            if (p["position"] != null) go.transform.position = JsonHelper.ToVector3(p["position"]);
            if (p["color"] != null) tmp.color = JsonHelper.ToColor(p["color"]);

            return new { success = true, gameObject = go.name, instanceId = go.GetInstanceID() };
        }

        private static object SetText(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var tmp = go.GetComponent<TMP_Text>();
            if (tmp == null) throw new McpException(-32000, $"No TMP component on '{go.name}'");

            Undo.RecordObject(tmp, "Set TMP Text");
            tmp.text = (string)p["text"];
            EditorUtility.SetDirty(tmp);

            return new { success = true, gameObject = go.name };
        }

        private static object SetStyle(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var tmp = go.GetComponent<TMP_Text>();
            if (tmp == null) throw new McpException(-32000, $"No TMP component on '{go.name}'");

            Undo.RecordObject(tmp, "Set TMP Style");
            if (p["fontSize"] != null) tmp.fontSize = (float)p["fontSize"];
            if (p["fontStyle"] != null) tmp.fontStyle = ParseFontStyle((string)p["fontStyle"]);
            if (p["alignment"] != null) tmp.alignment = ParseAlignment((string)p["alignment"]);
            if (p["color"] != null) tmp.color = JsonHelper.ToColor(p["color"]);
            if (p["enableWordWrapping"] != null) tmp.enableWordWrapping = (bool)p["enableWordWrapping"];
            if (p["lineSpacing"] != null) tmp.lineSpacing = (float)p["lineSpacing"];
            if (p["characterSpacing"] != null) tmp.characterSpacing = (float)p["characterSpacing"];
            if (p["paragraphSpacing"] != null) tmp.paragraphSpacing = (float)p["paragraphSpacing"];

            EditorUtility.SetDirty(tmp);
            return new { success = true, gameObject = go.name };
        }

        private static object SetFont(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var tmp = go.GetComponent<TMP_Text>();
            if (tmp == null) throw new McpException(-32000, $"No TMP component on '{go.name}'");

            string fontPath = (string)p["fontAssetPath"];
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            if (font == null) throw new McpException(-32000, $"TMP_FontAsset not found at '{fontPath}'");

            Undo.RecordObject(tmp, "Set TMP Font");
            tmp.font = font;

            if (p["materialPresetPath"] != null)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>((string)p["materialPresetPath"]);
                if (mat != null) tmp.fontMaterial = mat;
            }

            EditorUtility.SetDirty(tmp);
            return new { success = true, gameObject = go.name, font = font.name };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var tmp = go.GetComponent<TMP_Text>();
            if (tmp == null) throw new McpException(-32000, $"No TMP component on '{go.name}'");

            return new
            {
                gameObject = go.name,
                type = tmp is TextMeshProUGUI ? "UI" : "3D",
                text = tmp.text,
                fontSize = tmp.fontSize,
                fontStyle = tmp.fontStyle.ToString(),
                alignment = tmp.alignment.ToString(),
                color = new { r = tmp.color.r, g = tmp.color.g, b = tmp.color.b, a = tmp.color.a },
                font = tmp.font != null ? tmp.font.name : null,
                wordWrapping = tmp.enableWordWrapping,
                lineSpacing = tmp.lineSpacing,
                characterSpacing = tmp.characterSpacing,
            };
        }

        private static object Find(JToken p)
        {
            string textFilter = (string)p?["textFilter"];
            var all = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);

            if (!string.IsNullOrEmpty(textFilter))
                all = all.Where(t => t.text != null && t.text.Contains(textFilter)).ToArray();

            var result = all.Select(t => new
            {
                gameObject = t.gameObject.name,
                path = GameObjectFinder.GetPath(t.gameObject),
                type = t is TextMeshProUGUI ? "UI" : "3D",
                text = t.text?.Length > 50 ? t.text.Substring(0, 50) + "..." : t.text,
                fontSize = t.fontSize,
            }).ToArray();

            return new { count = result.Length, texts = result };
        }

        private static object FindFontAssets(JToken p)
        {
            string nameFilter = (string)p?["nameFilter"];
            var guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            var fonts = guids.Select(g =>
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                return new { name = font?.name, path };
            });

            if (!string.IsNullOrEmpty(nameFilter))
                fonts = fonts.Where(f => f.name != null && f.name.Contains(nameFilter, System.StringComparison.OrdinalIgnoreCase));

            var result = fonts.ToArray();
            return new { count = result.Length, fontAssets = result };
        }

        private static TextAlignmentOptions ParseAlignment(string alignment)
        {
            return alignment?.ToLower() switch
            {
                "center" => TextAlignmentOptions.Center,
                "left" => TextAlignmentOptions.Left,
                "right" => TextAlignmentOptions.Right,
                "topleft" => TextAlignmentOptions.TopLeft,
                "topright" => TextAlignmentOptions.TopRight,
                "top" => TextAlignmentOptions.Top,
                "bottomleft" => TextAlignmentOptions.BottomLeft,
                "bottomright" => TextAlignmentOptions.BottomRight,
                "bottom" => TextAlignmentOptions.Bottom,
                "justified" => TextAlignmentOptions.Justified,
                _ => TextAlignmentOptions.TopLeft
            };
        }

        private static FontStyles ParseFontStyle(string style)
        {
            return style?.ToLower() switch
            {
                "bold" => FontStyles.Bold,
                "italic" => FontStyles.Italic,
                "underline" => FontStyles.Underline,
                "strikethrough" => FontStyles.Strikethrough,
                "bolditalic" => FontStyles.Bold | FontStyles.Italic,
                _ => FontStyles.Normal
            };
        }
    }
}
