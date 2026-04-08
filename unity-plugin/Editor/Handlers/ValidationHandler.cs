using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarnelLabs.MCP
{
    public static class ValidationHandler
    {
        public static void Register()
        {
            CommandRouter.Register("validate.missingScripts", FindMissingScripts);
            CommandRouter.Register("validate.missingReferences", FindMissingReferences);
            CommandRouter.Register("validate.shaderErrors", FindShaderErrors);
            CommandRouter.Register("validate.emptyGameObjects", FindEmptyGameObjects);
            CommandRouter.Register("validate.duplicateNames", FindDuplicateNames);
            CommandRouter.Register("validate.disabledRenderers", FindDisabledRenderers);
            CommandRouter.Register("validate.sceneStats", GetSceneStats);
            CommandRouter.Register("validate.prefabOverrides", FindPrefabOverrides);
            CommandRouter.Register("validate.largeMeshes", FindLargeMeshes);
            CommandRouter.Register("validate.untaggedObjects", FindUntaggedObjects);
        }

        private static object FindMissingScripts(JToken p)
        {
            var results = new List<object>();
            var rootObjects = GetAllRootObjects();
            var stack = new Stack<Transform>();
            foreach (var root in rootObjects) stack.Push(root.transform);

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (t == null) continue;
                var go = t.gameObject;
                var components = go.GetComponents<Component>();
                int missingCount = components.Count(c => c == null);
                if (missingCount > 0)
                    results.Add(new { name = go.name, path = GameObjectFinder.GetPath(go), missingCount });
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }
            return new { count = results.Count, objects = results };
        }

        private static object FindMissingReferences(JToken p)
        {
            var maxResults = p["maxResults"]?.Value<int>() ?? 100;
            var results = new List<object>();
            var rootObjects = GetAllRootObjects();
            var stack = new Stack<Transform>();
            foreach (var root in rootObjects) stack.Push(root.transform);

            while (stack.Count > 0 && results.Count < maxResults)
            {
                var t = stack.Pop();
                if (t == null) continue;
                var go = t.gameObject;
                var components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    var so = new SerializedObject(comp);
                    var iter = so.GetIterator();
                    while (iter.Next(true))
                    {
                        if (iter.propertyType == SerializedPropertyType.ObjectReference &&
                            iter.objectReferenceValue == null &&
                            iter.objectReferenceInstanceIDValue != 0)
                        {
                            results.Add(new
                            {
                                gameObject = go.name,
                                path = GameObjectFinder.GetPath(go),
                                component = comp.GetType().Name,
                                property = iter.name,
                            });
                            if (results.Count >= maxResults) break;
                        }
                    }
                    if (results.Count >= maxResults) break;
                }
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }
            return new { count = results.Count, missingReferences = results };
        }

        private static object FindShaderErrors(JToken p)
        {
            var guids = AssetDatabase.FindAssets("t:Material");
            var results = new List<object>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null) continue;
                if (mat.shader.name == "Hidden/InternalErrorShader")
                    results.Add(new { materialName = mat.name, path, shaderName = "ERROR (missing shader)" });
            }
            return new { count = results.Count, materials = results };
        }

        private static object FindEmptyGameObjects(JToken p)
        {
            var results = new List<object>();
            var rootObjects = GetAllRootObjects();
            var stack = new Stack<Transform>();
            foreach (var root in rootObjects) stack.Push(root.transform);

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (t == null) continue;
                var go = t.gameObject;
                var components = go.GetComponents<Component>();
                // Only Transform component = empty
                if (components.Length == 1 && t.childCount == 0)
                    results.Add(new { name = go.name, path = GameObjectFinder.GetPath(go) });
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }
            return new { count = results.Count, emptyObjects = results };
        }

        private static object FindDuplicateNames(JToken p)
        {
            var nameCounts = new Dictionary<string, List<string>>();
            var rootObjects = GetAllRootObjects();
            var stack = new Stack<Transform>();
            foreach (var root in rootObjects) stack.Push(root.transform);

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (t == null) continue;
                var go = t.gameObject;
                if (!nameCounts.ContainsKey(go.name))
                    nameCounts[go.name] = new List<string>();
                nameCounts[go.name].Add(GameObjectFinder.GetPath(go));
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }

            var duplicates = nameCounts
                .Where(kv => kv.Value.Count > 1)
                .Select(kv => new { name = kv.Key, count = kv.Value.Count, paths = kv.Value.Take(10).ToArray() })
                .OrderByDescending(x => x.count)
                .Take(50)
                .ToArray();

            return new { count = duplicates.Length, duplicates };
        }

        private static object FindDisabledRenderers(JToken p)
        {
            var results = new List<object>();
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var r in renderers)
            {
                if (!r.enabled)
                    results.Add(new { name = r.gameObject.name, path = GameObjectFinder.GetPath(r.gameObject), type = r.GetType().Name });
            }
            return new { count = results.Count, disabledRenderers = results };
        }

        private static object GetSceneStats(JToken p)
        {
            int totalObjects = 0, activeObjects = 0, totalComponents = 0;
            int meshRenderers = 0, lights = 0, cameras = 0, colliders = 0, rigidbodies = 0;
            long totalVertices = 0, totalTriangles = 0;

            var rootObjects = GetAllRootObjects();
            var stack = new Stack<Transform>();
            foreach (var root in rootObjects) stack.Push(root.transform);

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (t == null) continue;
                var go = t.gameObject;
                totalObjects++;
                if (go.activeInHierarchy) activeObjects++;
                totalComponents += go.GetComponents<Component>().Count(c => c != null);

                var mf = go.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    meshRenderers++;
                    totalVertices += mf.sharedMesh.vertexCount;
                    totalTriangles += mf.sharedMesh.triangles.Length / 3;
                }
                if (go.GetComponent<Light>() != null) lights++;
                if (go.GetComponent<Camera>() != null) cameras++;
                if (go.GetComponent<Collider>() != null) colliders++;
                if (go.GetComponent<Rigidbody>() != null) rigidbodies++;

                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }

            return new
            {
                totalGameObjects = totalObjects,
                activeGameObjects = activeObjects,
                totalComponents,
                meshRenderers, lights, cameras, colliders, rigidbodies,
                totalVertices, totalTriangles,
                sceneCount = SceneManager.sceneCount,
            };
        }

        private static object FindPrefabOverrides(JToken p)
        {
            var results = new List<object>();
            var rootObjects = GetAllRootObjects();
            var stack = new Stack<Transform>();
            foreach (var root in rootObjects) stack.Push(root.transform);

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (t == null) continue;
                var go = t.gameObject;
                if (PrefabUtility.IsPartOfPrefabInstance(go))
                {
                    var mods = PrefabUtility.GetPropertyModifications(go);
                    if (mods != null && mods.Length > 0)
                    {
                        results.Add(new
                        {
                            name = go.name,
                            path = GameObjectFinder.GetPath(go),
                            overrideCount = mods.Length,
                            prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go),
                        });
                    }
                }
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }
            return new { count = results.Count, overrides = results.Take(50).ToArray() };
        }

        private static object FindLargeMeshes(JToken p)
        {
            var threshold = p["vertexThreshold"]?.Value<int>() ?? 10000;
            var results = new List<(string name, string path, string meshName, int vertexCount, int triangleCount)>();
            var meshFilters = UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;
                if (mf.sharedMesh.vertexCount >= threshold)
                {
                    results.Add((mf.gameObject.name, GameObjectFinder.GetPath(mf.gameObject), mf.sharedMesh.name, mf.sharedMesh.vertexCount, mf.sharedMesh.triangles.Length / 3));
                }
            }
            return new { threshold, count = results.Count, meshes = results.OrderByDescending(x => x.vertexCount).Select(x => new { x.name, x.path, x.meshName, x.vertexCount, x.triangleCount }).ToArray() };
        }

        private static object FindUntaggedObjects(JToken p)
        {
            var results = new List<object>();
            var rootObjects = GetAllRootObjects();
            var stack = new Stack<Transform>();
            foreach (var root in rootObjects) stack.Push(root.transform);

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (t == null) continue;
                var go = t.gameObject;
                if (go.tag == "Untagged" && go.activeInHierarchy)
                    results.Add(new { name = go.name, path = GameObjectFinder.GetPath(go) });
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }
            return new { count = results.Count, objects = results.Take(100).ToArray() };
        }

        private static List<GameObject> GetAllRootObjects()
        {
            var roots = new List<GameObject>();
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (scene.isLoaded) roots.AddRange(scene.GetRootGameObjects());
            }
            return roots;
        }
    }
}
