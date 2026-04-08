using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KarnelLabs.MCP
{
    public static class LightHandler
    {
        public static void Register()
        {
            CommandRouter.Register("light.create", Create);
            CommandRouter.Register("light.setProperties", SetProperties);
            CommandRouter.Register("light.getProperties", GetProperties);
            CommandRouter.Register("light.find", FindLights);
            CommandRouter.Register("light.setActive", SetActive);
            CommandRouter.Register("light.delete", Delete);
            CommandRouter.Register("light.setLightmapBakeType", SetLightmapBakeType);
            CommandRouter.Register("light.setShadows", SetShadows);
            CommandRouter.Register("light.setAmbient", SetAmbient);
            CommandRouter.Register("light.getAmbient", GetAmbient);
            CommandRouter.Register("light.setColorTemperature", SetColorTemperature);
            CommandRouter.Register("light.setCookie", SetCookie);
            CommandRouter.Register("light.setCullingMask", SetCullingMask);
            CommandRouter.Register("light.createReflectionProbe", CreateReflectionProbe);
            CommandRouter.Register("light.setReflectionProbe", SetReflectionProbe);
            CommandRouter.Register("light.findReflectionProbes", FindReflectionProbes);
            CommandRouter.Register("light.createLightProbeGroup", CreateLightProbeGroup);
        }

        private static Color ParseColor(JToken c)
        {
            if (c == null) return Color.white;
            return new Color(
                c["r"]?.Value<float>() ?? 1f, c["g"]?.Value<float>() ?? 1f,
                c["b"]?.Value<float>() ?? 1f, c["a"]?.Value<float>() ?? 1f
            );
        }

        private static object LightInfo(Light light)
        {
            var go = light.gameObject;
            return new
            {
                name = go.name,
                instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                type = light.type.ToString(),
                color = new { light.color.r, light.color.g, light.color.b, light.color.a },
                intensity = light.intensity,
                range = light.range,
                spotAngle = light.spotAngle,
                shadows = light.shadows.ToString(),
                lightmapBakeType = light.lightmapBakeType.ToString(),
                enabled = light.enabled,
                active = go.activeInHierarchy,
            };
        }

        private static object Create(JToken p)
        {
            var lightName = p["name"]?.Value<string>() ?? "New Light";
            var typeStr = p["type"]?.Value<string>() ?? "Point";
            var lightType = Validate.ParseEnum<LightType>(typeStr, "type");

            var go = new GameObject(lightName);
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Light");
            var light = go.AddComponent<Light>();
            light.type = lightType;

            if (p["color"] != null) light.color = ParseColor(p["color"]);
            if (p["intensity"] != null) light.intensity = p["intensity"].Value<float>();
            if (p["range"] != null) light.range = p["range"].Value<float>();
            if (p["spotAngle"] != null) light.spotAngle = p["spotAngle"].Value<float>();

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0
                );
            }
            if (p["rotation"] != null)
            {
                go.transform.eulerAngles = new Vector3(
                    p["rotation"]["x"]?.Value<float>() ?? 0,
                    p["rotation"]["y"]?.Value<float>() ?? 0,
                    p["rotation"]["z"]?.Value<float>() ?? 0
                );
            }

            var parentPath = p["parent"]?.Value<string>();
            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = GameObject.Find(parentPath);
                if (parent != null) go.transform.SetParent(parent.transform, true);
            }

            return LightInfo(light);
        }

        private static object SetProperties(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var light = go.GetComponent<Light>();
            if (light == null) throw new McpException(-32602, $"No Light component on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(light, "MCP: Set Light Properties");

            if (p["color"] != null) light.color = ParseColor(p["color"]);
            if (p["intensity"] != null) light.intensity = p["intensity"].Value<float>();
            if (p["range"] != null) light.range = p["range"].Value<float>();
            if (p["spotAngle"] != null) light.spotAngle = p["spotAngle"].Value<float>();
            if (p["type"] != null) light.type = Validate.ParseEnum<LightType>(p["type"].Value<string>(), "type");
            if (p["enabled"] != null) light.enabled = p["enabled"].Value<bool>();

            return LightInfo(light);
        }

        private static object GetProperties(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var light = go.GetComponent<Light>();
            if (light == null) throw new McpException(-32602, $"No Light component on '{go.name}'");
            return LightInfo(light);
        }

        private static object FindLights(JToken p)
        {
            var typeFilter = p["type"]?.Value<string>();
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            IEnumerable<Light> filtered = lights;

            if (!string.IsNullOrEmpty(typeFilter))
            {
                var lt = Validate.ParseEnum<LightType>(typeFilter, "type");
                filtered = filtered.Where(l => l.type == lt);
            }

            return new { lights = filtered.Select(LightInfo).ToArray() };
        }

        private static object SetActive(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var active = p["active"]?.Value<bool>() ?? true;
            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(go, "MCP: Set Light Active");
            go.SetActive(active);
            return new { name = go.name, active };
        }

        private static object Delete(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (go.GetComponent<Light>() == null)
                throw new McpException(-32602, $"'{go.name}' has no Light component");
            var name = go.name;
            Undo.DestroyObjectImmediate(go);
            return new { deleted = true, name };
        }

        private static object SetLightmapBakeType(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var light = go.GetComponent<Light>();
            if (light == null) throw new McpException(-32602, $"No Light on '{go.name}'");
            var bakeType = Validate.Required<string>(p, "bakeType");
            var mode = Validate.ParseEnum<LightmapBakeType>(bakeType, "bakeType");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(light, "MCP: Set Lightmap Bake Type");
            light.lightmapBakeType = mode;
            return new { name = go.name, lightmapBakeType = mode.ToString() };
        }

        private static object SetShadows(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var light = go.GetComponent<Light>();
            if (light == null) throw new McpException(-32602, $"No Light on '{go.name}'");
            var shadowType = Validate.Required<string>(p, "shadowType");
            var shadows = Validate.ParseEnum<LightShadows>(shadowType, "shadowType");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(light, "MCP: Set Light Shadows");
            light.shadows = shadows;
            if (p["strength"] != null) light.shadowStrength = p["strength"].Value<float>();
            if (p["resolution"] != null) light.shadowResolution = Validate.ParseEnum<LightShadowResolution>(p["resolution"].Value<string>(), "resolution");
            if (p["bias"] != null) light.shadowBias = p["bias"].Value<float>();
            if (p["normalBias"] != null) light.shadowNormalBias = p["normalBias"].Value<float>();

            return new { name = go.name, shadows = light.shadows.ToString(), strength = light.shadowStrength };
        }

        private static object SetAmbient(JToken p)
        {
            var modeStr = p["mode"]?.Value<string>();
            if (!string.IsNullOrEmpty(modeStr))
                RenderSettings.ambientMode = Validate.ParseEnum<AmbientMode>(modeStr, "mode");

            if (p["color"] != null) RenderSettings.ambientLight = ParseColor(p["color"]);
            if (p["intensity"] != null) RenderSettings.ambientIntensity = p["intensity"].Value<float>();
            if (p["skyColor"] != null) RenderSettings.ambientSkyColor = ParseColor(p["skyColor"]);
            if (p["equatorColor"] != null) RenderSettings.ambientEquatorColor = ParseColor(p["equatorColor"]);
            if (p["groundColor"] != null) RenderSettings.ambientGroundColor = ParseColor(p["groundColor"]);

            return new { mode = RenderSettings.ambientMode.ToString(), intensity = RenderSettings.ambientIntensity };
        }

        private static object GetAmbient(JToken p)
        {
            return new
            {
                mode = RenderSettings.ambientMode.ToString(),
                intensity = RenderSettings.ambientIntensity,
                color = new { RenderSettings.ambientLight.r, RenderSettings.ambientLight.g, RenderSettings.ambientLight.b },
                skyColor = new { RenderSettings.ambientSkyColor.r, RenderSettings.ambientSkyColor.g, RenderSettings.ambientSkyColor.b },
                equatorColor = new { RenderSettings.ambientEquatorColor.r, RenderSettings.ambientEquatorColor.g, RenderSettings.ambientEquatorColor.b },
                groundColor = new { RenderSettings.ambientGroundColor.r, RenderSettings.ambientGroundColor.g, RenderSettings.ambientGroundColor.b },
            };
        }

        private static object SetColorTemperature(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var light = go.GetComponent<Light>();
            if (light == null) throw new McpException(-32602, $"No Light on '{go.name}'");
            Undo.RecordObject(light, "Set Color Temperature");
            if (p["temperature"] != null) light.colorTemperature = p["temperature"].Value<float>();
            if (p["useColorTemperature"] != null) light.useColorTemperature = p["useColorTemperature"].Value<bool>();
            return new { gameObject = go.name, colorTemperature = light.colorTemperature, useColorTemperature = light.useColorTemperature };
        }

        private static object SetCookie(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var light = go.GetComponent<Light>();
            if (light == null) throw new McpException(-32602, $"No Light on '{go.name}'");
            var texturePath = Validate.Required<string>(p, "texturePath");
            var tex = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
            if (tex == null) throw new McpException(-32003, $"Texture not found: {texturePath}");
            Undo.RecordObject(light, "Set Light Cookie");
            light.cookie = tex;
            if (p["cookieSize"] != null)
            {
                var s = p["cookieSize"].Value<float>();
                light.cookieSize2D = new Vector2(s, s);
            }
            return new { gameObject = go.name, cookie = tex.name };
        }

        private static object SetCullingMask(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var light = go.GetComponent<Light>();
            if (light == null) throw new McpException(-32602, $"No Light on '{go.name}'");
            Undo.RecordObject(light, "Set Culling Mask");
            if (p["cullingMask"] != null) light.cullingMask = p["cullingMask"].Value<int>();
            if (p["layers"] is JArray layers)
            {
                int mask = 0;
                foreach (var l in layers)
                    mask |= 1 << LayerMask.NameToLayer(l.Value<string>());
                light.cullingMask = mask;
            }
            return new { gameObject = go.name, cullingMask = light.cullingMask };
        }

        private static object CreateReflectionProbe(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "ReflectionProbe";
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create ReflectionProbe");
            if (p["position"] != null)
                go.transform.position = new Vector3(p["position"]["x"]?.Value<float>() ?? 0, p["position"]["y"]?.Value<float>() ?? 0, p["position"]["z"]?.Value<float>() ?? 0);

            var probe = go.AddComponent<ReflectionProbe>();
            if (p["size"] != null)
                probe.size = new Vector3(p["size"]["x"]?.Value<float>() ?? 10, p["size"]["y"]?.Value<float>() ?? 10, p["size"]["z"]?.Value<float>() ?? 10);
            if (p["resolution"] != null) probe.resolution = p["resolution"].Value<int>();
            if (p["mode"] != null && Enum.TryParse<ReflectionProbeMode>(p["mode"].Value<string>(), true, out var mode))
                probe.mode = mode;
            if (p["intensity"] != null) probe.intensity = p["intensity"].Value<float>();
            if (p["boxProjection"] != null) probe.boxProjection = p["boxProjection"].Value<bool>();

            return new
            {
                success = true, name = go.name, instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                size = new { probe.size.x, probe.size.y, probe.size.z },
                resolution = probe.resolution,
                mode = probe.mode.ToString(),
            };
        }

        private static object SetReflectionProbe(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var probe = go.GetComponent<ReflectionProbe>();
            if (probe == null) throw new McpException(-32602, $"No ReflectionProbe on '{go.name}'");
            Undo.RecordObject(probe, "Set ReflectionProbe");

            if (p["size"] != null) probe.size = new Vector3(p["size"]["x"]?.Value<float>() ?? probe.size.x, p["size"]["y"]?.Value<float>() ?? probe.size.y, p["size"]["z"]?.Value<float>() ?? probe.size.z);
            if (p["resolution"] != null) probe.resolution = p["resolution"].Value<int>();
            if (p["mode"] != null && Enum.TryParse<ReflectionProbeMode>(p["mode"].Value<string>(), true, out var mode)) probe.mode = mode;
            if (p["intensity"] != null) probe.intensity = p["intensity"].Value<float>();
            if (p["boxProjection"] != null) probe.boxProjection = p["boxProjection"].Value<bool>();
            if (p["nearClipPlane"] != null) probe.nearClipPlane = p["nearClipPlane"].Value<float>();
            if (p["farClipPlane"] != null) probe.farClipPlane = p["farClipPlane"].Value<float>();

            EditorUtility.SetDirty(probe);
            return new { success = true, gameObject = go.name };
        }

        private static object FindReflectionProbes(JToken p)
        {
            var probes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);
            return new
            {
                count = probes.Length,
                probes = probes.Select(probe => new
                {
                    name = probe.gameObject.name,
                    path = GameObjectFinder.GetPath(probe.gameObject),
                    size = new { probe.size.x, probe.size.y, probe.size.z },
                    resolution = probe.resolution,
                    mode = probe.mode.ToString(),
                    intensity = probe.intensity,
                }).ToArray(),
            };
        }

        private static object CreateLightProbeGroup(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "LightProbeGroup";
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create LightProbeGroup");
            if (p["position"] != null)
                go.transform.position = new Vector3(p["position"]["x"]?.Value<float>() ?? 0, p["position"]["y"]?.Value<float>() ?? 0, p["position"]["z"]?.Value<float>() ?? 0);

            var group = go.AddComponent<LightProbeGroup>();

            var positions = p["probePositions"] as JArray;
            if (positions != null)
            {
                group.probePositions = positions.Select(pos => new Vector3(
                    pos["x"]?.Value<float>() ?? 0, pos["y"]?.Value<float>() ?? 0, pos["z"]?.Value<float>() ?? 0)).ToArray();
            }

            return new
            {
                success = true, name = go.name, instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                probeCount = group.probePositions.Length,
            };
        }
    }
}
