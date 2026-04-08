using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class SmartHandler
    {
        public static void Register()
        {
            CommandRouter.Register("smart.sceneQuery", SceneQuery);
            CommandRouter.Register("smart.referenceBind", ReferenceBind);
        }

        /// <summary>
        /// SQL-like scene query. Example queries:
        /// "Light.intensity > 1" — find all Lights with intensity greater than 1
        /// "Renderer.enabled == false" — find disabled renderers
        /// "Collider" — find all objects with any Collider
        /// </summary>
        private static object SceneQuery(JToken p)
        {
            string query = Validate.Required<string>(p, "query");
            int limit = (int?)p?["limit"] ?? 100;

            // Parse query: "ComponentType.property operator value" or just "ComponentType"
            string componentName;
            string propertyName = null;
            string op = null;
            string valueStr = null;

            var parts = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var dotSplit = parts[0].Split('.');

            componentName = dotSplit[0];
            if (dotSplit.Length > 1) propertyName = dotSplit[1];
            if (parts.Length >= 3)
            {
                op = parts[1];
                valueStr = string.Join(" ", parts.Skip(2));
            }

            // Find the component type
            Type componentType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    componentType = assembly.GetTypes().FirstOrDefault(t =>
                        t.Name == componentName && typeof(Component).IsAssignableFrom(t));
                    if (componentType != null) break;
                }
                catch { }
            }

            if (componentType == null)
                throw new McpException(-32003, $"Component type not found: {componentName}");

            // Find all instances
            var allComponents = UnityEngine.Object.FindObjectsByType(componentType, FindObjectsSortMode.None);
            var results = new List<object>();

            foreach (Component comp in allComponents)
            {
                if (comp == null) continue;

                // If no property filter, include all
                if (string.IsNullOrEmpty(propertyName))
                {
                    results.Add(new
                    {
                        name = comp.gameObject.name,
                        path = GameObjectFinder.GetPath(comp.gameObject),
                        component = componentName,
                    });
                    if (results.Count >= limit) break;
                    continue;
                }

                // Get property/field value
                object value = null;
                var propInfo = componentType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (propInfo != null)
                {
                    try { value = propInfo.GetValue(comp); } catch { continue; }
                }
                else
                {
                    var fieldInfo = componentType.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        try { value = fieldInfo.GetValue(comp); } catch { continue; }
                    }
                    else continue;
                }

                // If no operator, include if property exists
                if (string.IsNullOrEmpty(op))
                {
                    results.Add(new
                    {
                        name = comp.gameObject.name,
                        path = GameObjectFinder.GetPath(comp.gameObject),
                        component = componentName,
                        property = propertyName,
                        value = value?.ToString(),
                    });
                    if (results.Count >= limit) break;
                    continue;
                }

                // Compare values
                bool match = false;
                try
                {
                    if (value is float fVal)
                    {
                        float target = float.Parse(valueStr);
                        match = op switch
                        {
                            ">" => fVal > target,
                            ">=" => fVal >= target,
                            "<" => fVal < target,
                            "<=" => fVal <= target,
                            "==" => Math.Abs(fVal - target) < 0.001f,
                            "!=" => Math.Abs(fVal - target) >= 0.001f,
                            _ => false,
                        };
                    }
                    else if (value is int iVal)
                    {
                        int target = int.Parse(valueStr);
                        match = op switch
                        {
                            ">" => iVal > target,
                            ">=" => iVal >= target,
                            "<" => iVal < target,
                            "<=" => iVal <= target,
                            "==" => iVal == target,
                            "!=" => iVal != target,
                            _ => false,
                        };
                    }
                    else if (value is bool bVal)
                    {
                        bool target = bool.Parse(valueStr);
                        match = op switch
                        {
                            "==" => bVal == target,
                            "!=" => bVal != target,
                            _ => false,
                        };
                    }
                    else if (value is Enum eVal)
                    {
                        match = op switch
                        {
                            "==" => eVal.ToString() == valueStr,
                            "!=" => eVal.ToString() != valueStr,
                            _ => false,
                        };
                    }
                    else
                    {
                        string sVal = value?.ToString() ?? "";
                        match = op switch
                        {
                            "==" => sVal == valueStr,
                            "!=" => sVal != valueStr,
                            "contains" => sVal.Contains(valueStr, StringComparison.OrdinalIgnoreCase),
                            _ => false,
                        };
                    }
                }
                catch { continue; }

                if (match)
                {
                    results.Add(new
                    {
                        name = comp.gameObject.name,
                        path = GameObjectFinder.GetPath(comp.gameObject),
                        component = componentName,
                        property = propertyName,
                        value = value?.ToString(),
                    });
                    if (results.Count >= limit) break;
                }
            }

            return new { query, count = results.Count, results = results.ToArray() };
        }

        /// <summary>
        /// Auto-fill serialized List/Array fields on a component by matching objects with tag/name pattern.
        /// </summary>
        private static object ReferenceBind(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            string componentType = Validate.Required<string>(p, "componentType");
            string fieldName = Validate.Required<string>(p, "fieldName");
            string matchBy = (string)p?["matchBy"] ?? "tag"; // "tag", "name", "component"
            string pattern = Validate.Required<string>(p, "pattern");

            WorkflowManager.SnapshotObject(go);

            var comp = go.GetComponents<Component>()
                .FirstOrDefault(c => c != null && c.GetType().Name == componentType);
            if (comp == null)
                throw new McpException(-32003, $"Component '{componentType}' not found on '{go.name}'");

            var so = new SerializedObject(comp);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
                throw new McpException(-32003, $"Field '{fieldName}' not found on '{componentType}'");

            if (!prop.isArray)
                throw new McpException(-32602, $"Field '{fieldName}' is not an array/list");

            // Find matching objects
            var allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var matched = new List<GameObject>();

            foreach (var obj in allObjects)
            {
                bool isMatch = matchBy switch
                {
                    "tag" => obj.CompareTag(pattern),
                    "name" => obj.name.Contains(pattern, StringComparison.OrdinalIgnoreCase),
                    "component" => obj.GetComponent(pattern) != null,
                    _ => false,
                };
                if (isMatch) matched.Add(obj);
            }

            // Determine target type and assign
            prop.arraySize = matched.Count;
            for (int i = 0; i < matched.Count; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                if (element.propertyType == SerializedPropertyType.ObjectReference)
                {
                    // Check if it expects a Component or a GameObject
                    string arrayTypeName = prop.arrayElementType;
                    if (arrayTypeName == "PPtr<$GameObject>")
                    {
                        element.objectReferenceValue = matched[i];
                    }
                    else
                    {
                        // Try to find the component type on the matched object
                        string cleanType = arrayTypeName.Replace("PPtr<$", "").Replace(">", "");
                        var targetComp = matched[i].GetComponent(cleanType);
                        element.objectReferenceValue = targetComp != null ? targetComp : (UnityEngine.Object)matched[i];
                    }
                }
            }

            so.ApplyModifiedProperties();

            return new
            {
                success = true,
                gameObject = go.name,
                componentType,
                fieldName,
                matchBy,
                pattern,
                boundCount = matched.Count,
                boundObjects = matched.Take(20).Select(m => m.name).ToArray(),
            };
        }
    }
}
