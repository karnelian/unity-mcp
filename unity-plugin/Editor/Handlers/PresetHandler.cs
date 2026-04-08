using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class PresetHandler
    {
        public static void Register()
        {
            CommandRouter.Register("preset.create", Create);
            CommandRouter.Register("preset.apply", Apply);
            CommandRouter.Register("preset.getInfo", GetInfo);
            CommandRouter.Register("preset.find", Find);
            CommandRouter.Register("preset.setAsDefault", SetAsDefault);
        }

        private static object Create(JToken p)
        {
            string savePath = (string)p["savePath"];
            Object source = null;

            if (p["sourcePath"] != null)
            {
                source = AssetDatabase.LoadMainAssetAtPath((string)p["sourcePath"]);
            }
            else if (p["sourceGameObject"] != null)
            {
                var go = GameObjectFinder.FindByName((string)p["sourceGameObject"]);
                string componentType = (string)p["componentType"];
                if (componentType != null)
                {
                    var comp = go.GetComponents<Component>()
                        .FirstOrDefault(c => c.GetType().Name == componentType);
                    source = comp;
                }
                else
                {
                    source = go;
                }
            }

            if (source == null)
                throw new McpException(-32000, "Source object not found for preset creation");

            var preset = new Preset(source);
            AssetDatabase.CreateAsset(preset, savePath);
            AssetDatabase.SaveAssets();

            return new { success = true, path = savePath, targetType = preset.GetTargetTypeName() };
        }

        private static object Apply(JToken p)
        {
            string presetPath = (string)p["presetPath"];
            var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
            if (preset == null) throw new McpException(-32000, $"Preset not found at '{presetPath}'");

            Object target = null;

            if (p["targetPath"] != null)
            {
                target = AssetDatabase.LoadMainAssetAtPath((string)p["targetPath"]);
            }
            else if (p["targetGameObject"] != null)
            {
                var go = GameObjectFinder.FindByName((string)p["targetGameObject"]);
                string componentType = (string)p["componentType"];
                if (componentType != null)
                {
                    target = go.GetComponents<Component>()
                        .FirstOrDefault(c => c.GetType().Name == componentType);
                }
                else
                {
                    target = go;
                }
            }

            if (target == null)
                throw new McpException(-32000, "Target object not found for preset application");

            if (!preset.CanBeAppliedTo(target))
                throw new McpException(-32000, $"Preset '{presetPath}' cannot be applied to target of type '{target.GetType().Name}'");

            Undo.RecordObject(target, "Apply Preset");
            preset.ApplyTo(target);
            EditorUtility.SetDirty(target);

            return new { success = true, presetPath, targetType = target.GetType().Name };
        }

        private static object GetInfo(JToken p)
        {
            string path = (string)p["path"];
            var preset = AssetDatabase.LoadAssetAtPath<Preset>(path);
            if (preset == null) throw new McpException(-32000, $"Preset not found at '{path}'");

            var properties = preset.PropertyModifications.Select(pm => new
            {
                propertyPath = pm.propertyPath,
                value = pm.value,
            }).Take(50).ToArray();

            return new
            {
                path,
                targetType = preset.GetTargetTypeName(),
                isValid = preset.IsValid(),
                propertyCount = preset.PropertyModifications.Length,
                properties,
            };
        }

        private static object Find(JToken p)
        {
            string nameFilter = (string)p?["nameFilter"];
            string typeFilter = (string)p?["typeFilter"];
            string folder = (string)p?["folder"];
            string[] searchFolders = !string.IsNullOrEmpty(folder) ? new[] { folder } : null;

            var guids = searchFolders != null
                ? AssetDatabase.FindAssets("t:Preset", searchFolders)
                : AssetDatabase.FindAssets("t:Preset");

            var presets = guids.Select(g =>
            {
                string presetPath = AssetDatabase.GUIDToAssetPath(g);
                var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                return new
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(presetPath),
                    path = presetPath,
                    targetType = preset?.GetTargetTypeName()
                };
            });

            if (!string.IsNullOrEmpty(nameFilter))
                presets = presets.Where(p2 => p2.name.Contains(nameFilter, System.StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(typeFilter))
                presets = presets.Where(p2 => p2.targetType != null && p2.targetType.Contains(typeFilter, System.StringComparison.OrdinalIgnoreCase));

            var result = presets.ToArray();
            return new { count = result.Length, presets = result };
        }

        private static object SetAsDefault(JToken p)
        {
            string presetPath = (string)p["presetPath"];
            var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
            if (preset == null) throw new McpException(-32000, $"Preset not found at '{presetPath}'");

            bool enabled = p["enabled"]?.Value<bool>() ?? true;
            string filter = (string)p["filter"];

            if (enabled)
            {
                var entry = new DefaultPreset(filter ?? "", preset);
                var type = preset.GetPresetType();
                var existingDefaults = Preset.GetDefaultPresetsForType(type).ToList();
                existingDefaults.Add(entry);
                Preset.SetDefaultPresetsForType(type, existingDefaults.ToArray());
            }

            return new { success = true, presetPath, enabled };
        }
    }
}
