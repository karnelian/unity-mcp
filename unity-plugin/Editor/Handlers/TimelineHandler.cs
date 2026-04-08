using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    /// <summary>
    /// Animation Clip 기반 타임라인 조작.
    /// Unity Timeline 패키지(com.unity.timeline)가 없는 프로젝트에서도 동작하도록
    /// AnimationClip + AnimationWindow 기반으로 구현.
    /// </summary>
    public static class TimelineHandler
    {
        public static void Register()
        {
            CommandRouter.Register("timeline.createClip", CreateClip);
            CommandRouter.Register("timeline.getClipInfo", GetClipInfo);
            CommandRouter.Register("timeline.setCurve", SetCurve);
            CommandRouter.Register("timeline.getCurves", GetCurves);
            CommandRouter.Register("timeline.addEvent", AddEvent);
            CommandRouter.Register("timeline.getEvents", GetEvents);
            CommandRouter.Register("timeline.setClipSettings", SetClipSettings);
            CommandRouter.Register("timeline.duplicateClip", DuplicateClip);
            CommandRouter.Register("timeline.findClips", FindClips);
            CommandRouter.Register("timeline.deleteKey", DeleteKey);
        }

        private static AnimationClip LoadClip(JToken p)
        {
            var path = Validate.Required<string>(p, "clipPath");
            path = Validate.SafeAssetPath(path);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) throw new McpException(-32003, $"AnimationClip not found: {path}");
            return clip;
        }

        private static object CreateClip(JToken p)
        {
            var savePath = Validate.Required<string>(p, "savePath");
            savePath = Validate.SafeAssetPath(savePath);
            var clip = new AnimationClip();
            clip.name = p["name"]?.Value<string>() ?? System.IO.Path.GetFileNameWithoutExtension(savePath);

            if (p["frameRate"] != null) clip.frameRate = p["frameRate"].Value<float>();
            if (p["wrapMode"] != null) clip.wrapMode = Validate.ParseEnum<WrapMode>(p["wrapMode"].Value<string>(), "wrapMode");

            var dir = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(Application.dataPath, "..", savePath));
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(clip, savePath);
            AssetDatabase.SaveAssets();
            return new { path = savePath, name = clip.name, frameRate = clip.frameRate };
        }

        private static object GetClipInfo(JToken p)
        {
            var clip = LoadClip(p);
            var bindings = AnimationUtility.GetCurveBindings(clip);
            return new
            {
                name = clip.name,
                path = AssetDatabase.GetAssetPath(clip),
                length = clip.length,
                frameRate = clip.frameRate,
                isLooping = clip.isLooping,
                wrapMode = clip.wrapMode.ToString(),
                curveCount = bindings.Length,
                eventCount = AnimationUtility.GetAnimationEvents(clip).Length,
                curves = bindings.Select(b => new { b.path, b.propertyName, type = b.type.Name }).Take(50).ToArray(),
            };
        }

        private static object SetCurve(JToken p)
        {
            var clip = LoadClip(p);
            var propertyPath = Validate.Required<string>(p, "propertyName");
            var componentType = Validate.Required<string>(p, "componentType");
            var objectPath = p["objectPath"]?.Value<string>() ?? "";

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(clip));

            var type = TypeCache.GetTypesDerivedFrom<Component>()
                .FirstOrDefault(t => t.Name == componentType || t.FullName == componentType);
            if (type == null) type = typeof(Transform);

            var keysData = p["keys"]?.ToObject<float[][]>();
            if (keysData == null || keysData.Length == 0)
                throw new McpException(-32602, "keys required: array of [time, value] pairs");

            var keys = keysData.Select(k => new Keyframe(k[0], k[1])).ToArray();
            var curve = new AnimationCurve(keys);

            clip.SetCurve(objectPath, type, propertyPath, curve);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            return new { clip = clip.name, propertyName = propertyPath, keyCount = keys.Length };
        }

        private static object GetCurves(JToken p)
        {
            var clip = LoadClip(p);
            var bindings = AnimationUtility.GetCurveBindings(clip);
            var curves = bindings.Select(b =>
            {
                var curve = AnimationUtility.GetEditorCurve(clip, b);
                return new
                {
                    path = b.path,
                    propertyName = b.propertyName,
                    type = b.type.Name,
                    keyCount = curve?.keys.Length ?? 0,
                    keys = curve?.keys.Select(k => new { k.time, k.value, k.inTangent, k.outTangent }).Take(100).ToArray(),
                };
            }).ToArray();

            return new { clip = clip.name, curveCount = curves.Length, curves };
        }

        private static object AddEvent(JToken p)
        {
            var clip = LoadClip(p);
            var time = Validate.Required<float>(p, "time");
            var functionName = Validate.Required<string>(p, "functionName");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(clip));

            var events = AnimationUtility.GetAnimationEvents(clip).ToList();
            var evt = new AnimationEvent
            {
                time = time,
                functionName = functionName,
            };
            if (p["stringParameter"] != null) evt.stringParameter = p["stringParameter"].Value<string>();
            if (p["intParameter"] != null) evt.intParameter = p["intParameter"].Value<int>();
            if (p["floatParameter"] != null) evt.floatParameter = p["floatParameter"].Value<float>();

            events.Add(evt);
            AnimationUtility.SetAnimationEvents(clip, events.ToArray());
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            return new { clip = clip.name, eventCount = events.Count, added = functionName, time };
        }

        private static object GetEvents(JToken p)
        {
            var clip = LoadClip(p);
            var events = AnimationUtility.GetAnimationEvents(clip);
            return new
            {
                clip = clip.name,
                eventCount = events.Length,
                events = events.Select(e => new
                {
                    time = e.time,
                    functionName = e.functionName,
                    stringParameter = e.stringParameter,
                    intParameter = e.intParameter,
                    floatParameter = e.floatParameter,
                }).ToArray(),
            };
        }

        private static object SetClipSettings(JToken p)
        {
            var clip = LoadClip(p);
            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(clip));

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (p["loopTime"] != null) settings.loopTime = p["loopTime"].Value<bool>();
            if (p["loopBlend"] != null) settings.loopBlend = p["loopBlend"].Value<bool>();
            if (p["keepOriginalOrientation"] != null) settings.keepOriginalOrientation = p["keepOriginalOrientation"].Value<bool>();
            if (p["keepOriginalPositionXZ"] != null) settings.keepOriginalPositionXZ = p["keepOriginalPositionXZ"].Value<bool>();
            if (p["keepOriginalPositionY"] != null) settings.keepOriginalPositionY = p["keepOriginalPositionY"].Value<bool>();

            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            return new { clip = clip.name, loopTime = settings.loopTime, loopBlend = settings.loopBlend };
        }

        private static object DuplicateClip(JToken p)
        {
            var clip = LoadClip(p);
            var srcPath = AssetDatabase.GetAssetPath(clip);
            var newName = p["newName"]?.Value<string>() ?? clip.name + "_Copy";
            var dir = System.IO.Path.GetDirectoryName(srcPath);
            var destPath = System.IO.Path.Combine(dir, newName + ".anim").Replace("\\", "/");

            AssetDatabase.CopyAsset(srcPath, destPath);
            AssetDatabase.SaveAssets();
            return new { source = srcPath, destination = destPath, name = newName };
        }

        private static object FindClips(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var nameFilter = p["nameFilter"]?.Value<string>();
            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folder });
            var results = new List<object>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null || clip.name.StartsWith("__preview__")) continue;
                if (!string.IsNullOrEmpty(nameFilter) && !clip.name.ToLower().Contains(nameFilter.ToLower())) continue;
                results.Add(new { name = clip.name, path, length = clip.length, frameRate = clip.frameRate });
            }
            return new { count = results.Count, clips = results };
        }

        private static object DeleteKey(JToken p)
        {
            var clip = LoadClip(p);
            var propertyName = Validate.Required<string>(p, "propertyName");
            var objectPath = p["objectPath"]?.Value<string>() ?? "";
            var keyIndex = Validate.Required<int>(p, "keyIndex");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(clip));

            var bindings = AnimationUtility.GetCurveBindings(clip);
            var binding = bindings.FirstOrDefault(b => b.propertyName == propertyName && b.path == objectPath);
            if (binding.propertyName == null)
                throw new McpException(-32003, $"Curve not found: {objectPath}/{propertyName}");

            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            if (keyIndex < 0 || keyIndex >= curve.keys.Length)
                throw new McpException(-32602, $"Key index {keyIndex} out of range (0-{curve.keys.Length - 1})");

            curve.RemoveKey(keyIndex);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            return new { clip = clip.name, propertyName, removedIndex = keyIndex, remainingKeys = curve.keys.Length };
        }
    }
}
