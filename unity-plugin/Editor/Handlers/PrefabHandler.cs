using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class PrefabHandler
    {
        public static void Register()
        {
            CommandRouter.Register("prefab.create", Create);
            CommandRouter.Register("prefab.instantiate", Instantiate);
            CommandRouter.Register("prefab.instantiateBatch", InstantiateBatch);
            CommandRouter.Register("prefab.apply", Apply);
            CommandRouter.Register("prefab.revert", Revert);
            CommandRouter.Register("prefab.unpack", Unpack);
            CommandRouter.Register("prefab.getOverrides", GetOverrides);
            CommandRouter.Register("prefab.createVariant", CreateVariant);
            CommandRouter.Register("prefab.find", FindPrefabs);
            CommandRouter.Register("prefab.getInfo", GetInfo);
            CommandRouter.Register("prefab.setActive", SetActive);
            CommandRouter.Register("prefab.getAssetType", GetAssetType);
            CommandRouter.Register("prefab.getInstanceStatus", GetInstanceStatus);
            CommandRouter.Register("prefab.replace", ReplacePrefab);
        }

        private static object PrefabInfo(string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) return new { error = $"Not found: {assetPath}" };
            return new
            {
                name = prefab.name,
                path = assetPath,
                guid = AssetDatabase.AssetPathToGUID(assetPath),
                childCount = prefab.transform.childCount,
                components = prefab.GetComponents<Component>().Select(c => c.GetType().Name).ToArray(),
            };
        }

        private static object Create(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var savePath = p["savePath"]?.Value<string>() ?? $"Assets/Prefabs/{go.name}.prefab";
            savePath = Validate.SafeAssetPath(savePath);

            var dir = Path.GetDirectoryName(Path.Combine(Application.dataPath, "..", savePath));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, savePath, InteractionMode.UserAction);
            return PrefabInfo(savePath);
        }

        private static object Instantiate(JToken p)
        {
            var prefabPath = Validate.Required<string>(p, "prefabPath");
            prefabPath = Validate.SafeAssetPath(prefabPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) throw new McpException(-32003, $"Prefab not found: {prefabPath}");

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(go, "MCP: Instantiate Prefab");

            if (p["name"] != null) go.name = p["name"].Value<string>();
            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0);
            }
            if (p["rotation"] != null)
            {
                go.transform.eulerAngles = new Vector3(
                    p["rotation"]["x"]?.Value<float>() ?? 0,
                    p["rotation"]["y"]?.Value<float>() ?? 0,
                    p["rotation"]["z"]?.Value<float>() ?? 0);
            }
            if (p["parent"] != null)
            {
                var parent = GameObject.Find(p["parent"].Value<string>());
                if (parent != null) go.transform.SetParent(parent.transform, true);
            }

            return GameObjectFinder.ToRichInfo(go);
        }

        private static object InstantiateBatch(JToken p)
        {
            var items = Validate.Required<JArray>(p, "items");
            return BatchExecutor.Execute(items, item => Instantiate(item));
        }

        private static object Apply(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                throw new McpException(-32602, $"'{go.name}' is not a prefab instance");

            PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
            var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
            return new { applied = true, gameObject = go.name, prefabPath };
        }

        private static object Revert(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                throw new McpException(-32602, $"'{go.name}' is not a prefab instance");

            WorkflowManager.SnapshotObject(go);
            PrefabUtility.RevertPrefabInstance(go, InteractionMode.UserAction);
            return new { reverted = true, gameObject = go.name };
        }

        private static object Unpack(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                throw new McpException(-32602, $"'{go.name}' is not a prefab instance");

            var completely = p["completely"]?.Value<bool>() ?? false;
            WorkflowManager.SnapshotObject(go);
            if (completely)
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            else
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);

            return new { unpacked = true, gameObject = go.name, completely };
        }

        private static object GetOverrides(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                throw new McpException(-32602, $"'{go.name}' is not a prefab instance");

            var modifications = PrefabUtility.GetPropertyModifications(go);
            var overrides = modifications?.Select(m => new
            {
                target = m.target?.name,
                propertyPath = m.propertyPath,
                value = m.value,
            }).ToArray();

            return new { gameObject = go.name, overrideCount = overrides?.Length ?? 0, overrides };
        }

        private static object CreateVariant(JToken p)
        {
            var basePath = Validate.Required<string>(p, "basePrefabPath");
            basePath = Validate.SafeAssetPath(basePath);
            var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
            if (basePrefab == null) throw new McpException(-32003, $"Base prefab not found: {basePath}");

            var variantName = p["name"]?.Value<string>() ?? basePrefab.name + "_Variant";
            var savePath = p["savePath"]?.Value<string>() ?? $"Assets/Prefabs/{variantName}.prefab";
            savePath = Validate.SafeAssetPath(savePath);

            var dir = Path.GetDirectoryName(Path.Combine(Application.dataPath, "..", savePath));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
            instance.name = variantName;
            var variant = PrefabUtility.SaveAsPrefabAsset(instance, savePath);
            UnityEngine.Object.DestroyImmediate(instance);

            return PrefabInfo(savePath);
        }

        private static object FindPrefabs(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var nameFilter = p["nameFilter"]?.Value<string>();

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            var results = new List<object>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefabName = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(nameFilter) && !prefabName.ToLower().Contains(nameFilter.ToLower()))
                    continue;
                results.Add(new { name = prefabName, path, guid });
            }

            return new { count = results.Count, prefabs = results };
        }

        private static object GetInfo(JToken p)
        {
            var prefabPath = Validate.Required<string>(p, "prefabPath");
            return PrefabInfo(Validate.SafeAssetPath(prefabPath));
        }

        private static object SetActive(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var active = p["active"]?.Value<bool>() ?? true;
            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(go, "MCP: Prefab SetActive");
            go.SetActive(active);
            return new { name = go.name, active };
        }

        private static object GetAssetType(JToken p)
        {
            var assetPath = Validate.Required<string>(p, "assetPath");
            var type = PrefabUtility.GetPrefabAssetType(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
            return new { assetPath, prefabAssetType = type.ToString() };
        }

        private static object GetInstanceStatus(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var status = PrefabUtility.GetPrefabInstanceStatus(go);
            var assetType = PrefabUtility.GetPrefabAssetType(go);
            var isPartOfPrefab = PrefabUtility.IsPartOfAnyPrefab(go);
            var isRoot = PrefabUtility.IsAnyPrefabInstanceRoot(go);
            string sourcePath = null;
            if (isPartOfPrefab)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (source != null) sourcePath = AssetDatabase.GetAssetPath(source);
            }
            return new
            {
                name = go.name,
                instanceStatus = status.ToString(),
                assetType = assetType.ToString(),
                isPartOfPrefab, isRoot, sourcePath,
            };
        }

        private static object ReplacePrefab(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var newPrefabPath = Validate.Required<string>(p, "newPrefabPath");
            var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath);
            if (newPrefab == null) throw new McpException(-32003, $"Prefab not found: {newPrefabPath}");

            var parent = go.transform.parent;
            var pos = go.transform.position;
            var rot = go.transform.rotation;
            var scale = go.transform.localScale;
            var sibIndex = go.transform.GetSiblingIndex();

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
            Undo.RegisterCreatedObjectUndo(instance, "MCP: Replace Prefab");
            instance.transform.position = pos;
            instance.transform.rotation = rot;
            instance.transform.localScale = scale;
            if (parent != null) instance.transform.SetParent(parent, true);
            instance.transform.SetSiblingIndex(sibIndex);

            Undo.DestroyObjectImmediate(go);
            return new
            {
                success = true,
                newName = instance.name,
                path = GameObjectFinder.GetPath(instance),
                prefabSource = newPrefabPath,
            };
        }
    }
}
