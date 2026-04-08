using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class OptimizationHandler
    {
        public static void Register()
        {
            CommandRouter.Register("optimize.textureOverview", TextureOverview);
            CommandRouter.Register("optimize.compressTextures", CompressTextures);
            CommandRouter.Register("optimize.meshOverview", MeshOverview);
            CommandRouter.Register("optimize.enableMeshCompression", EnableMeshCompression);
            CommandRouter.Register("optimize.audioOverview", AudioOverview);
            CommandRouter.Register("optimize.compressAudio", CompressAudio);
            CommandRouter.Register("optimize.findLargeAssets", FindLargeAssets);
            CommandRouter.Register("optimize.setStaticFlags", SetStaticFlags);
            CommandRouter.Register("optimize.disableMipmaps", DisableMipmaps);
            CommandRouter.Register("optimize.enableGPUInstancing", EnableGPUInstancing);
        }

        private static object TextureOverview(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            long totalSize = 0;
            int oversized = 0, readable = 0, noCompression = 0;
            var textures = new List<object>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                var fileSize = new FileInfo(Path.Combine(Application.dataPath, "..", path)).Length;
                totalSize += fileSize;
                if (importer.maxTextureSize > 2048) oversized++;
                if (importer.isReadable) readable++;
                if (importer.textureCompression == TextureImporterCompression.Uncompressed) noCompression++;
            }

            return new
            {
                totalTextures = guids.Length,
                totalSizeMB = Math.Round(totalSize / (1024.0 * 1024.0), 2),
                oversized,
                readableEnabled = readable,
                uncompressed = noCompression,
                suggestions = new[]
                {
                    oversized > 0 ? $"{oversized} textures exceed 2048px — consider reducing" : null,
                    readable > 0 ? $"{readable} textures have Read/Write enabled — disable if not needed" : null,
                    noCompression > 0 ? $"{noCompression} textures are uncompressed — enable compression" : null,
                }.Where(s => s != null).ToArray(),
            };
        }

        private static object CompressTextures(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var maxSize = p["maxTextureSize"]?.Value<int>() ?? 2048;
            var compression = p["compression"]?.Value<string>() ?? "CompressedHQ";
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            int modified = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                bool changed = false;
                if (importer.maxTextureSize > maxSize) { importer.maxTextureSize = maxSize; changed = true; }
                if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = Validate.ParseEnum<TextureImporterCompression>(compression, "compression");
                    changed = true;
                }
                if (changed) { importer.SaveAndReimport(); modified++; }
            }
            return new { scanned = guids.Length, modified, maxSize, compression };
        }

        private static object MeshOverview(JToken p)
        {
            var guids = AssetDatabase.FindAssets("t:Model");
            int totalModels = 0, readableCount = 0, uncompressedCount = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;
                totalModels++;
                if (importer.isReadable) readableCount++;
                if (importer.meshCompression == ModelImporterMeshCompression.Off) uncompressedCount++;
            }

            return new
            {
                totalModels,
                readableEnabled = readableCount,
                uncompressed = uncompressedCount,
                suggestions = new[]
                {
                    readableCount > 0 ? $"{readableCount} models have Read/Write enabled" : null,
                    uncompressedCount > 0 ? $"{uncompressedCount} models have no mesh compression" : null,
                }.Where(s => s != null).ToArray(),
            };
        }

        private static object EnableMeshCompression(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var level = p["level"]?.Value<string>() ?? "Medium";
            var guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
            int modified = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null || importer.meshCompression != ModelImporterMeshCompression.Off) continue;
                importer.meshCompression = Validate.ParseEnum<ModelImporterMeshCompression>(level, "level");
                importer.SaveAndReimport();
                modified++;
            }
            return new { scanned = guids.Length, modified, level };
        }

        private static object AudioOverview(JToken p)
        {
            var guids = AssetDatabase.FindAssets("t:AudioClip");
            int total = 0, uncompressed = 0, forceToMono = 0;
            long totalSize = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;
                total++;
                var settings = importer.defaultSampleSettings;
                if (settings.compressionFormat == AudioCompressionFormat.PCM) uncompressed++;
                if (importer.forceToMono) forceToMono++;
                try { totalSize += new FileInfo(Path.Combine(Application.dataPath, "..", path)).Length; } catch { }
            }

            return new
            {
                totalClips = total,
                totalSizeMB = Math.Round(totalSize / (1024.0 * 1024.0), 2),
                uncompressed,
                forcedToMono = forceToMono,
            };
        }

        private static object CompressAudio(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var format = p["format"]?.Value<string>() ?? "Vorbis";
            var quality = p["quality"]?.Value<float>() ?? 0.5f;
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
            int modified = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;
                var settings = importer.defaultSampleSettings;
                if (settings.compressionFormat == AudioCompressionFormat.PCM)
                {
                    settings.compressionFormat = Validate.ParseEnum<AudioCompressionFormat>(format, "format");
                    settings.quality = quality;
                    importer.defaultSampleSettings = settings;
                    importer.SaveAndReimport();
                    modified++;
                }
            }
            return new { scanned = guids.Length, modified, format, quality };
        }

        private static object FindLargeAssets(JToken p)
        {
            var thresholdMB = p["thresholdMB"]?.Value<double>() ?? 10.0;
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var thresholdBytes = (long)(thresholdMB * 1024 * 1024);
            var results = new List<(string path, double sizeMB, string extension)>();
            var allPaths = AssetDatabase.GetAllAssetPaths().Where(pa => pa.StartsWith(folder));

            foreach (var path in allPaths)
            {
                try
                {
                    var fullPath = Path.Combine(Application.dataPath, "..", path);
                    if (!File.Exists(fullPath)) continue;
                    var size = new FileInfo(fullPath).Length;
                    if (size >= thresholdBytes)
                    {
                        results.Add((path, Math.Round(size / (1024.0 * 1024.0), 2), Path.GetExtension(path)));
                    }
                }
                catch { }
            }

            return new { thresholdMB, count = results.Count, assets = results.OrderByDescending(x => x.sizeMB).Take(50).Select(x => new { x.path, x.sizeMB, x.extension }).ToArray() };
        }

        private static object SetStaticFlags(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var recursive = p["recursive"]?.Value<bool>() ?? false;
            var flags = p["flags"]?.Value<string>();
            WorkflowManager.SnapshotObject(go);

            StaticEditorFlags newFlags;
            if (string.IsNullOrEmpty(flags))
                newFlags = StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic;
            else
                newFlags = Validate.ParseEnum<StaticEditorFlags>(flags, "flags");

            GameObjectUtility.SetStaticEditorFlags(go, newFlags);
            if (recursive)
            {
                foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.SetStaticEditorFlags(child.gameObject, newFlags);
            }

            return new { name = go.name, path = GameObjectFinder.GetPath(go), staticFlags = newFlags.ToString(), recursive };
        }

        private static object DisableMipmaps(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var onlyUI = p["onlyUI"]?.Value<bool>() ?? false;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            int modified = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || !importer.mipmapEnabled) continue;
                if (onlyUI && importer.textureType != TextureImporterType.Sprite) continue;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
                modified++;
            }
            return new { scanned = guids.Length, modified };
        }

        private static object EnableGPUInstancing(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var guids = AssetDatabase.FindAssets("t:Material", new[] { folder });
            int modified = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.enableInstancing) continue;
                if (mat.shader != null && mat.shader.name != "Hidden/InternalErrorShader")
                {
                    mat.enableInstancing = true;
                    EditorUtility.SetDirty(mat);
                    modified++;
                }
            }
            AssetDatabase.SaveAssets();
            return new { scanned = guids.Length, modified };
        }
    }
}
