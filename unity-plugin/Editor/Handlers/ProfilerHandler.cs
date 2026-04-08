using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarnelLabs.MCP
{
    public static class ProfilerHandler
    {
        public static void Register()
        {
            CommandRouter.Register("profiler.memoryOverview", MemoryOverview);
            CommandRouter.Register("profiler.objectCount", ObjectCount);
            CommandRouter.Register("profiler.renderingStats", RenderingStats);
            CommandRouter.Register("profiler.textureMemory", TextureMemory);
            CommandRouter.Register("profiler.meshMemory", MeshMemory);
            CommandRouter.Register("profiler.materialCount", MaterialCount);
            CommandRouter.Register("profiler.componentStats", ComponentStats);
            CommandRouter.Register("profiler.shaderVariants", ShaderVariants);
            CommandRouter.Register("profiler.assetCount", AssetCount);
            CommandRouter.Register("profiler.sceneComplexity", SceneComplexity);
        }

        private static object MemoryOverview(JToken p)
        {
            var totalMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
            var usedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            var monoUsed = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
            var monoHeap = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong();
            var gfxMemory = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver();

            return new
            {
                totalReservedMB = Math.Round(totalMemory / (1024.0 * 1024.0), 2),
                totalAllocatedMB = Math.Round(usedMemory / (1024.0 * 1024.0), 2),
                monoUsedMB = Math.Round(monoUsed / (1024.0 * 1024.0), 2),
                monoHeapMB = Math.Round(monoHeap / (1024.0 * 1024.0), 2),
                gfxDriverMB = Math.Round(gfxMemory / (1024.0 * 1024.0), 2),
            };
        }

        private static object ObjectCount(JToken p)
        {
            var includeInactive = p["includeInactive"]?.Value<bool>() ?? true;

            int gameObjects = 0, meshRenderers = 0, skinnedMeshRenderers = 0;
            int lights = 0, cameras = 0, colliders = 0, rigidbodies = 0, audioSources = 0;
            int particleSystems = 0, canvases = 0, animators = 0;

            var allGOs = includeInactive
                ? Resources.FindObjectsOfTypeAll<GameObject>().Where(go => go.scene.isLoaded)
                : UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).AsEnumerable();

            foreach (var go in allGOs)
            {
                gameObjects++;
                if (go.GetComponent<MeshRenderer>()) meshRenderers++;
                if (go.GetComponent<SkinnedMeshRenderer>()) skinnedMeshRenderers++;
                if (go.GetComponent<Light>()) lights++;
                if (go.GetComponent<Camera>()) cameras++;
                if (go.GetComponent<Collider>()) colliders++;
                if (go.GetComponent<Rigidbody>()) rigidbodies++;
                if (go.GetComponent<AudioSource>()) audioSources++;
                if (go.GetComponent<ParticleSystem>()) particleSystems++;
                if (go.GetComponent<Canvas>()) canvases++;
                if (go.GetComponent<Animator>()) animators++;
            }

            return new
            {
                gameObjects, meshRenderers, skinnedMeshRenderers,
                lights, cameras, colliders, rigidbodies,
                audioSources, particleSystems, canvases, animators,
            };
        }

        private static object RenderingStats(JToken p)
        {
            long totalVerts = 0, totalTris = 0;
            int meshCount = 0;
            var materialSet = new HashSet<int>();

            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var r in renderers)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    meshCount++;
                    totalVerts += mf.sharedMesh.vertexCount;
                    totalTris += mf.sharedMesh.triangles.Length / 3;
                }
                var smr = r as SkinnedMeshRenderer;
                if (smr != null && smr.sharedMesh != null)
                {
                    meshCount++;
                    totalVerts += smr.sharedMesh.vertexCount;
                    totalTris += smr.sharedMesh.triangles.Length / 3;
                }
                foreach (var mat in r.sharedMaterials)
                    if (mat != null) materialSet.Add(mat.GetInstanceID());
            }

            return new
            {
                visibleRenderers = renderers.Length,
                meshCount,
                totalVertices = totalVerts,
                totalTriangles = totalTris,
                uniqueMaterials = materialSet.Count,
                drawCallEstimate = renderers.Length, // rough estimate
            };
        }

        private static object TextureMemory(JToken p)
        {
            var maxResults = p["maxResults"]?.Value<int>() ?? 20;
            var textures = Resources.FindObjectsOfTypeAll<Texture>()
                .Where(t => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(t)))
                .Select(t =>
                {
                    var mem = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t);
                    return new { texture = t, memory = mem, path = AssetDatabase.GetAssetPath(t) };
                })
                .OrderByDescending(x => x.memory)
                .Take(maxResults)
                .Select(x => new
                {
                    name = x.texture.name,
                    path = x.path,
                    memoryMB = Math.Round(x.memory / (1024.0 * 1024.0), 2),
                    width = x.texture.width,
                    height = x.texture.height,
                })
                .ToArray();

            var totalMem = Resources.FindObjectsOfTypeAll<Texture>()
                .Sum(t => UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t));

            return new
            {
                totalTextures = Resources.FindObjectsOfTypeAll<Texture>().Length,
                totalMemoryMB = Math.Round(totalMem / (1024.0 * 1024.0), 2),
                topTextures = textures,
            };
        }

        private static object MeshMemory(JToken p)
        {
            var maxResults = p["maxResults"]?.Value<int>() ?? 20;
            var meshes = Resources.FindObjectsOfTypeAll<Mesh>()
                .Where(m => !string.IsNullOrEmpty(m.name))
                .Select(m =>
                {
                    var mem = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(m);
                    return new { mesh = m, memory = mem };
                })
                .OrderByDescending(x => x.memory)
                .Take(maxResults)
                .Select(x => new
                {
                    name = x.mesh.name,
                    memoryKB = Math.Round(x.memory / 1024.0, 2),
                    vertexCount = x.mesh.vertexCount,
                    triangles = x.mesh.triangles.Length / 3,
                })
                .ToArray();

            return new { topMeshes = meshes };
        }

        private static object MaterialCount(JToken p)
        {
            var guids = AssetDatabase.FindAssets("t:Material");
            var shaderUsage = new Dictionary<string, int>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null) continue;
                var shaderName = mat.shader.name;
                if (!shaderUsage.ContainsKey(shaderName)) shaderUsage[shaderName] = 0;
                shaderUsage[shaderName]++;
            }

            return new
            {
                totalMaterials = guids.Length,
                uniqueShaders = shaderUsage.Count,
                shaderUsage = shaderUsage.OrderByDescending(kv => kv.Value)
                    .Take(20)
                    .Select(kv => new { shader = kv.Key, materialCount = kv.Value })
                    .ToArray(),
            };
        }

        private static object ComponentStats(JToken p)
        {
            var typeCounts = new Dictionary<string, int>();
            var allComponents = UnityEngine.Object.FindObjectsByType<Component>(FindObjectsSortMode.None);

            foreach (var comp in allComponents)
            {
                if (comp == null) continue;
                var typeName = comp.GetType().Name;
                if (!typeCounts.ContainsKey(typeName)) typeCounts[typeName] = 0;
                typeCounts[typeName]++;
            }

            return new
            {
                totalComponents = allComponents.Length,
                uniqueTypes = typeCounts.Count,
                byType = typeCounts.OrderByDescending(kv => kv.Value)
                    .Take(30)
                    .Select(kv => new { type = kv.Key, count = kv.Value })
                    .ToArray(),
            };
        }

        private static object ShaderVariants(JToken p)
        {
            var guids = AssetDatabase.FindAssets("t:Material");
            var shaderInfo = new Dictionary<string, HashSet<string>>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null) continue;
                if (!shaderInfo.ContainsKey(mat.shader.name))
                    shaderInfo[mat.shader.name] = new HashSet<string>();
                foreach (var keyword in mat.shaderKeywords)
                    shaderInfo[mat.shader.name].Add(keyword);
            }

            return new
            {
                shaderCount = shaderInfo.Count,
                shaders = shaderInfo
                    .OrderByDescending(kv => kv.Value.Count)
                    .Take(20)
                    .Select(kv => new { shader = kv.Key, keywordCount = kv.Value.Count, keywords = kv.Value.Take(10).ToArray() })
                    .ToArray(),
            };
        }

        private static object AssetCount(JToken p)
        {
            return new
            {
                textures = AssetDatabase.FindAssets("t:Texture2D").Length,
                materials = AssetDatabase.FindAssets("t:Material").Length,
                meshes = AssetDatabase.FindAssets("t:Mesh").Length,
                models = AssetDatabase.FindAssets("t:Model").Length,
                prefabs = AssetDatabase.FindAssets("t:Prefab").Length,
                scripts = AssetDatabase.FindAssets("t:Script").Length,
                audioClips = AssetDatabase.FindAssets("t:AudioClip").Length,
                animationClips = AssetDatabase.FindAssets("t:AnimationClip").Length,
                shaders = AssetDatabase.FindAssets("t:Shader").Length,
                scenes = AssetDatabase.FindAssets("t:Scene").Length,
                scriptableObjects = AssetDatabase.FindAssets("t:ScriptableObject").Length,
            };
        }

        private static object SceneComplexity(JToken p)
        {
            int totalObjects = 0, totalComponents = 0, maxDepth = 0;
            var rootObjects = new List<GameObject>();
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (scene.isLoaded) rootObjects.AddRange(scene.GetRootGameObjects());
            }

            var stack = new Stack<(Transform t, int depth)>();
            foreach (var root in rootObjects) stack.Push((root.transform, 0));

            while (stack.Count > 0)
            {
                var (t, depth) = stack.Pop();
                if (t == null) continue;
                totalObjects++;
                totalComponents += t.gameObject.GetComponents<Component>().Count(c => c != null);
                if (depth > maxDepth) maxDepth = depth;
                for (int i = 0; i < t.childCount; i++) stack.Push((t.GetChild(i), depth + 1));
            }

            return new
            {
                rootObjectCount = rootObjects.Count,
                totalGameObjects = totalObjects,
                totalComponents,
                maxHierarchyDepth = maxDepth,
                avgComponentsPerObject = totalObjects > 0 ? Math.Round((double)totalComponents / totalObjects, 2) : 0,
                complexityScore = totalObjects + totalComponents * 2 + maxDepth * 10,
            };
        }
    }
}
