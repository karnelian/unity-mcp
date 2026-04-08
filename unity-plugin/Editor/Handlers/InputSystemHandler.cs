#if ENABLE_INPUT_SYSTEM
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KarnelLabs.MCP
{
    public static class InputSystemHandler
    {
        public static void Register()
        {
            CommandRouter.Register("input.createActionAsset", CreateActionAsset);
            CommandRouter.Register("input.getActionAsset", GetActionAsset);
            CommandRouter.Register("input.addActionMap", AddActionMap);
            CommandRouter.Register("input.addAction", AddAction);
            CommandRouter.Register("input.addBinding", AddBinding);
            CommandRouter.Register("input.removeAction", RemoveAction);
            CommandRouter.Register("input.removeActionMap", RemoveActionMap);
            CommandRouter.Register("input.findActionAssets", FindActionAssets);
            CommandRouter.Register("input.addPlayerInput", AddPlayerInput);
            CommandRouter.Register("input.getPlayerInput", GetPlayerInput);
        }

        private static InputActionAsset LoadAsset(JToken p)
        {
            var path = Validate.Required<string>(p, "assetPath");
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (asset == null) throw new McpException(-32003, $"InputActionAsset not found: {path}");
            return asset;
        }

        private static object CreateActionAsset(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "InputActions";
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var safePath = Validate.SafeAssetPath($"{folder}/{name}.inputactions", "path");

            var asset = ScriptableObject.CreateInstance<InputActionAsset>();

            // Add default maps if requested
            var maps = p["maps"] as JArray;
            if (maps != null)
            {
                foreach (var mapName in maps)
                {
                    asset.AddActionMap(mapName.Value<string>());
                }
            }

            var dir = Path.GetDirectoryName(safePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(safePath, asset.ToJson());
            UnityEngine.Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(safePath);

            return new { success = true, path = safePath };
        }

        private static object GetActionAsset(JToken p)
        {
            var asset = LoadAsset(p);
            var maps = new List<object>();
            foreach (var map in asset.actionMaps)
            {
                var actions = new List<object>();
                foreach (var action in map.actions)
                {
                    var bindings = new List<object>();
                    foreach (var binding in action.bindings)
                    {
                        bindings.Add(new
                        {
                            path = binding.path,
                            interactions = binding.interactions,
                            processors = binding.processors,
                            groups = binding.groups,
                            isComposite = binding.isComposite,
                            isPartOfComposite = binding.isPartOfComposite,
                        });
                    }
                    actions.Add(new
                    {
                        name = action.name,
                        type = action.type.ToString(),
                        expectedControlType = action.expectedControlType,
                        bindingCount = bindings.Count,
                        bindings = bindings.ToArray(),
                    });
                }
                maps.Add(new
                {
                    name = map.name,
                    actionCount = actions.Count,
                    actions = actions.ToArray(),
                });
            }
            return new { actionMaps = maps.ToArray() };
        }

        private static object AddActionMap(JToken p)
        {
            var asset = LoadAsset(p);
            var mapName = Validate.Required<string>(p, "mapName");

            Undo.RecordObject(asset, "Add ActionMap");
            asset.AddActionMap(mapName);
            SaveAsset(asset);
            return new { success = true, mapName, totalMaps = asset.actionMaps.Count };
        }

        private static object AddAction(JToken p)
        {
            var asset = LoadAsset(p);
            var mapName = Validate.Required<string>(p, "mapName");
            var actionName = Validate.Required<string>(p, "actionName");
            var actionType = p["actionType"]?.Value<string>() ?? "Value";

            var map = asset.FindActionMap(mapName);
            if (map == null) throw new McpException(-32602, $"ActionMap not found: {mapName}");

            if (!Enum.TryParse<InputActionType>(actionType, true, out var type))
                type = InputActionType.Value;

            Undo.RecordObject(asset, "Add Action");
            var action = map.AddAction(actionName, type);

            var expectedControlType = p["expectedControlType"]?.Value<string>();
            if (!string.IsNullOrEmpty(expectedControlType))
                action.expectedControlType = expectedControlType;

            SaveAsset(asset);
            return new { success = true, mapName, actionName, actionType = type.ToString() };
        }

        private static object AddBinding(JToken p)
        {
            var asset = LoadAsset(p);
            var mapName = Validate.Required<string>(p, "mapName");
            var actionName = Validate.Required<string>(p, "actionName");
            var bindingPath = Validate.Required<string>(p, "bindingPath");

            var map = asset.FindActionMap(mapName);
            if (map == null) throw new McpException(-32602, $"ActionMap not found: {mapName}");
            var action = map.FindAction(actionName);
            if (action == null) throw new McpException(-32602, $"Action not found: {actionName}");

            Undo.RecordObject(asset, "Add Binding");
            action.AddBinding(bindingPath);

            SaveAsset(asset);
            return new { success = true, actionName, bindingPath };
        }

        private static object RemoveAction(JToken p)
        {
            var asset = LoadAsset(p);
            var mapName = Validate.Required<string>(p, "mapName");
            var actionName = Validate.Required<string>(p, "actionName");

            var map = asset.FindActionMap(mapName);
            if (map == null) throw new McpException(-32602, $"ActionMap not found: {mapName}");
            var action = map.FindAction(actionName);
            if (action == null) throw new McpException(-32602, $"Action not found: {actionName}");

            Undo.RecordObject(asset, "Remove Action");
            action.RemoveAction();
            SaveAsset(asset);
            return new { success = true, removed = actionName };
        }

        private static object RemoveActionMap(JToken p)
        {
            var asset = LoadAsset(p);
            var mapName = Validate.Required<string>(p, "mapName");

            var map = asset.FindActionMap(mapName);
            if (map == null) throw new McpException(-32602, $"ActionMap not found: {mapName}");

            Undo.RecordObject(asset, "Remove ActionMap");
            asset.RemoveActionMap(map);
            SaveAsset(asset);
            return new { success = true, removed = mapName };
        }

        private static object FindActionAssets(JToken p)
        {
            var guids = AssetDatabase.FindAssets("t:InputActionAsset");
            var results = guids.Select(g =>
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                return new
                {
                    path,
                    name = asset?.name,
                    actionMapCount = asset?.actionMaps.Count ?? 0,
                };
            }).ToArray();
            return new { count = results.Length, assets = results };
        }

        private static object AddPlayerInput(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            Undo.RecordObject(go, "Add PlayerInput");
            var pi = go.GetComponent<PlayerInput>();
            if (pi == null) pi = go.AddComponent<PlayerInput>();

            var assetPath = p["assetPath"]?.Value<string>();
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
                if (asset != null) pi.actions = asset;
            }

            var defaultMap = p["defaultMap"]?.Value<string>();
            if (!string.IsNullOrEmpty(defaultMap)) pi.defaultActionMap = defaultMap;

            EditorUtility.SetDirty(go);
            return new { success = true, name = go.name, hasActions = pi.actions != null };
        }

        private static object GetPlayerInput(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var pi = go.GetComponent<PlayerInput>();
            if (pi == null) throw new McpException(-32010, $"No PlayerInput on '{go.name}'");

            return new
            {
                name = go.name,
                hasActions = pi.actions != null,
                actionsAsset = pi.actions?.name,
                defaultActionMap = pi.defaultActionMap,
                currentActionMap = pi.currentActionMap?.name,
            };
        }

        private static void SaveAsset(InputActionAsset asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, asset.ToJson());
                AssetDatabase.ImportAsset(path);
            }
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
