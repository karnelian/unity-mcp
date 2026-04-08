using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KarnelLabs.MCP
{
    public static class UIToolkitHandler
    {
        public static void Register()
        {
            CommandRouter.Register("uitoolkit.createUIDocument", CreateUIDocument);
            CommandRouter.Register("uitoolkit.getInfo", GetInfo);
            CommandRouter.Register("uitoolkit.findUIDocuments", FindUIDocuments);
            CommandRouter.Register("uitoolkit.createUXML", CreateUXML);
            CommandRouter.Register("uitoolkit.createUSS", CreateUSS);
            CommandRouter.Register("uitoolkit.setPanelSettings", SetPanelSettings);
            CommandRouter.Register("uitoolkit.createPanelSettings", CreatePanelSettings);
            CommandRouter.Register("uitoolkit.getHierarchy", GetHierarchy);
        }

        private static object CreateUIDocument(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "UIDocument";
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create UIDocument");

            var doc = go.AddComponent<UIDocument>();

            var uxmlPath = p["uxmlPath"]?.Value<string>();
            if (!string.IsNullOrEmpty(uxmlPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                if (asset != null) doc.visualTreeAsset = asset;
                else throw new McpException(-32003, $"UXML not found: {uxmlPath}");
            }

            var panelPath = p["panelSettingsPath"]?.Value<string>();
            if (!string.IsNullOrEmpty(panelPath))
            {
                var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
                if (panel != null) doc.panelSettings = panel;
            }

            if (p["sortingOrder"] != null) doc.sortingOrder = p["sortingOrder"].Value<float>();

            return new
            {
                success = true, name = go.name, instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                hasUXML = doc.visualTreeAsset != null,
                hasPanelSettings = doc.panelSettings != null,
            };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var doc = go.GetComponent<UIDocument>();
            if (doc == null) throw new McpException(-32010, $"No UIDocument on '{go.name}'");

            return new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
                hasUXML = doc.visualTreeAsset != null,
                uxmlName = doc.visualTreeAsset?.name,
                hasPanelSettings = doc.panelSettings != null,
                panelSettingsName = doc.panelSettings?.name,
                sortingOrder = doc.sortingOrder,
                rootElementChildCount = doc.rootVisualElement?.childCount ?? 0,
            };
        }

        private static object FindUIDocuments(JToken p)
        {
            var docs = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            var results = docs.Select(d => new
            {
                name = d.gameObject.name,
                path = GameObjectFinder.GetPath(d.gameObject),
                instanceId = d.gameObject.GetInstanceID(),
                hasUXML = d.visualTreeAsset != null,
                uxmlName = d.visualTreeAsset?.name,
                sortingOrder = d.sortingOrder,
            }).ToArray();
            return new { count = results.Length, documents = results };
        }

        private static object CreateUXML(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "NewUI";
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var safePath = Validate.SafeAssetPath($"{folder}/{name}.uxml", "path");

            var elements = p["elements"] as JArray;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:uie=\"UnityEditor.UIElements\">");

            if (elements != null)
            {
                foreach (var el in elements)
                {
                    var tag = el["tag"]?.Value<string>() ?? "VisualElement";
                    var elName = el["name"]?.Value<string>();
                    var text = el["text"]?.Value<string>();
                    var classes = el["classes"]?.Value<string>();

                    sb.Append($"    <ui:{tag}");
                    if (!string.IsNullOrEmpty(elName)) sb.Append($" name=\"{elName}\"");
                    if (!string.IsNullOrEmpty(text)) sb.Append($" text=\"{text}\"");
                    if (!string.IsNullOrEmpty(classes)) sb.Append($" class=\"{classes}\"");
                    sb.AppendLine(" />");
                }
            }
            else
            {
                sb.AppendLine("    <ui:VisualElement name=\"root\" style=\"flex-grow: 1;\" />");
            }

            sb.AppendLine("</ui:UXML>");

            var dir = Path.GetDirectoryName(safePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(safePath, sb.ToString());
            AssetDatabase.ImportAsset(safePath);
            return new { success = true, path = safePath };
        }

        private static object CreateUSS(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "NewStyle";
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var safePath = Validate.SafeAssetPath($"{folder}/{name}.uss", "path");

            var rules = p["rules"] as JArray;
            var sb = new System.Text.StringBuilder();

            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    var selector = rule["selector"]?.Value<string>() ?? ".default";
                    sb.AppendLine($"{selector} {{");
                    var props = rule["properties"] as JObject;
                    if (props != null)
                    {
                        foreach (var prop in props)
                        {
                            sb.AppendLine($"    {prop.Key}: {prop.Value};");
                        }
                    }
                    sb.AppendLine("}");
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("#root {");
                sb.AppendLine("    flex-grow: 1;");
                sb.AppendLine("}");
            }

            var dir = Path.GetDirectoryName(safePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(safePath, sb.ToString());
            AssetDatabase.ImportAsset(safePath);
            return new { success = true, path = safePath };
        }

        private static object SetPanelSettings(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var doc = go.GetComponent<UIDocument>();
            if (doc == null) throw new McpException(-32010, $"No UIDocument on '{go.name}'");

            var panelPath = p["panelSettingsPath"]?.Value<string>();
            if (!string.IsNullOrEmpty(panelPath))
            {
                var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
                if (panel == null) throw new McpException(-32003, $"PanelSettings not found: {panelPath}");
                Undo.RecordObject(doc, "Set PanelSettings");
                doc.panelSettings = panel;
            }

            var uxmlPath = p["uxmlPath"]?.Value<string>();
            if (!string.IsNullOrEmpty(uxmlPath))
            {
                var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                if (uxml == null) throw new McpException(-32003, $"UXML not found: {uxmlPath}");
                Undo.RecordObject(doc, "Set UXML");
                doc.visualTreeAsset = uxml;
            }

            if (p["sortingOrder"] != null)
            {
                Undo.RecordObject(doc, "Set SortingOrder");
                doc.sortingOrder = p["sortingOrder"].Value<float>();
            }

            EditorUtility.SetDirty(doc);
            return new { success = true, name = go.name };
        }

        private static object CreatePanelSettings(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "PanelSettings";
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var safePath = Validate.SafeAssetPath($"{folder}/{name}.asset", "path");

            var panel = ScriptableObject.CreateInstance<PanelSettings>();

            if (p["scaleMode"] != null && Enum.TryParse<PanelScaleMode>(p["scaleMode"].Value<string>(), true, out var mode))
                panel.scaleMode = mode;
            if (p["referenceResolutionX"] != null || p["referenceResolutionY"] != null)
                panel.referenceResolution = new Vector2Int(
                    p["referenceResolutionX"]?.Value<int>() ?? 1920,
                    p["referenceResolutionY"]?.Value<int>() ?? 1080);

            var dir = Path.GetDirectoryName(safePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(panel, safePath);
            AssetDatabase.SaveAssets();
            return new { success = true, path = safePath };
        }

        private static object GetHierarchy(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var doc = go.GetComponent<UIDocument>();
            if (doc == null) throw new McpException(-32010, $"No UIDocument on '{go.name}'");

            var root = doc.rootVisualElement;
            if (root == null) return new { name = go.name, hierarchy = (object)null };

            return new
            {
                name = go.name,
                hierarchy = BuildElementInfo(root, 0, 3),
            };
        }

        private static object BuildElementInfo(VisualElement el, int depth, int maxDepth)
        {
            var children = new List<object>();
            if (depth < maxDepth)
            {
                foreach (var child in el.Children())
                {
                    children.Add(BuildElementInfo(child, depth + 1, maxDepth));
                }
            }

            return new
            {
                type = el.GetType().Name,
                name = el.name,
                classes = el.GetClasses().ToArray(),
                childCount = el.childCount,
                children = children.Count > 0 ? children.ToArray() : null,
            };
        }
    }
}
