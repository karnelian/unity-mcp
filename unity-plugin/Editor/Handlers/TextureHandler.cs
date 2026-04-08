using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class TextureHandler
    {
        public static void Register()
        {
            CommandRouter.Register("texture.getSettings", GetSettings);
            CommandRouter.Register("texture.setSettings", SetSettings);
            CommandRouter.Register("texture.getInfo", GetInfo);
            CommandRouter.Register("texture.find", Find);
            CommandRouter.Register("texture.setPlatformSettings", SetPlatformSettings);
            CommandRouter.Register("texture.setSpriteSettings", SetSpriteSettings);
            CommandRouter.Register("texture.setNormalMap", SetNormalMap);
            CommandRouter.Register("texture.resize", Resize);
            CommandRouter.Register("texture.getMemorySize", GetMemorySize);
            CommandRouter.Register("texture.setReadable", SetReadable);
        }

        private static TextureImporter GetImporter(JToken p)
        {
            var path = Validate.Required<string>(p, "texturePath");
            path = Validate.SafeAssetPath(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new McpException(-32003, $"TextureImporter not found: {path}");
            return importer;
        }

        private static object GetSettings(JToken p)
        {
            var importer = GetImporter(p);
            return new
            {
                path = importer.assetPath,
                textureType = importer.textureType.ToString(),
                maxTextureSize = importer.maxTextureSize,
                textureCompression = importer.textureCompression.ToString(),
                isReadable = importer.isReadable,
                mipmapEnabled = importer.mipmapEnabled,
                filterMode = importer.filterMode.ToString(),
                wrapMode = importer.wrapMode.ToString(),
                alphaSource = importer.alphaSource.ToString(),
                alphaIsTransparency = importer.alphaIsTransparency,
                sRGBTexture = importer.sRGBTexture,
                spriteImportMode = importer.spriteImportMode.ToString(),
                spritePixelsPerUnit = importer.spritePixelsPerUnit,
            };
        }

        private static object SetSettings(JToken p)
        {
            var importer = GetImporter(p);
            WorkflowManager.SnapshotAsset(importer.assetPath);

            if (p["textureType"] != null) importer.textureType = Validate.ParseEnum<TextureImporterType>(p["textureType"].Value<string>(), "textureType");
            if (p["maxTextureSize"] != null) importer.maxTextureSize = p["maxTextureSize"].Value<int>();
            if (p["textureCompression"] != null) importer.textureCompression = Validate.ParseEnum<TextureImporterCompression>(p["textureCompression"].Value<string>(), "textureCompression");
            if (p["isReadable"] != null) importer.isReadable = p["isReadable"].Value<bool>();
            if (p["mipmapEnabled"] != null) importer.mipmapEnabled = p["mipmapEnabled"].Value<bool>();
            if (p["filterMode"] != null) importer.filterMode = Validate.ParseEnum<FilterMode>(p["filterMode"].Value<string>(), "filterMode");
            if (p["wrapMode"] != null) importer.wrapMode = Validate.ParseEnum<TextureWrapMode>(p["wrapMode"].Value<string>(), "wrapMode");
            if (p["sRGBTexture"] != null) importer.sRGBTexture = p["sRGBTexture"].Value<bool>();
            if (p["alphaIsTransparency"] != null) importer.alphaIsTransparency = p["alphaIsTransparency"].Value<bool>();

            importer.SaveAndReimport();
            return GetSettings(p);
        }

        private static object GetInfo(JToken p)
        {
            var path = Validate.Required<string>(p, "texturePath");
            path = Validate.SafeAssetPath(path);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) throw new McpException(-32003, $"Texture not found: {path}");

            return new
            {
                name = tex.name,
                path,
                width = tex.width,
                height = tex.height,
                format = tex.format.ToString(),
                mipmapCount = tex.mipmapCount,
                isReadable = tex.isReadable,
                filterMode = tex.filterMode.ToString(),
                wrapMode = tex.wrapMode.ToString(),
            };
        }

        private static object Find(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var nameFilter = p["nameFilter"]?.Value<string>();

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            var results = new List<object>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(nameFilter) && !name.ToLower().Contains(nameFilter.ToLower())) continue;
                results.Add(new { name, path, guid });
            }
            return new { count = results.Count, textures = results };
        }

        private static object SetPlatformSettings(JToken p)
        {
            var importer = GetImporter(p);
            var platform = Validate.Required<string>(p, "platform");
            WorkflowManager.SnapshotAsset(importer.assetPath);

            var settings = importer.GetPlatformTextureSettings(platform);
            settings.overridden = true;
            if (p["maxTextureSize"] != null) settings.maxTextureSize = p["maxTextureSize"].Value<int>();
            if (p["format"] != null) settings.format = Validate.ParseEnum<TextureImporterFormat>(p["format"].Value<string>(), "format");
            if (p["compressionQuality"] != null) settings.compressionQuality = p["compressionQuality"].Value<int>();

            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
            return new { platform, maxTextureSize = settings.maxTextureSize, format = settings.format.ToString() };
        }

        private static object SetSpriteSettings(JToken p)
        {
            var importer = GetImporter(p);
            WorkflowManager.SnapshotAsset(importer.assetPath);

            importer.textureType = TextureImporterType.Sprite;
            if (p["spriteImportMode"] != null) importer.spriteImportMode = Validate.ParseEnum<SpriteImportMode>(p["spriteImportMode"].Value<string>(), "spriteImportMode");
            if (p["pixelsPerUnit"] != null) importer.spritePixelsPerUnit = p["pixelsPerUnit"].Value<float>();
            if (p["pivot"] != null)
            {
                var pivot = p["pivot"];
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spritePivot = new Vector2(pivot["x"]?.Value<float>() ?? 0.5f, pivot["y"]?.Value<float>() ?? 0.5f);
                importer.SetTextureSettings(settings);
            }

            importer.SaveAndReimport();
            return new { path = importer.assetPath, spriteImportMode = importer.spriteImportMode.ToString(), pixelsPerUnit = importer.spritePixelsPerUnit };
        }

        private static object SetNormalMap(JToken p)
        {
            var importer = GetImporter(p);
            WorkflowManager.SnapshotAsset(importer.assetPath);

            importer.textureType = TextureImporterType.NormalMap;
            if (p["bumpScale"] != null)
            {
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.heightmapScale = p["bumpScale"].Value<float>();
                importer.SetTextureSettings(settings);
            }

            importer.SaveAndReimport();
            return new { path = importer.assetPath, textureType = "NormalMap" };
        }

        private static object Resize(JToken p)
        {
            var importer = GetImporter(p);
            var maxSize = Validate.Required<int>(p, "maxTextureSize");
            WorkflowManager.SnapshotAsset(importer.assetPath);

            importer.maxTextureSize = maxSize;
            importer.SaveAndReimport();
            return new { path = importer.assetPath, maxTextureSize = maxSize };
        }

        private static object GetMemorySize(JToken p)
        {
            var path = Validate.Required<string>(p, "texturePath");
            path = Validate.SafeAssetPath(path);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null) throw new McpException(-32003, $"Texture not found: {path}");

            var runtimeMemory = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);
            return new
            {
                name = tex.name,
                path,
                width = tex.width,
                height = tex.height,
                runtimeMemoryBytes = runtimeMemory,
                runtimeMemoryMB = Math.Round(runtimeMemory / (1024.0 * 1024.0), 2),
            };
        }

        private static object SetReadable(JToken p)
        {
            var importer = GetImporter(p);
            var readable = p["readable"]?.Value<bool>() ?? true;
            WorkflowManager.SnapshotAsset(importer.assetPath);

            importer.isReadable = readable;
            importer.SaveAndReimport();
            return new { path = importer.assetPath, isReadable = readable };
        }
    }
}
