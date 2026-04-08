using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarnelLabs.MCP
{
    public static class PerceptionHandler
    {
        public static void Register()
        {
            CommandRouter.Register("perception.sceneSummary", SceneSummary);
            CommandRouter.Register("perception.hierarchyDescribe", HierarchyDescribe);
            CommandRouter.Register("perception.scriptAnalyze", ScriptAnalyze);
            CommandRouter.Register("perception.sceneMaterials", SceneMaterials);
            CommandRouter.Register("perception.sceneContext", SceneContext);
        }

        private static object SceneSummary(JToken p)
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var allTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int totalObjects = allTransforms.Length;

            // Component statistics
            var componentCounts = new Dictionary<string, int>();
            int totalComponents = 0;
            int activeCount = 0;
            int inactiveCount = 0;
            int maxDepth = 0;

            foreach (var t in allTransforms)
            {
                if (t.gameObject.activeInHierarchy) activeCount++;
                else inactiveCount++;

                int depth = 0;
                var parent = t.parent;
                while (parent != null) { depth++; parent = parent.parent; }
                if (depth > maxDepth) maxDepth = depth;

                var components = t.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c == null) continue;
                    totalComponents++;
                    string typeName = c.GetType().Name;
                    componentCounts.TryGetValue(typeName, out int count);
                    componentCounts[typeName] = count + 1;
                }
            }

            var topComponents = componentCounts
                .OrderByDescending(kv => kv.Value)
                .Take(20)
                .Select(kv => new { type = kv.Key, count = kv.Value })
                .ToArray();

            // Light counts
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            int totalVertices = 0;
            int totalTriangles = 0;
            foreach (var r in renderers)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    totalVertices += mf.sharedMesh.vertexCount;
                    totalTriangles += mf.sharedMesh.triangles.Length / 3;
                }
            }

            return new
            {
                sceneName = scene.name,
                scenePath = scene.path,
                isDirty = scene.isDirty,
                totalGameObjects = totalObjects,
                rootObjects = roots.Length,
                activeObjects = activeCount,
                inactiveObjects = inactiveCount,
                maxHierarchyDepth = maxDepth,
                totalComponents,
                uniqueComponentTypes = componentCounts.Count,
                topComponents,
                lightCount = lights.Length,
                cameraCount = cameras.Length,
                rendererCount = renderers.Length,
                totalVertices,
                totalTriangles,
            };
        }

        private static object HierarchyDescribe(JToken p)
        {
            int maxDepth = (int?)p?["maxDepth"] ?? 5;
            bool includeComponents = (bool?)p?["includeComponents"] ?? false;
            string rootFilter = (string)p?["root"];

            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();

            if (!string.IsNullOrEmpty(rootFilter))
            {
                var filtered = roots.Where(r => r.name.Contains(rootFilter, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (filtered.Length == 0) throw new McpException(-32003, $"No root object matching '{rootFilter}'");
                roots = filtered;
            }

            var lines = new List<string>();
            lines.Add($"Scene: {scene.name}");
            foreach (var root in roots)
            {
                BuildTree(root.transform, 0, maxDepth, includeComponents, lines);
            }

            return new { tree = string.Join("\n", lines), lineCount = lines.Count };
        }

        private static void BuildTree(Transform t, int depth, int maxDepth, bool includeComponents, List<string> lines)
        {
            string indent = depth == 0 ? "" : new string(' ', depth * 2) + (depth > 0 ? "├─ " : "");
            string activeFlag = t.gameObject.activeSelf ? "" : " [inactive]";
            string componentInfo = "";
            if (includeComponents)
            {
                var comps = t.GetComponents<Component>()
                    .Where(c => c != null && c.GetType().Name != "Transform")
                    .Select(c => c.GetType().Name);
                var compList = string.Join(", ", comps);
                if (!string.IsNullOrEmpty(compList)) componentInfo = $" ({compList})";
            }
            lines.Add($"{indent}{t.name}{activeFlag}{componentInfo}");

            if (depth >= maxDepth && t.childCount > 0)
            {
                string childIndent = new string(' ', (depth + 1) * 2) + "└─ ";
                lines.Add($"{childIndent}... ({t.childCount} children)");
                return;
            }

            for (int i = 0; i < t.childCount; i++)
            {
                BuildTree(t.GetChild(i), depth + 1, maxDepth, includeComponents, lines);
            }
        }

        private static object ScriptAnalyze(JToken p)
        {
            string typeName = Validate.Required<string>(p, "typeName");

            // Search in all assemblies
            Type targetType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    targetType = assembly.GetTypes().FirstOrDefault(t =>
                        t.Name == typeName || t.FullName == typeName);
                    if (targetType != null) break;
                }
                catch { }
            }

            if (targetType == null)
                throw new McpException(-32003, $"Type not found: {typeName}");

            var publicFields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(f => new { name = f.Name, type = f.FieldType.Name, isSerializable = true })
                .ToArray();

            var serializedPrivateFields = targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(f => f.GetCustomAttribute<SerializeField>() != null)
                .Select(f => new { name = f.Name, type = f.FieldType.Name, isSerializable = true })
                .ToArray();

            var publicProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(prop => new { name = prop.Name, type = prop.PropertyType.Name, canRead = prop.CanRead, canWrite = prop.CanWrite })
                .ToArray();

            var publicMethods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName) // exclude property getters/setters
                .Select(m => new
                {
                    name = m.Name,
                    returnType = m.ReturnType.Name,
                    parameters = m.GetParameters().Select(param => new { name = param.Name, type = param.ParameterType.Name }).ToArray(),
                })
                .ToArray();

            var baseTypes = new List<string>();
            var bt = targetType.BaseType;
            while (bt != null && bt != typeof(object))
            {
                baseTypes.Add(bt.Name);
                bt = bt.BaseType;
            }

            var interfaces = targetType.GetInterfaces().Select(i => i.Name).ToArray();

            return new
            {
                name = targetType.Name,
                fullName = targetType.FullName,
                assembly = targetType.Assembly.GetName().Name,
                baseTypes = baseTypes.ToArray(),
                interfaces,
                isAbstract = targetType.IsAbstract,
                isSealed = targetType.IsSealed,
                publicFields,
                serializedPrivateFields,
                publicProperties,
                publicMethods,
            };
        }

        private static object SceneMaterials(JToken p)
        {
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var materialMap = new Dictionary<string, (Material mat, int count, List<string> users)>();

            foreach (var r in renderers)
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null) continue;
                    string key = mat.GetInstanceID().ToString();
                    if (!materialMap.ContainsKey(key))
                        materialMap[key] = (mat, 0, new List<string>());

                    var entry = materialMap[key];
                    entry.count++;
                    if (entry.users.Count < 5) entry.users.Add(r.gameObject.name);
                    materialMap[key] = entry;
                }
            }

            var materials = materialMap.Values
                .OrderByDescending(m => m.count)
                .Take(50)
                .Select(m => new
                {
                    name = m.mat.name,
                    shader = m.mat.shader != null ? m.mat.shader.name : "null",
                    renderQueue = m.mat.renderQueue,
                    usageCount = m.count,
                    sampleUsers = m.users.ToArray(),
                })
                .ToArray();

            var shaderCounts = materialMap.Values
                .GroupBy(m => m.mat.shader != null ? m.mat.shader.name : "null")
                .Select(g => new { shader = g.Key, materialCount = g.Count() })
                .OrderByDescending(s => s.materialCount)
                .ToArray();

            return new
            {
                totalMaterials = materialMap.Count,
                totalRenderers = renderers.Length,
                shaderBreakdown = shaderCounts,
                materials,
            };
        }

        private static object SceneContext(JToken p)
        {
            var scene = SceneManager.GetActiveScene();

            // Current selection
            var selectedObjects = Selection.gameObjects.Select(go => new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
                components = go.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToArray(),
            }).ToArray();

            var selectedAssets = Selection.assetGUIDs.Select(guid =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return new { path, type = AssetDatabase.GetMainAssetTypeAtPath(path)?.Name ?? "Unknown" };
            }).ToArray();

            // Active scene view
            var sceneView = SceneView.lastActiveSceneView;
            object sceneViewInfo = null;
            if (sceneView != null)
            {
                sceneViewInfo = new
                {
                    cameraPosition = new { x = sceneView.camera.transform.position.x, y = sceneView.camera.transform.position.y, z = sceneView.camera.transform.position.z },
                    pivot = new { x = sceneView.pivot.x, y = sceneView.pivot.y, z = sceneView.pivot.z },
                    size = sceneView.size,
                    is2D = sceneView.in2DMode,
                    orthographic = sceneView.orthographic,
                };
            }

            return new
            {
                sceneName = scene.name,
                scenePath = scene.path,
                isDirty = scene.isDirty,
                isPlaying = EditorApplication.isPlaying,
                isPaused = EditorApplication.isPaused,
                isCompiling = EditorApplication.isCompiling,
                selectedGameObjects = selectedObjects,
                selectedAssets,
                sceneView = sceneViewInfo,
            };
        }
    }
}
