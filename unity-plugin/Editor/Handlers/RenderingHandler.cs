#if UNITY_RENDER_PIPELINES_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KarnelLabs.MCP
{
    public static class RenderingHandler
    {
        public static void Register()
        {
            CommandRouter.Register("rendering.createVolume", CreateVolume);
            CommandRouter.Register("rendering.getVolumeInfo", GetVolumeInfo);
            CommandRouter.Register("rendering.findVolumes", FindVolumes);
            CommandRouter.Register("rendering.addOverride", AddOverride);
            CommandRouter.Register("rendering.removeOverride", RemoveOverride);
            CommandRouter.Register("rendering.setOverrideProperty", SetOverrideProperty);
            CommandRouter.Register("rendering.getOverrides", GetOverrides);
            CommandRouter.Register("rendering.getPipelineInfo", GetPipelineInfo);
            CommandRouter.Register("rendering.setFog", SetFog);
            CommandRouter.Register("rendering.getFog", GetFog);
            CommandRouter.Register("rendering.setSkybox", SetSkybox);
            CommandRouter.Register("rendering.setGlobalShaderProperty", SetGlobalShaderProperty);
        }

        private static object CreateVolume(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "Volume";
            var isGlobal = p["isGlobal"]?.Value<bool>() ?? true;
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create Volume");

            if (p["position"] != null)
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0);

            var volume = go.AddComponent<Volume>();
            volume.isGlobal = isGlobal;
            if (p["priority"] != null) volume.priority = p["priority"].Value<float>();
            if (p["weight"] != null) volume.weight = p["weight"].Value<float>();

            // Create a new profile if requested
            var profilePath = p["profilePath"]?.Value<string>();
            if (!string.IsNullOrEmpty(profilePath))
            {
                var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
                if (profile != null) volume.profile = profile;
            }
            else
            {
                volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            }

            if (!isGlobal)
            {
                var collider = go.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                if (p["size"] != null)
                    collider.size = new Vector3(
                        p["size"]["x"]?.Value<float>() ?? 10,
                        p["size"]["y"]?.Value<float>() ?? 10,
                        p["size"]["z"]?.Value<float>() ?? 10);
            }

            return new
            {
                success = true, name = go.name, instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go), isGlobal,
            };
        }

        private static Volume FindVolume(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vol = go.GetComponent<Volume>();
            if (vol == null) throw new McpException(-32010, $"No Volume on '{go.name}'");
            return vol;
        }

        private static object GetVolumeInfo(JToken p)
        {
            var vol = FindVolume(p);
            var profile = vol.profile;
            return new
            {
                name = vol.gameObject.name,
                path = GameObjectFinder.GetPath(vol.gameObject),
                isGlobal = vol.isGlobal,
                priority = vol.priority,
                weight = vol.weight,
                hasProfile = profile != null,
                overrideCount = profile?.components?.Count ?? 0,
                overrides = profile?.components?.Select(c => new
                {
                    type = c.GetType().Name,
                    active = c.active,
                }).ToArray(),
            };
        }

        private static object FindVolumes(JToken p)
        {
            var volumes = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            return new
            {
                count = volumes.Length,
                volumes = volumes.Select(v => new
                {
                    name = v.gameObject.name,
                    path = GameObjectFinder.GetPath(v.gameObject),
                    isGlobal = v.isGlobal,
                    priority = v.priority,
                    overrideCount = v.profile?.components?.Count ?? 0,
                }).ToArray(),
            };
        }

        private static object AddOverride(JToken p)
        {
            var vol = FindVolume(p);
            var typeName = Validate.Required<string>(p, "overrideType");

            if (vol.profile == null)
                vol.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            // Search for the VolumeComponent type
            var type = FindVolumeComponentType(typeName);
            if (type == null) throw new McpException(-32602, $"VolumeComponent type not found: {typeName}. Check available types with rendering.getOverrides");

            Undo.RecordObject(vol.profile, "Add Volume Override");
            var component = vol.profile.Add(type);
            component.active = true;

            EditorUtility.SetDirty(vol.profile);
            return new { success = true, added = type.Name, totalOverrides = vol.profile.components.Count };
        }

        private static object RemoveOverride(JToken p)
        {
            var vol = FindVolume(p);
            var typeName = Validate.Required<string>(p, "overrideType");
            if (vol.profile == null) throw new McpException(-32602, "Volume has no profile");

            var type = FindVolumeComponentType(typeName);
            if (type == null) throw new McpException(-32602, $"VolumeComponent type not found: {typeName}");

            Undo.RecordObject(vol.profile, "Remove Volume Override");
            vol.profile.Remove(type);
            EditorUtility.SetDirty(vol.profile);
            return new { success = true, removed = typeName };
        }

        private static object SetOverrideProperty(JToken p)
        {
            var vol = FindVolume(p);
            var typeName = Validate.Required<string>(p, "overrideType");
            var propertyName = Validate.Required<string>(p, "property");

            if (vol.profile == null) throw new McpException(-32602, "Volume has no profile");

            var type = FindVolumeComponentType(typeName);
            if (type == null) throw new McpException(-32602, $"VolumeComponent type not found: {typeName}");

            VolumeComponent component = null;
            foreach (var c in vol.profile.components)
            {
                if (c.GetType() == type) { component = c; break; }
            }
            if (component == null) throw new McpException(-32602, $"Override {typeName} not found on volume profile");

            Undo.RecordObject(vol.profile, "Set Volume Override Property");

            // Use SerializedObject to set the property
            var so = new SerializedObject(component);
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                // Try with "m_Value" suffix for VolumeParameter
                prop = so.FindProperty(propertyName + ".m_Value");
            }
            if (prop == null) throw new McpException(-32602, $"Property '{propertyName}' not found on {typeName}");

            var value = p["value"];
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Float:
                    prop.floatValue = value.Value<float>();
                    break;
                case SerializedPropertyType.Integer:
                    prop.intValue = value.Value<int>();
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = value.Value<bool>();
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = new Color(
                        value["r"]?.Value<float>() ?? 0, value["g"]?.Value<float>() ?? 0,
                        value["b"]?.Value<float>() ?? 0, value["a"]?.Value<float>() ?? 1);
                    break;
                default:
                    throw new McpException(-32602, $"Unsupported property type: {prop.propertyType}");
            }

            // Also enable the override
            var overrideProp = so.FindProperty(propertyName + ".m_OverrideState");
            if (overrideProp != null) overrideProp.boolValue = true;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(vol.profile);
            return new { success = true, overrideType = typeName, property = propertyName };
        }

        private static object GetOverrides(JToken p)
        {
            // List all available VolumeComponent types
            var types = new List<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsAbstract && typeof(VolumeComponent).IsAssignableFrom(type))
                            types.Add(type.Name);
                    }
                }
                catch { }
            }
            types.Sort();
            return new { availableTypes = types.ToArray() };
        }

        private static object GetPipelineInfo(JToken p)
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            return new
            {
                hasSRP = pipeline != null,
                pipelineName = pipeline?.name,
                pipelineType = pipeline?.GetType().Name,
                pipelinePath = pipeline != null ? AssetDatabase.GetAssetPath(pipeline) : null,
                colorSpace = QualitySettings.activeColorSpace.ToString(),
                hdr = PlayerSettings.useHDRDisplay,
            };
        }

        private static object SetFog(JToken p)
        {
            if (p["enabled"] != null) RenderSettings.fog = p["enabled"].Value<bool>();
            if (p["color"] != null)
                RenderSettings.fogColor = new Color(
                    p["color"]["r"]?.Value<float>() ?? 0.5f,
                    p["color"]["g"]?.Value<float>() ?? 0.5f,
                    p["color"]["b"]?.Value<float>() ?? 0.5f);
            if (p["mode"] != null && Enum.TryParse<FogMode>(p["mode"].Value<string>(), true, out var fm))
                RenderSettings.fogMode = fm;
            if (p["density"] != null) RenderSettings.fogDensity = p["density"].Value<float>();
            if (p["startDistance"] != null) RenderSettings.fogStartDistance = p["startDistance"].Value<float>();
            if (p["endDistance"] != null) RenderSettings.fogEndDistance = p["endDistance"].Value<float>();

            return new { success = true, fog = RenderSettings.fog, mode = RenderSettings.fogMode.ToString() };
        }

        private static object GetFog(JToken p)
        {
            return new
            {
                enabled = RenderSettings.fog,
                mode = RenderSettings.fogMode.ToString(),
                color = new { RenderSettings.fogColor.r, RenderSettings.fogColor.g, RenderSettings.fogColor.b },
                density = RenderSettings.fogDensity,
                startDistance = RenderSettings.fogStartDistance,
                endDistance = RenderSettings.fogEndDistance,
            };
        }

        private static object SetSkybox(JToken p)
        {
            var materialPath = p["materialPath"]?.Value<string>();
            if (!string.IsNullOrEmpty(materialPath))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (mat == null) throw new McpException(-32003, $"Material not found: {materialPath}");
                RenderSettings.skybox = mat;
            }
            if (p["ambientIntensity"] != null) RenderSettings.ambientIntensity = p["ambientIntensity"].Value<float>();
            if (p["reflectionIntensity"] != null) RenderSettings.reflectionIntensity = p["reflectionIntensity"].Value<float>();

            return new
            {
                success = true,
                skybox = RenderSettings.skybox?.name,
                ambientIntensity = RenderSettings.ambientIntensity,
                reflectionIntensity = RenderSettings.reflectionIntensity,
            };
        }

        private static object SetGlobalShaderProperty(JToken p)
        {
            var property = Validate.Required<string>(p, "property");
            if (p["floatValue"] != null) Shader.SetGlobalFloat(property, p["floatValue"].Value<float>());
            else if (p["intValue"] != null) Shader.SetGlobalInt(property, p["intValue"].Value<int>());
            else if (p["colorValue"] != null)
            {
                var c = p["colorValue"];
                Shader.SetGlobalColor(property, new Color(c["r"]?.Value<float>() ?? 0, c["g"]?.Value<float>() ?? 0, c["b"]?.Value<float>() ?? 0, c["a"]?.Value<float>() ?? 1));
            }
            else if (p["vectorValue"] != null)
            {
                var v = p["vectorValue"];
                Shader.SetGlobalVector(property, new Vector4(v["x"]?.Value<float>() ?? 0, v["y"]?.Value<float>() ?? 0, v["z"]?.Value<float>() ?? 0, v["w"]?.Value<float>() ?? 0));
            }
            return new { success = true, property };
        }

        private static Type FindVolumeComponentType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsAbstract && typeof(VolumeComponent).IsAssignableFrom(type) &&
                            (type.Name == typeName || type.FullName == typeName))
                            return type;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
#endif
