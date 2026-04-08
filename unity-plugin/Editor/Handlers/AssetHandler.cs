using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class AssetHandler
    {
        public static void Register()
        {
            CommandRouter.Register("asset.search", SearchAssets);
            CommandRouter.Register("asset.info", GetAssetInfo);
            CommandRouter.Register("asset.createMaterial", CreateMaterial);
            CommandRouter.Register("asset.createPrefab", CreatePrefab);
            CommandRouter.Register("asset.importSettings", ImportSettings);
            CommandRouter.Register("asset.createFolder", CreateFolder);
            // ── 확장 도구 ──
            CommandRouter.Register("asset.delete", DeleteAsset);
            CommandRouter.Register("asset.move", MoveAsset);
            CommandRouter.Register("asset.copy", CopyAsset);
            CommandRouter.Register("asset.refresh", RefreshAssets);
            CommandRouter.Register("asset.reimport", ReimportAsset);
            CommandRouter.Register("asset.getLabels", GetLabels);
            CommandRouter.Register("asset.setLabels", SetLabels);
            // ── 배치 도구 ──
            CommandRouter.Register("asset.importSettingsBatch", ImportSettingsBatch);
        }

        private static object SearchAssets(JToken p)
        {
            string query = (string)p?["query"] ?? "";
            string type = (string)p?["type"];
            string folder = (string)p?["folder"];
            string filter = query;
            if (!string.IsNullOrEmpty(type)) filter = $"t:{type} {filter}".Trim();
            string[] searchFolders = !string.IsNullOrEmpty(folder) ? new[] { folder } : null;
            var guids = AssetDatabase.FindAssets(filter, searchFolders);
            var assets = guids.Take(100).Select(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                return new { path, type = assetType?.Name, guid };
            }).ToArray();
            return new { count = assets.Length, totalFound = guids.Length, assets };
        }

        private static object GetAssetInfo(JToken p)
        {
            string path = Validate.SafeAssetPath((string)p?["path"], "path");
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) throw new McpException(-32003, $"Asset not found: {path}");
            var importer = AssetImporter.GetAtPath(path);
            var dependencies = AssetDatabase.GetDependencies(path, false);
            long fileSize = -1;
            try { var fi = new FileInfo(Path.Combine(Application.dataPath, "..", path)); if (fi.Exists) fileSize = fi.Length; } catch { }
            return new
            {
                path, name = asset.name, type = asset.GetType().Name,
                guid = AssetDatabase.AssetPathToGUID(path), fileSize,
                importerType = importer?.GetType().Name,
                dependencies = dependencies.Where(d => d != path).ToArray(),
                labels = AssetDatabase.GetLabels(asset),
            };
        }

        private static object CreateMaterial(JToken p)
        {
            string name = Validate.Required((string)p?["name"], "name");
            string shaderName = (string)p?["shaderName"] ?? "Standard";
            string savePath = (string)p?["path"] ?? $"Assets/{name}.mat";
            savePath = Validate.SafeAssetPath(savePath, "path");

            var shader = Shader.Find(shaderName);
            if (shader == null) throw new McpException(-32003, $"Shader not found: {shaderName}");
            var material = new Material(shader) { name = name };

            WorkflowManager.SnapshotAsset(savePath, $"asset.createMaterial({name})");

            var props = p?["properties"] as JObject;
            if (props != null)
            {
                foreach (var prop in props.Properties())
                {
                    if (prop.Value.Type == JTokenType.Float || prop.Value.Type == JTokenType.Integer)
                    {
                        material.SetFloat(prop.Name, (float)prop.Value);
                    }
                    else if (prop.Value.Type == JTokenType.Object)
                    {
                        if (prop.Value["r"] != null)
                        {
                            material.SetColor(prop.Name, new Color(
                                (float)prop.Value["r"], (float)prop.Value["g"], (float)prop.Value["b"],
                                (float?)prop.Value["a"] ?? 1f));
                        }
                        else if (prop.Value["x"] != null)
                        {
                            material.SetVector(prop.Name, new Vector4(
                                (float)prop.Value["x"], (float)prop.Value["y"],
                                (float?)prop.Value["z"] ?? 0f, (float?)prop.Value["w"] ?? 0f));
                        }
                    }
                }
            }

            EnsureDirectoryExists(savePath);
            AssetDatabase.CreateAsset(material, savePath);
            AssetDatabase.SaveAssets();
            return new { success = true, path = savePath, shader = shaderName, guid = AssetDatabase.AssetPathToGUID(savePath) };
        }

        private static object CreatePrefab(JToken p)
        {
            string objectPath = Validate.Required((string)p?["objectPath"], "objectPath");
            string savePath = Validate.SafeAssetPath((string)p?["savePath"], "savePath");

            var go = GameObjectFinder.FindOrThrow(path: objectPath, name: objectPath);
            WorkflowManager.SnapshotObject(go, $"asset.createPrefab({savePath})");

            EnsureDirectoryExists(savePath);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, savePath);
            if (prefab == null) throw new McpException(-32000, $"Failed to create prefab at: {savePath}");
            return new { success = true, savePath, source = objectPath, guid = AssetDatabase.AssetPathToGUID(savePath) };
        }

        private static object ImportSettings(JToken p)
        {
            string path = Validate.SafeAssetPath((string)p?["path"], "path");
            string action = (string)p?["action"] ?? "get";
            var importer = AssetImporter.GetAtPath(path);
            if (importer == null) throw new McpException(-32003, $"No importer for: {path}");

            if (action == "get")
            {
                object settings = null;
                if (importer is TextureImporter tex)
                    settings = new { type = "TextureImporter", textureType = tex.textureType.ToString(), maxTextureSize = tex.maxTextureSize, textureCompression = tex.textureCompression.ToString(), mipmapEnabled = tex.mipmapEnabled, isReadable = tex.isReadable, filterMode = tex.filterMode.ToString(), wrapMode = tex.wrapMode.ToString() };
                else if (importer is ModelImporter mdl)
                    settings = new { type = "ModelImporter", globalScale = mdl.globalScale, importAnimation = mdl.importAnimation, materialImportMode = mdl.materialImportMode.ToString(), meshCompression = mdl.meshCompression.ToString(), isReadable = mdl.isReadable, generateColliders = mdl.addCollider };
                else if (importer is AudioImporter aud)
                {
                    var ds = aud.defaultSampleSettings;
                    settings = new { type = "AudioImporter", forceToMono = aud.forceToMono, loadInBackground = aud.loadInBackground, compressionFormat = ds.compressionFormat.ToString(), quality = ds.quality, sampleRateSetting = ds.sampleRateSetting.ToString() };
                }
                else
                    settings = new { type = importer.GetType().Name };
                return new { path, action = "get", settings };
            }

            if (action == "set")
            {
                WorkflowManager.SnapshotAsset(path, $"asset.importSettings.set({path})");
                var s = p?["settings"];
                if (importer is TextureImporter tex && s != null)
                {
                    if (s["maxTextureSize"] != null) tex.maxTextureSize = (int)s["maxTextureSize"];
                    if (s["mipmapEnabled"] != null) tex.mipmapEnabled = (bool)s["mipmapEnabled"];
                    if (s["isReadable"] != null) tex.isReadable = (bool)s["isReadable"];
                    if (s["textureType"] != null) tex.textureType = Validate.ParseEnum<TextureImporterType>((string)s["textureType"], "textureType");
                }
                else if (importer is ModelImporter mdl && s != null)
                {
                    if (s["globalScale"] != null) mdl.globalScale = (float)s["globalScale"];
                    if (s["importAnimation"] != null) mdl.importAnimation = (bool)s["importAnimation"];
                    if (s["materialImportMode"] != null) mdl.materialImportMode = Validate.ParseEnum<ModelImporterMaterialImportMode>((string)s["materialImportMode"], "materialImportMode");
                    if (s["isReadable"] != null) mdl.isReadable = (bool)s["isReadable"];
                }
                importer.SaveAndReimport();
                return new { success = true, path, action = "set" };
            }
            throw new McpException(-32602, $"Unknown action: {action}. Use 'get' or 'set'.");
        }

        private static object CreateFolder(JToken p)
        {
            string path = Validate.Required((string)p?["path"], "path");
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
            return new { success = true, path };
        }

        // ── 배치 도구 ──────────────────────────────────────────────

        private static object ImportSettingsBatch(JToken p)
        {
            var items = p?["items"] as JArray;
            return BatchExecutor.ExecuteAssetBatch(items, (item, idx) => ImportSettings(item));
        }

        // ── 확장 메서드 ─────────────────────────────────────────────

        private static object DeleteAsset(JToken p)
        {
            string path = Validate.SafeAssetPath(Validate.Required<string>(p, "path"), "path");
            bool useTrash = p?["useTrash"]?.Value<bool>() ?? true;
            WorkflowManager.SnapshotAsset(path, $"asset.delete({path})");
            bool ok = useTrash ? AssetDatabase.MoveAssetToTrash(path) : AssetDatabase.DeleteAsset(path);
            if (!ok) throw new McpException(-32000, $"Failed to delete: {path}");
            return new { success = true, path, useTrash };
        }

        private static object MoveAsset(JToken p)
        {
            string oldPath = Validate.SafeAssetPath(Validate.Required<string>(p, "oldPath"), "oldPath");
            string newPath = Validate.SafeAssetPath(Validate.Required<string>(p, "newPath"), "newPath");
            EnsureDirectoryExists(newPath);
            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(error)) throw new McpException(-32000, $"Move failed: {error}");
            return new { success = true, oldPath, newPath, guid = AssetDatabase.AssetPathToGUID(newPath) };
        }

        private static object CopyAsset(JToken p)
        {
            string sourcePath = Validate.SafeAssetPath(Validate.Required<string>(p, "sourcePath"), "sourcePath");
            string destPath = Validate.SafeAssetPath(Validate.Required<string>(p, "destPath"), "destPath");
            EnsureDirectoryExists(destPath);
            bool ok = AssetDatabase.CopyAsset(sourcePath, destPath);
            if (!ok) throw new McpException(-32000, $"Copy failed: {sourcePath} → {destPath}");
            return new { success = true, sourcePath, destPath, guid = AssetDatabase.AssetPathToGUID(destPath) };
        }

        private static object RefreshAssets(JToken p)
        {
            AssetDatabase.Refresh();
            return new { success = true, message = "AssetDatabase refreshed" };
        }

        private static object ReimportAsset(JToken p)
        {
            string path = Validate.SafeAssetPath(Validate.Required<string>(p, "path"), "path");
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            return new { success = true, path };
        }

        private static object GetLabels(JToken p)
        {
            string path = Validate.SafeAssetPath(Validate.Required<string>(p, "path"), "path");
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) throw new McpException(-32003, $"Asset not found: {path}");
            return new { path, labels = AssetDatabase.GetLabels(asset) };
        }

        private static object SetLabels(JToken p)
        {
            string path = Validate.SafeAssetPath(Validate.Required<string>(p, "path"), "path");
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) throw new McpException(-32003, $"Asset not found: {path}");
            var labels = p["labels"]?.ToObject<string[]>() ?? Array.Empty<string>();
            AssetDatabase.SetLabels(asset, labels);
            return new { success = true, path, labels };
        }

        // ── 헬퍼 ──────────────────────────────────────────────────

        private static void EnsureDirectoryExists(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(dir) || AssetDatabase.IsValidFolder(dir)) return;
            var parts = dir.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
