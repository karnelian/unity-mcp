using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KarnelLabs.MCP
{
    public static class UIHandler
    {
        public static void Register()
        {
            CommandRouter.Register("ui.createCanvas", CreateCanvas);
            CommandRouter.Register("ui.createPanel", CreatePanel);
            CommandRouter.Register("ui.createButton", CreateButton);
            CommandRouter.Register("ui.createText", CreateText);
            CommandRouter.Register("ui.createImage", CreateImage);
            CommandRouter.Register("ui.createSlider", CreateSlider);
            CommandRouter.Register("ui.createToggle", CreateToggle);
            CommandRouter.Register("ui.createDropdown", CreateDropdown);
            CommandRouter.Register("ui.createInputField", CreateInputField);
            CommandRouter.Register("ui.setRectTransform", SetRectTransform);
            CommandRouter.Register("ui.setText", SetText);
            CommandRouter.Register("ui.setImage", SetImage);
            CommandRouter.Register("ui.setButton", SetButton);
            CommandRouter.Register("ui.setSlider", SetSlider);
            CommandRouter.Register("ui.setToggle", SetToggle);
            CommandRouter.Register("ui.addLayout", AddLayout);
            CommandRouter.Register("ui.findUI", FindUI);
            CommandRouter.Register("ui.setCanvasProperties", SetCanvasProperties);
            CommandRouter.Register("ui.click", ClickUI);
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                Undo.RegisterCreatedObjectUndo(esGo, "MCP: Create EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }
        }

        private static Canvas FindOrCreateCanvas(JToken p)
        {
            var parentPath = p?["parent"]?.Value<string>();
            if (!string.IsNullOrEmpty(parentPath))
            {
                var parentGo = GameObject.Find(parentPath);
                if (parentGo != null)
                {
                    var parentCanvas = parentGo.GetComponentInParent<Canvas>();
                    if (parentCanvas != null) return parentCanvas;
                }
            }

            var existing = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (existing != null) return existing;

            // Auto-create canvas
            var canvasGo = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "MCP: Auto Create Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
            return canvas;
        }

        private static void SetParent(GameObject go, JToken p)
        {
            var parentPath = p?["parent"]?.Value<string>();
            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = GameObject.Find(parentPath);
                if (parent != null) { go.transform.SetParent(parent.transform, false); return; }
            }
            var canvas = FindOrCreateCanvas(p);
            go.transform.SetParent(canvas.transform, false);
        }

        private static object UIInfo(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            return new
            {
                name = go.name,
                instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                anchoredPosition = rt != null ? new { rt.anchoredPosition.x, rt.anchoredPosition.y } : null,
                sizeDelta = rt != null ? new { x = rt.sizeDelta.x, y = rt.sizeDelta.y } : null,
            };
        }

        private static object CreateCanvas(JToken p)
        {
            var canvasName = p["name"]?.Value<string>() ?? "Canvas";
            var renderMode = p["renderMode"]?.Value<string>() ?? "ScreenSpaceOverlay";

            var go = new GameObject(canvasName);
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = Validate.ParseEnum<RenderMode>(renderMode, "renderMode");
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            return UIInfo(go);
        }

        private static object CreatePanel(JToken p)
        {
            var go = new GameObject(p["name"]?.Value<string>() ?? "Panel");
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Panel");
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.4f);
            SetParent(go, p);
            return UIInfo(go);
        }

        private static object CreateButton(JToken p)
        {
            var go = new GameObject(p["name"]?.Value<string>() ?? "Button");
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Button");
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            go.AddComponent<Button>();
            SetParent(go, p);

            // Add text child
            var textGo = new GameObject("Text");
            Undo.RegisterCreatedObjectUndo(textGo, "MCP: Create Button Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<Text>();
            text.text = p["text"]?.Value<string>() ?? "Button";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            return UIInfo(go);
        }

        private static object CreateText(JToken p)
        {
            var go = new GameObject(p["name"]?.Value<string>() ?? "Text");
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Text");
            go.AddComponent<RectTransform>();
            var text = go.AddComponent<Text>();
            text.text = p["text"]?.Value<string>() ?? "New Text";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (p["fontSize"] != null) text.fontSize = p["fontSize"].Value<int>();
            if (p["alignment"] != null) text.alignment = Validate.ParseEnum<TextAnchor>(p["alignment"].Value<string>(), "alignment");
            if (p["color"] != null)
            {
                var c = p["color"];
                text.color = new Color(c["r"]?.Value<float>() ?? 0, c["g"]?.Value<float>() ?? 0, c["b"]?.Value<float>() ?? 0, c["a"]?.Value<float>() ?? 1);
            }
            SetParent(go, p);
            return UIInfo(go);
        }

        private static object CreateImage(JToken p)
        {
            var go = new GameObject(p["name"]?.Value<string>() ?? "Image");
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Image");
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();

            if (p["spritePath"] != null)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p["spritePath"].Value<string>());
                if (sprite != null) img.sprite = sprite;
            }
            if (p["color"] != null)
            {
                var c = p["color"];
                img.color = new Color(c["r"]?.Value<float>() ?? 1, c["g"]?.Value<float>() ?? 1, c["b"]?.Value<float>() ?? 1, c["a"]?.Value<float>() ?? 1);
            }

            SetParent(go, p);
            return UIInfo(go);
        }

        private static object CreateSlider(JToken p)
        {
            var go = DefaultControls.CreateSlider(new DefaultControls.Resources());
            go.name = p["name"]?.Value<string>() ?? "Slider";
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Slider");
            var slider = go.GetComponent<Slider>();
            if (p["minValue"] != null) slider.minValue = p["minValue"].Value<float>();
            if (p["maxValue"] != null) slider.maxValue = p["maxValue"].Value<float>();
            if (p["value"] != null) slider.value = p["value"].Value<float>();
            SetParent(go, p);
            return UIInfo(go);
        }

        private static object CreateToggle(JToken p)
        {
            var go = DefaultControls.CreateToggle(new DefaultControls.Resources());
            go.name = p["name"]?.Value<string>() ?? "Toggle";
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Toggle");
            var toggle = go.GetComponent<Toggle>();
            if (p["isOn"] != null) toggle.isOn = p["isOn"].Value<bool>();
            SetParent(go, p);
            return UIInfo(go);
        }

        private static object CreateDropdown(JToken p)
        {
            var go = DefaultControls.CreateDropdown(new DefaultControls.Resources());
            go.name = p["name"]?.Value<string>() ?? "Dropdown";
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Dropdown");
            var dropdown = go.GetComponent<Dropdown>();
            if (p["options"] != null)
            {
                dropdown.ClearOptions();
                var opts = p["options"].Select(o => new Dropdown.OptionData(o.Value<string>())).ToList();
                dropdown.AddOptions(opts);
            }
            SetParent(go, p);
            return UIInfo(go);
        }

        private static object CreateInputField(JToken p)
        {
            var go = DefaultControls.CreateInputField(new DefaultControls.Resources());
            go.name = p["name"]?.Value<string>() ?? "InputField";
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create InputField");
            var input = go.GetComponent<InputField>();
            if (p["placeholder"] != null)
            {
                var ph = go.transform.Find("Placeholder")?.GetComponent<Text>();
                if (ph != null) ph.text = p["placeholder"].Value<string>();
            }
            if (p["text"] != null) input.text = p["text"].Value<string>();
            SetParent(go, p);
            return UIInfo(go);
        }

        private static object SetRectTransform(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) throw new McpException(-32602, $"No RectTransform on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(rt, "MCP: Set RectTransform");

            if (p["anchoredPosition"] != null)
                rt.anchoredPosition = new Vector2(p["anchoredPosition"]["x"]?.Value<float>() ?? 0, p["anchoredPosition"]["y"]?.Value<float>() ?? 0);
            if (p["sizeDelta"] != null)
                rt.sizeDelta = new Vector2(p["sizeDelta"]["x"]?.Value<float>() ?? 100, p["sizeDelta"]["y"]?.Value<float>() ?? 100);
            if (p["anchorMin"] != null)
                rt.anchorMin = new Vector2(p["anchorMin"]["x"]?.Value<float>() ?? 0, p["anchorMin"]["y"]?.Value<float>() ?? 0);
            if (p["anchorMax"] != null)
                rt.anchorMax = new Vector2(p["anchorMax"]["x"]?.Value<float>() ?? 1, p["anchorMax"]["y"]?.Value<float>() ?? 1);
            if (p["pivot"] != null)
                rt.pivot = new Vector2(p["pivot"]["x"]?.Value<float>() ?? 0.5f, p["pivot"]["y"]?.Value<float>() ?? 0.5f);

            return UIInfo(go);
        }

        private static object SetText(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var text = go.GetComponent<Text>();
            if (text == null) throw new McpException(-32602, $"No Text on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(text, "MCP: Set Text");

            if (p["text"] != null) text.text = p["text"].Value<string>();
            if (p["fontSize"] != null) text.fontSize = p["fontSize"].Value<int>();
            if (p["alignment"] != null) text.alignment = Validate.ParseEnum<TextAnchor>(p["alignment"].Value<string>(), "alignment");
            if (p["color"] != null)
            {
                var c = p["color"];
                text.color = new Color(c["r"]?.Value<float>() ?? 0, c["g"]?.Value<float>() ?? 0, c["b"]?.Value<float>() ?? 0, c["a"]?.Value<float>() ?? 1);
            }
            if (p["fontStyle"] != null) text.fontStyle = Validate.ParseEnum<FontStyle>(p["fontStyle"].Value<string>(), "fontStyle");

            return new { gameObject = go.name, text = text.text, fontSize = text.fontSize };
        }

        private static object SetImage(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var img = go.GetComponent<Image>();
            if (img == null) throw new McpException(-32602, $"No Image on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(img, "MCP: Set Image");

            if (p["spritePath"] != null)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p["spritePath"].Value<string>());
                if (sprite != null) img.sprite = sprite;
            }
            if (p["color"] != null)
            {
                var c = p["color"];
                img.color = new Color(c["r"]?.Value<float>() ?? 1, c["g"]?.Value<float>() ?? 1, c["b"]?.Value<float>() ?? 1, c["a"]?.Value<float>() ?? 1);
            }
            if (p["type"] != null) img.type = Validate.ParseEnum<Image.Type>(p["type"].Value<string>(), "type");
            if (p["fillAmount"] != null) img.fillAmount = p["fillAmount"].Value<float>();

            return new { gameObject = go.name, sprite = img.sprite?.name };
        }

        private static object SetButton(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var btn = go.GetComponent<Button>();
            if (btn == null) throw new McpException(-32602, $"No Button on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(btn, "MCP: Set Button");

            if (p["interactable"] != null) btn.interactable = p["interactable"].Value<bool>();
            if (p["text"] != null)
            {
                var text = go.GetComponentInChildren<Text>();
                if (text != null) { Undo.RecordObject(text, "MCP: Set Button Text"); text.text = p["text"].Value<string>(); }
            }

            return new { gameObject = go.name, interactable = btn.interactable };
        }

        private static object SetSlider(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var slider = go.GetComponent<Slider>();
            if (slider == null) throw new McpException(-32602, $"No Slider on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(slider, "MCP: Set Slider");

            if (p["value"] != null) slider.value = p["value"].Value<float>();
            if (p["minValue"] != null) slider.minValue = p["minValue"].Value<float>();
            if (p["maxValue"] != null) slider.maxValue = p["maxValue"].Value<float>();
            if (p["wholeNumbers"] != null) slider.wholeNumbers = p["wholeNumbers"].Value<bool>();

            return new { gameObject = go.name, value = slider.value, minValue = slider.minValue, maxValue = slider.maxValue };
        }

        private static object SetToggle(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var toggle = go.GetComponent<Toggle>();
            if (toggle == null) throw new McpException(-32602, $"No Toggle on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(toggle, "MCP: Set Toggle");

            if (p["isOn"] != null) toggle.isOn = p["isOn"].Value<bool>();
            if (p["interactable"] != null) toggle.interactable = p["interactable"].Value<bool>();

            return new { gameObject = go.name, isOn = toggle.isOn };
        }

        private static object AddLayout(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var layoutType = Validate.Required<string>(p, "layoutType");

            WorkflowManager.SnapshotObject(go);
            switch (layoutType.ToLower())
            {
                case "horizontal":
                    var hlg = Undo.AddComponent<HorizontalLayoutGroup>(go);
                    if (p["spacing"] != null) hlg.spacing = p["spacing"].Value<float>();
                    if (p["padding"] != null)
                    {
                        var pad = p["padding"];
                        hlg.padding = new RectOffset(
                            pad["left"]?.Value<int>() ?? 0, pad["right"]?.Value<int>() ?? 0,
                            pad["top"]?.Value<int>() ?? 0, pad["bottom"]?.Value<int>() ?? 0);
                    }
                    break;
                case "vertical":
                    var vlg = Undo.AddComponent<VerticalLayoutGroup>(go);
                    if (p["spacing"] != null) vlg.spacing = p["spacing"].Value<float>();
                    if (p["padding"] != null)
                    {
                        var pad = p["padding"];
                        vlg.padding = new RectOffset(
                            pad["left"]?.Value<int>() ?? 0, pad["right"]?.Value<int>() ?? 0,
                            pad["top"]?.Value<int>() ?? 0, pad["bottom"]?.Value<int>() ?? 0);
                    }
                    break;
                case "grid":
                    var glg = Undo.AddComponent<GridLayoutGroup>(go);
                    if (p["cellSize"] != null)
                        glg.cellSize = new Vector2(p["cellSize"]["x"]?.Value<float>() ?? 100, p["cellSize"]["y"]?.Value<float>() ?? 100);
                    if (p["spacing"] != null)
                        glg.spacing = new Vector2(p["spacing"]["x"]?.Value<float>() ?? 0, p["spacing"]["y"]?.Value<float>() ?? 0);
                    break;
                default:
                    throw new McpException(-32602, $"Unknown layout type: {layoutType}. Use: horizontal, vertical, grid");
            }

            return new { gameObject = go.name, layoutType };
        }

        private static object FindUI(JToken p)
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var results = canvases.Select(c => new
            {
                name = c.gameObject.name,
                renderMode = c.renderMode.ToString(),
                childCount = c.transform.childCount,
                instanceId = c.gameObject.GetInstanceID(),
            }).ToArray();
            return new { canvases = results };
        }

        private static object SetCanvasProperties(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var canvas = go.GetComponent<Canvas>();
            if (canvas == null) throw new McpException(-32602, $"No Canvas on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(canvas, "MCP: Set Canvas Properties");

            if (p["renderMode"] != null) canvas.renderMode = Validate.ParseEnum<RenderMode>(p["renderMode"].Value<string>(), "renderMode");
            if (p["sortingOrder"] != null) canvas.sortingOrder = p["sortingOrder"].Value<int>();
            if (p["pixelPerfect"] != null) canvas.pixelPerfect = p["pixelPerfect"].Value<bool>();

            return new { gameObject = go.name, renderMode = canvas.renderMode.ToString(), sortingOrder = canvas.sortingOrder };
        }

        private static object ClickUI(JToken p)
        {
            var path = (string)p["path"];
            var name = (string)p["name"];

            GameObject go = null;

            // Find by path or name — supports inactive via transform.Find
            if (!string.IsNullOrEmpty(path))
            {
                // Try direct Find first (active only)
                go = GameObject.Find(path);
                // Fallback: walk the path via transform.Find from root
                if (go == null)
                {
                    var parts = path.Split('/');
                    var root = GameObject.Find(parts[0]);
                    if (root != null && parts.Length > 1)
                    {
                        var child = root.transform.Find(string.Join("/", parts, 1, parts.Length - 1));
                        if (child != null) go = child.gameObject;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(name))
            {
                var all = Resources.FindObjectsOfTypeAll<GameObject>();
                go = all.FirstOrDefault(g => g.name == name && g.scene.isLoaded);
            }

            if (go == null)
                throw new McpException(-32000, $"UI element not found: {path ?? name}");

            // Get screen position of the target element
            var rectTransform = go.GetComponent<RectTransform>();
            Vector2 screenPos;
            if (rectTransform != null)
            {
                // Convert center of RectTransform to screen coordinates
                var canvas = go.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    screenPos = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
                }
                else
                {
                    var cam = canvas != null ? canvas.worldCamera : Camera.main;
                    screenPos = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);
                }
            }
            else
            {
                screenPos = Camera.main.WorldToScreenPoint(go.transform.position);
            }

            // Raycast through EventSystem to find what's actually at this position
            var eventData = new PointerEventData(EventSystem.current) { position = screenPos };
            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            if (raycastResults.Count == 0)
                return new { success = false, reason = "No UI element at screen position", target = go.name, screenPos = new { x = screenPos.x, y = screenPos.y } };

            // The first raycast result is the topmost UI element
            var hitGo = raycastResults[0].gameObject;
            var isBlocked = !hitGo.transform.IsChildOf(go.transform) && hitGo != go;

            if (isBlocked)
                return new { success = false, reason = "Blocked by another UI element", target = go.name, blockedBy = GetPath(hitGo) };

            // Execute click on the actual hit target (respects UI layering)
            var button = hitGo.GetComponentInParent<Button>();
            if (button != null && (button.gameObject == go || button.gameObject.transform.IsChildOf(go.transform) || go.transform.IsChildOf(button.gameObject.transform)))
            {
                button.onClick.Invoke();
                return new { success = true, type = "Button", gameObject = button.gameObject.name, path = GetPath(button.gameObject) };
            }

            var toggle = hitGo.GetComponentInParent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = !toggle.isOn;
                return new { success = true, type = "Toggle", gameObject = toggle.gameObject.name, isOn = toggle.isOn, path = GetPath(toggle.gameObject) };
            }

            ExecuteEvents.Execute(hitGo, eventData, ExecuteEvents.pointerClickHandler);
            return new { success = true, type = "PointerClick", gameObject = hitGo.name, path = GetPath(hitGo) };
        }

        private static string GetPath(GameObject go)
        {
            var path = go.name;
            var t = go.transform.parent;
            while (t != null)
            {
                path = t.name + "/" + path;
                t = t.parent;
            }
            return path;
        }
    }
}
