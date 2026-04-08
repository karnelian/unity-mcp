#if UNITY_VFX_GRAPH
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

namespace KarnelLabs.MCP
{
    public static class VFXHandler
    {
        public static void Register()
        {
            CommandRouter.Register("vfx.create", Create);
            CommandRouter.Register("vfx.getInfo", GetInfo);
            CommandRouter.Register("vfx.play", Play);
            CommandRouter.Register("vfx.stop", Stop);
            CommandRouter.Register("vfx.setFloat", SetFloat);
            CommandRouter.Register("vfx.setInt", SetInt);
            CommandRouter.Register("vfx.setBool", SetBool);
            CommandRouter.Register("vfx.setVector", SetVector);
            CommandRouter.Register("vfx.find", Find);
        }

        private static VisualEffect FindVFX(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vfx = go.GetComponent<VisualEffect>();
            if (vfx == null) throw new McpException(-32010, $"No VisualEffect on '{go.name}'");
            return vfx;
        }

        private static object Create(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "VFX";
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create VFX");

            if (p["position"] != null)
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0);

            var vfx = go.AddComponent<VisualEffect>();

            var assetPath = p["assetPath"]?.Value<string>();
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>(assetPath);
                if (asset != null) vfx.visualEffectAsset = asset;
                else throw new McpException(-32003, $"VFX asset not found: {assetPath}");
            }

            if (p["playOnAwake"] != null) vfx.playRate = p["playOnAwake"].Value<bool>() ? 1 : 0;

            return new
            {
                success = true, name = go.name, instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                hasAsset = vfx.visualEffectAsset != null,
            };
        }

        private static object GetInfo(JToken p)
        {
            var vfx = FindVFX(p);
            return new
            {
                name = vfx.gameObject.name,
                path = GameObjectFinder.GetPath(vfx.gameObject),
                hasAsset = vfx.visualEffectAsset != null,
                assetName = vfx.visualEffectAsset?.name,
                aliveParticleCount = vfx.aliveParticleCount,
                playRate = vfx.playRate,
                pause = vfx.pause,
                initialEventName = vfx.initialEventName,
                culled = vfx.culled,
            };
        }

        private static object Play(JToken p)
        {
            var vfx = FindVFX(p);
            var eventName = p["eventName"]?.Value<string>();
            if (!string.IsNullOrEmpty(eventName))
                vfx.SendEvent(eventName);
            else
                vfx.Play();
            return new { success = true, name = vfx.gameObject.name, action = "play" };
        }

        private static object Stop(JToken p)
        {
            var vfx = FindVFX(p);
            var eventName = p["eventName"]?.Value<string>();
            if (!string.IsNullOrEmpty(eventName))
                vfx.SendEvent(eventName);
            else
                vfx.Stop();
            return new { success = true, name = vfx.gameObject.name, action = "stop" };
        }

        private static object SetFloat(JToken p)
        {
            var vfx = FindVFX(p);
            var property = Validate.Required<string>(p, "property");
            var value = Validate.Required<float>(p, "value");
            if (!vfx.HasFloat(property))
                throw new McpException(-32602, $"VFX property '{property}' (float) not found. Use getInfo to list available properties.");
            Undo.RecordObject(vfx, "VFX Set Float");
            vfx.SetFloat(property, value);
            EditorUtility.SetDirty(vfx);
            return new { success = true, property, value };
        }

        private static object SetInt(JToken p)
        {
            var vfx = FindVFX(p);
            var property = Validate.Required<string>(p, "property");
            var value = Validate.Required<int>(p, "value");
            if (!vfx.HasInt(property))
                throw new McpException(-32602, $"VFX property '{property}' (int) not found. Use getInfo to list available properties.");
            Undo.RecordObject(vfx, "VFX Set Int");
            vfx.SetInt(property, value);
            EditorUtility.SetDirty(vfx);
            return new { success = true, property, value };
        }

        private static object SetBool(JToken p)
        {
            var vfx = FindVFX(p);
            var property = Validate.Required<string>(p, "property");
            var value = Validate.Required<bool>(p, "value");
            if (!vfx.HasBool(property))
                throw new McpException(-32602, $"VFX property '{property}' (bool) not found. Use getInfo to list available properties.");
            Undo.RecordObject(vfx, "VFX Set Bool");
            vfx.SetBool(property, value);
            EditorUtility.SetDirty(vfx);
            return new { success = true, property, value };
        }

        private static object SetVector(JToken p)
        {
            var vfx = FindVFX(p);
            var property = Validate.Required<string>(p, "property");
            if (!vfx.HasVector4(property))
                throw new McpException(-32602, $"VFX property '{property}' (vector) not found. Use getInfo to list available properties.");
            Undo.RecordObject(vfx, "VFX Set Vector");
            var v = p["value"];
            if (v == null) throw new McpException(-32602, "Missing 'value'");
            var vec = new Vector4(
                v["x"]?.Value<float>() ?? 0,
                v["y"]?.Value<float>() ?? 0,
                v["z"]?.Value<float>() ?? 0,
                v["w"]?.Value<float>() ?? 0);
            vfx.SetVector4(property, vec);
            EditorUtility.SetDirty(vfx);
            return new { success = true, property };
        }

        private static object Find(JToken p)
        {
            var effects = UnityEngine.Object.FindObjectsByType<VisualEffect>(FindObjectsSortMode.None);
            var nameFilter = p["nameFilter"]?.Value<string>();
            var results = effects
                .Where(v => string.IsNullOrEmpty(nameFilter) || v.gameObject.name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                .Select(v => new
                {
                    name = v.gameObject.name,
                    path = GameObjectFinder.GetPath(v.gameObject),
                    instanceId = v.gameObject.GetInstanceID(),
                    hasAsset = v.visualEffectAsset != null,
                    assetName = v.visualEffectAsset?.name,
                    aliveParticles = v.aliveParticleCount,
                }).ToArray();
            return new { count = results.Length, effects = results };
        }
    }
}
#endif
