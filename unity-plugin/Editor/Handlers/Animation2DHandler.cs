using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace KarnelLabs.MCP
{
    public static class Animation2DHandler
    {
        public static void Register()
        {
            CommandRouter.Register("animation2d.createClip", CreateClip);
            CommandRouter.Register("animation2d.getClipInfo", GetClipInfo);
            CommandRouter.Register("animation2d.findClips", FindClips);
            CommandRouter.Register("animation2d.setSpriteKeyframes", SetSpriteKeyframes);
            CommandRouter.Register("animation2d.setClipSettings", SetClipSettings);
            CommandRouter.Register("animation2d.createSpriteAtlas", CreateSpriteAtlas);
            CommandRouter.Register("animation2d.getSpriteAtlasInfo", GetSpriteAtlasInfo);
            CommandRouter.Register("animation2d.addToSpriteAtlas", AddToSpriteAtlas);
            CommandRouter.Register("animation2d.removeFromSpriteAtlas", RemoveFromSpriteAtlas);
            CommandRouter.Register("animation2d.sliceSprite", SliceSprite);
        }

        private static object CreateClip(JToken p)
        {
            string savePath = (string)p["savePath"] ?? "Assets/Animations/NewClip.anim";
            float sampleRate = p["sampleRate"]?.Value<float>() ?? 12f;
            bool loop = p["loop"]?.Value<bool>() ?? true;

            if (!savePath.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
                savePath += ".anim";

            var dir = System.IO.Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var clip = new AnimationClip { frameRate = sampleRate };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AssetDatabase.CreateAsset(clip, savePath);
            AssetDatabase.SaveAssets();

            return new { path = savePath, sampleRate, loop };
        }

        private static object GetClipInfo(JToken p)
        {
            string path = (string)p["path"];
            if (string.IsNullOrEmpty(path))
                throw new McpException(-32602, "path is required");

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                throw new McpException(-32602, $"AnimationClip not found: {path}");

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

            return new
            {
                name = clip.name,
                path,
                length = clip.length,
                frameRate = clip.frameRate,
                frameCount = Mathf.RoundToInt(clip.length * clip.frameRate),
                loop = settings.loopTime,
                isHumanMotion = clip.humanMotion,
                wrapMode = clip.wrapMode.ToString(),
                curveBindings = bindings.Select(b => new { b.path, b.propertyName, type = b.type.Name }).ToArray(),
                objectBindings = objectBindings.Select(b => new { b.path, b.propertyName, type = b.type.Name }).ToArray(),
            };
        }

        private static object FindClips(JToken p)
        {
            string nameFilter = (string)p?["nameFilter"] ?? "";
            var guids = AssetDatabase.FindAssets($"t:AnimationClip {nameFilter}");
            var clips = guids.Take(100).Select(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                return clip == null ? null : new
                {
                    name = clip.name,
                    path,
                    length = clip.length,
                    frameRate = clip.frameRate,
                    loop = AnimationUtility.GetAnimationClipSettings(clip).loopTime
                };
            }).Where(c => c != null).ToArray();

            return new { count = clips.Length, clips };
        }

        private static object SetSpriteKeyframes(JToken p)
        {
            string clipPath = (string)p["clipPath"];
            if (string.IsNullOrEmpty(clipPath))
                throw new McpException(-32602, "clipPath is required");

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
                throw new McpException(-32602, $"AnimationClip not found: {clipPath}");

            var spritePaths = p["spritePaths"]?.ToObject<string[]>();
            if (spritePaths == null || spritePaths.Length == 0)
                throw new McpException(-32602, "spritePaths array is required");

            string bindingPath = (string)p?["gameObjectPath"] ?? "";

            var binding = EditorCurveBinding.PPtrCurve(bindingPath, typeof(SpriteRenderer), "m_Sprite");

            var keyframes = new ObjectReferenceKeyframe[spritePaths.Length];
            for (int i = 0; i < spritePaths.Length; i++)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
                if (sprite == null)
                    throw new McpException(-32602, $"Sprite not found: {spritePaths[i]}");

                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = sprite
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            return new { clipPath, spriteCount = spritePaths.Length, duration = (spritePaths.Length - 1) / clip.frameRate };
        }

        private static object SetClipSettings(JToken p)
        {
            string path = (string)p["path"];
            if (string.IsNullOrEmpty(path))
                throw new McpException(-32602, "path is required");

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                throw new McpException(-32602, $"AnimationClip not found: {path}");

            var settings = AnimationUtility.GetAnimationClipSettings(clip);

            if (p["loop"] != null) settings.loopTime = p["loop"].Value<bool>();
            if (p["sampleRate"] != null) clip.frameRate = p["sampleRate"].Value<float>();

            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            return new { path, loop = settings.loopTime, sampleRate = clip.frameRate };
        }

        private static object CreateSpriteAtlas(JToken p)
        {
            string savePath = (string)p["savePath"] ?? "Assets/SpriteAtlases/NewAtlas.spriteatlas";
            if (!savePath.EndsWith(".spriteatlas", StringComparison.OrdinalIgnoreCase))
                savePath += ".spriteatlas";

            var dir = System.IO.Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var atlas = new SpriteAtlas();

            var packSettings = new SpriteAtlasPackingSettings
            {
                enableRotation = p["enableRotation"]?.Value<bool>() ?? false,
                enableTightPacking = p["enableTightPacking"]?.Value<bool>() ?? false,
                padding = p["padding"]?.Value<int>() ?? 4,
            };
            atlas.SetPackingSettings(packSettings);

            AssetDatabase.CreateAsset(atlas, savePath);
            AssetDatabase.SaveAssets();

            return new { path = savePath };
        }

        private static object GetSpriteAtlasInfo(JToken p)
        {
            string path = (string)p["path"];
            if (string.IsNullOrEmpty(path))
                throw new McpException(-32602, "path is required");

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (atlas == null)
                throw new McpException(-32602, $"SpriteAtlas not found: {path}");

            var packSettings = atlas.GetPackingSettings();
            var texSettings = atlas.GetTextureSettings();

            return new
            {
                name = atlas.name,
                path,
                spriteCount = atlas.spriteCount,
                isVariant = atlas.isVariant,
                packingSettings = new
                {
                    enableRotation = packSettings.enableRotation,
                    enableTightPacking = packSettings.enableTightPacking,
                    padding = packSettings.padding,
                },
                textureSettings = new
                {
                    readable = texSettings.readable,
                    generateMipMaps = texSettings.generateMipMaps,
                    filterMode = texSettings.filterMode.ToString(),
                },
            };
        }

        private static object AddToSpriteAtlas(JToken p)
        {
            string atlasPath = (string)p["atlasPath"];
            if (string.IsNullOrEmpty(atlasPath))
                throw new McpException(-32602, "atlasPath is required");

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
                throw new McpException(-32602, $"SpriteAtlas not found: {atlasPath}");

            var assetPaths = p["assetPaths"]?.ToObject<string[]>();
            if (assetPaths == null || assetPaths.Length == 0)
                throw new McpException(-32602, "assetPaths array is required");

            var objects = assetPaths.Select(ap =>
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ap);
                if (obj == null) throw new McpException(-32602, $"Asset not found: {ap}");
                return obj;
            }).ToArray();

            atlas.Add(objects);
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();

            return new { atlasPath, addedCount = objects.Length, totalSprites = atlas.spriteCount };
        }

        private static object RemoveFromSpriteAtlas(JToken p)
        {
            string atlasPath = (string)p["atlasPath"];
            if (string.IsNullOrEmpty(atlasPath))
                throw new McpException(-32602, "atlasPath is required");

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
                throw new McpException(-32602, $"SpriteAtlas not found: {atlasPath}");

            var assetPaths = p["assetPaths"]?.ToObject<string[]>();
            if (assetPaths == null || assetPaths.Length == 0)
                throw new McpException(-32602, "assetPaths array is required");

            var objects = assetPaths.Select(ap =>
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ap);
                if (obj == null) throw new McpException(-32602, $"Asset not found: {ap}");
                return obj;
            }).ToArray();

            atlas.Remove(objects);
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();

            return new { atlasPath, removedCount = objects.Length, totalSprites = atlas.spriteCount };
        }

#pragma warning disable CS0618 // spritesheet is deprecated but ISpriteEditorDataProvider requires extra asmdef
        private static object SliceSprite(JToken p)
        {
            string texturePath = (string)p["texturePath"];
            if (string.IsNullOrEmpty(texturePath))
                throw new McpException(-32602, "texturePath is required");

            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null)
                throw new McpException(-32602, $"TextureImporter not found: {texturePath}");

            string mode = (string)p?["mode"] ?? "grid";
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;

            if (mode == "grid")
            {
                int cellWidth = p["cellWidth"]?.Value<int>() ?? 32;
                int cellHeight = p["cellHeight"]?.Value<int>() ?? 32;
                int padding = p["padding"]?.Value<int>() ?? 0;
                string namePrefix = (string)p?["namePrefix"] ?? System.IO.Path.GetFileNameWithoutExtension(texturePath);

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                    throw new McpException(-32602, $"Texture not found: {texturePath}");

                int texWidth = texture.width;
                int texHeight = texture.height;

                int cols = texWidth / (cellWidth + padding);
                int rows = texHeight / (cellHeight + padding);
                var spritesheet = new SpriteMetaData[cols * rows];
                int index = 0;

                for (int row = rows - 1; row >= 0; row--)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        spritesheet[index] = new SpriteMetaData
                        {
                            name = $"{namePrefix}_{index}",
                            rect = new Rect(col * (cellWidth + padding), row * (cellHeight + padding), cellWidth, cellHeight),
                            alignment = (int)SpriteAlignment.Center,
                            pivot = new Vector2(0.5f, 0.5f),
                        };
                        index++;
                    }
                }

                importer.spritesheet = spritesheet;
            }

            importer.SaveAndReimport();

            var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>().Select(s => s.name).ToArray();

            return new { texturePath, mode, spriteCount = sprites.Length, sprites };
        }
#pragma warning restore CS0618
    }
}
