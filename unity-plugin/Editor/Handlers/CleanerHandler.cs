using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarnelLabs.MCP
{
    public static class CleanerHandler
    {
        public static void Register()
        {
            CommandRouter.Register("cleaner.findUnusedAssets", FindUnusedAssets);
            CommandRouter.Register("cleaner.findDuplicateAssets", FindDuplicateAssets);
            CommandRouter.Register("cleaner.findMissingScripts", FindMissingScriptsAndRemove);
            CommandRouter.Register("cleaner.findEmptyFolders", FindEmptyFolders);
            CommandRouter.Register("cleaner.getAssetDependencies", GetAssetDependencies);
            CommandRouter.Register("cleaner.getReferences", GetReferences);
            CommandRouter.Register("cleaner.findLargeFiles", FindLargeFiles);
            CommandRouter.Register("cleaner.findUnusedMaterials", FindUnusedMaterials);
            CommandRouter.Register("cleaner.deleteEmptyFolders", DeleteEmptyFolders);
            CommandRouter.Register("cleaner.projectSizeReport", ProjectSizeReport);
        }

        private static object FindUnusedAssets(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var types = p["types"]?.ToObject<string[]>() ?? new[] { "t:Texture2D", "t:Material", "t:AudioClip" };

            // Get all assets used in scenes
            var scenePaths = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            var usedGuids = new HashSet<string>();
            foreach (var scenePath in scenePaths)
            {
                var deps = AssetDatabase.GetDependencies(scenePath, true);
                foreach (var dep in deps)
                    usedGuids.Add(AssetDatabase.AssetPathToGUID(dep));
            }

            var unused = new List<(string path, string guid, string type, double sizeKB)>();
            foreach (var typeFilter in types)
            {
                var guids = AssetDatabase.FindAssets(typeFilter, new[] { folder });
                foreach (var guid in guids)
                {
                    if (usedGuids.Contains(guid)) continue;
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("/Editor/") || path.Contains("/Plugins/")) continue;
                    try
                    {
                        var size = new FileInfo(Path.Combine(Application.dataPath, "..", path)).Length;
                        unused.Add((path, guid, Path.GetExtension(path), Math.Round(size / 1024.0, 2)));
                    }
                    catch { unused.Add((path, guid, Path.GetExtension(path), 0.0)); }
                }
            }

            return new { count = unused.Count, assets = unused.OrderByDescending(x => x.sizeKB).Take(100).Select(x => new { x.path, x.guid, x.type, x.sizeKB }).ToArray() };
        }

        private static object FindDuplicateAssets(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var typeFilter = p["type"]?.Value<string>() ?? "t:Object";
            var guids = AssetDatabase.FindAssets(typeFilter, new[] { folder });

            var sizeGroups = new Dictionary<long, List<string>>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                try
                {
                    var size = new FileInfo(Path.Combine(Application.dataPath, "..", path)).Length;
                    if (!sizeGroups.ContainsKey(size)) sizeGroups[size] = new List<string>();
                    sizeGroups[size].Add(path);
                }
                catch { }
            }

            var duplicates = sizeGroups
                .Where(kv => kv.Value.Count > 1 && kv.Key > 1024) // > 1KB and multiple files
                .Select(kv => new
                {
                    sizeKB = Math.Round(kv.Key / 1024.0, 2),
                    count = kv.Value.Count,
                    files = kv.Value.ToArray(),
                })
                .OrderByDescending(x => x.sizeKB)
                .Take(50)
                .ToArray();

            return new { groups = duplicates.Length, duplicates };
        }

        private static object FindMissingScriptsAndRemove(JToken p)
        {
            var remove = p["remove"]?.Value<bool>() ?? false;
            var results = new List<object>();
            int removedCount = 0;

            var rootObjects = new List<GameObject>();
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (scene.isLoaded) rootObjects.AddRange(scene.GetRootGameObjects());
            }

            var stack = new Stack<Transform>();
            foreach (var root in rootObjects) stack.Push(root.transform);

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (t == null) continue;
                var go = t.gameObject;
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (missingCount > 0)
                {
                    results.Add(new { name = go.name, path = GameObjectFinder.GetPath(go), missingCount });
                    if (remove)
                    {
                        Undo.RegisterCompleteObjectUndo(go, "MCP: Remove Missing Scripts");
                        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                        removedCount += removed;
                    }
                }
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }

            return new { found = results.Count, removed = removedCount, objects = results };
        }

        private static object FindEmptyFolders(JToken p)
        {
            var root = p["folder"]?.Value<string>() ?? "Assets";
            var fullRoot = Path.Combine(Application.dataPath, "..", root);
            var emptyFolders = new List<string>();

            FindEmptyFoldersRecursive(fullRoot, root, emptyFolders);
            return new { count = emptyFolders.Count, folders = emptyFolders };
        }

        private static bool FindEmptyFoldersRecursive(string fullPath, string assetPath, List<string> results)
        {
            if (!Directory.Exists(fullPath)) return true;
            var dirs = Directory.GetDirectories(fullPath);
            var files = Directory.GetFiles(fullPath).Where(f => !f.EndsWith(".meta")).ToArray();
            bool allSubdirsEmpty = true;

            foreach (var dir in dirs)
            {
                var dirName = Path.GetFileName(dir);
                if (!FindEmptyFoldersRecursive(dir, assetPath + "/" + dirName, results))
                    allSubdirsEmpty = false;
            }

            if (files.Length == 0 && allSubdirsEmpty && dirs.Length == 0)
            {
                results.Add(assetPath);
                return true;
            }
            return false;
        }

        private static object GetAssetDependencies(JToken p)
        {
            var path = Validate.Required<string>(p, "assetPath");
            path = Validate.SafeAssetPath(path);
            var recursive = p["recursive"]?.Value<bool>() ?? true;

            var deps = AssetDatabase.GetDependencies(path, recursive);
            var results = deps.Where(d => d != path).Select(d => new
            {
                path = d,
                type = Path.GetExtension(d),
            }).ToArray();

            return new { asset = path, count = results.Length, dependencies = results };
        }

        private static object GetReferences(JToken p)
        {
            var targetPath = Validate.Required<string>(p, "assetPath");
            targetPath = Validate.SafeAssetPath(targetPath);
            var folder = p["folder"]?.Value<string>() ?? "Assets";

            var targetGuid = AssetDatabase.AssetPathToGUID(targetPath);
            if (string.IsNullOrEmpty(targetGuid))
                throw new McpException(-32003, $"Asset not found: {targetPath}");

            var allGuids = AssetDatabase.FindAssets("", new[] { folder });
            var referencingAssets = new List<object>();

            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == targetPath) continue;
                var deps = AssetDatabase.GetDependencies(path, false);
                if (deps.Contains(targetPath))
                    referencingAssets.Add(new { path, type = Path.GetExtension(path) });
            }

            return new { target = targetPath, referencedBy = referencingAssets.Count, references = referencingAssets.Take(50).ToArray() };
        }

        private static object FindLargeFiles(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var thresholdMB = p["thresholdMB"]?.Value<double>() ?? 5.0;
            var thresholdBytes = (long)(thresholdMB * 1024 * 1024);
            var fullRoot = Path.Combine(Application.dataPath, "..", folder);
            var results = new List<(string path, double sizeMB, string extension)>();

            if (Directory.Exists(fullRoot))
            {
                foreach (var file in Directory.GetFiles(fullRoot, "*", SearchOption.AllDirectories))
                {
                    if (file.EndsWith(".meta")) continue;
                    var fi = new FileInfo(file);
                    if (fi.Length >= thresholdBytes)
                    {
                        var relative = file.Replace("\\", "/");
                        var idx = relative.IndexOf(folder);
                        if (idx >= 0) relative = relative.Substring(idx);
                        results.Add((relative, Math.Round(fi.Length / (1024.0 * 1024.0), 2), fi.Extension));
                    }
                }
            }

            return new { thresholdMB, count = results.Count, files = results.OrderByDescending(x => x.sizeMB).Take(50).Select(x => new { x.path, x.sizeMB, x.extension }).ToArray() };
        }

        private static object FindUnusedMaterials(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";

            // Collect all materials used by renderers in scene
            var usedMaterials = new HashSet<string>();
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var r in renderers)
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat != null)
                    {
                        var matPath = AssetDatabase.GetAssetPath(mat);
                        if (!string.IsNullOrEmpty(matPath)) usedMaterials.Add(matPath);
                    }
                }
            }

            // Also check build scenes
            var scenePaths = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            foreach (var scenePath in scenePaths)
            {
                var deps = AssetDatabase.GetDependencies(scenePath, true);
                foreach (var dep in deps)
                    if (dep.EndsWith(".mat")) usedMaterials.Add(dep);
            }

            var allMatGuids = AssetDatabase.FindAssets("t:Material", new[] { folder });
            var unused = new List<object>();
            foreach (var guid in allMatGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!usedMaterials.Contains(path))
                    unused.Add(new { path, guid });
            }

            return new { totalMaterials = allMatGuids.Length, unusedCount = unused.Count, unused = unused.Take(100).ToArray() };
        }

        private static object DeleteEmptyFolders(JToken p)
        {
            var root = p["folder"]?.Value<string>() ?? "Assets";
            var fullRoot = Path.Combine(Application.dataPath, "..", root);
            var emptyFolders = new List<string>();
            FindEmptyFoldersRecursive(fullRoot, root, emptyFolders);

            int deleted = 0;
            foreach (var folder in emptyFolders)
            {
                if (AssetDatabase.DeleteAsset(folder)) deleted++;
            }
            AssetDatabase.Refresh();
            return new { found = emptyFolders.Count, deleted };
        }

        private static object ProjectSizeReport(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var fullRoot = Path.Combine(Application.dataPath, "..", folder);
            var sizeByCat = new Dictionary<string, long>();

            if (Directory.Exists(fullRoot))
            {
                foreach (var file in Directory.GetFiles(fullRoot, "*", SearchOption.AllDirectories))
                {
                    if (file.EndsWith(".meta")) continue;
                    var ext = Path.GetExtension(file).ToLower();
                    var category = CategorizeExtension(ext);
                    if (!sizeByCat.ContainsKey(category)) sizeByCat[category] = 0;
                    try { sizeByCat[category] += new FileInfo(file).Length; } catch { }
                }
            }

            return new
            {
                folder,
                totalSizeMB = Math.Round(sizeByCat.Values.Sum() / (1024.0 * 1024.0), 2),
                byCategory = sizeByCat
                    .OrderByDescending(kv => kv.Value)
                    .Select(kv => new { category = kv.Key, sizeMB = Math.Round(kv.Value / (1024.0 * 1024.0), 2) })
                    .ToArray(),
            };
        }

        private static string CategorizeExtension(string ext)
        {
            switch (ext)
            {
                case ".png": case ".jpg": case ".jpeg": case ".tga": case ".psd": case ".exr": case ".hdr": return "Textures";
                case ".fbx": case ".obj": case ".blend": case ".dae": case ".3ds": return "Models";
                case ".wav": case ".mp3": case ".ogg": case ".aiff": return "Audio";
                case ".anim": case ".controller": return "Animation";
                case ".cs": return "Scripts";
                case ".shader": case ".shadergraph": case ".compute": return "Shaders";
                case ".mat": return "Materials";
                case ".prefab": return "Prefabs";
                case ".unity": return "Scenes";
                case ".asset": return "ScriptableObjects";
                default: return "Other";
            }
        }
    }
}
