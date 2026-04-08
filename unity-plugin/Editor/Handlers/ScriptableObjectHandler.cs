using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class ScriptableObjectHandler
    {
        public static void Register()
        {
            CommandRouter.Register("so.create", Create);
            CommandRouter.Register("so.find", Find);
            CommandRouter.Register("so.getProperties", GetProperties);
            CommandRouter.Register("so.setProperty", SetProperty);
            CommandRouter.Register("so.duplicate", Duplicate);
            CommandRouter.Register("so.delete", Delete);
            CommandRouter.Register("so.toJson", ToJson);
            CommandRouter.Register("so.fromJson", FromJson);
            CommandRouter.Register("so.getTypes", GetTypes);
            CommandRouter.Register("so.getInfo", GetInfo);
        }

        private static ScriptableObject LoadSO(JToken p)
        {
            var path = Validate.Required<string>(p, "assetPath");
            path = Validate.SafeAssetPath(path);
            var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null) throw new McpException(-32003, $"ScriptableObject not found: {path}");
            return so;
        }

        private static object SOInfo(ScriptableObject so)
        {
            var path = AssetDatabase.GetAssetPath(so);
            return new
            {
                name = so.name,
                type = so.GetType().Name,
                fullType = so.GetType().FullName,
                path,
                guid = AssetDatabase.AssetPathToGUID(path),
            };
        }

        private static object Create(JToken p)
        {
            var typeName = Validate.Required<string>(p, "typeName");
            var savePath = Validate.Required<string>(p, "savePath");
            savePath = Validate.SafeAssetPath(savePath);

            var type = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
            if (type == null) throw new McpException(-32602, $"ScriptableObject type not found: {typeName}");

            var dir = Path.GetDirectoryName(Path.Combine(Application.dataPath, "..", savePath));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var so = ScriptableObject.CreateInstance(type);
            so.name = p["name"]?.Value<string>() ?? Path.GetFileNameWithoutExtension(savePath);
            AssetDatabase.CreateAsset(so, savePath);
            AssetDatabase.SaveAssets();
            return SOInfo(so);
        }

        private static object Find(JToken p)
        {
            var typeName = p["typeName"]?.Value<string>();
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var filter = string.IsNullOrEmpty(typeName) ? "t:ScriptableObject" : $"t:{typeName}";

            var guids = AssetDatabase.FindAssets(filter, new[] { folder });
            var results = guids.Select(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                return so != null ? new { name = so.name, type = so.GetType().Name, path, guid } : null;
            }).Where(x => x != null).ToArray();

            return new { count = results.Length, assets = results };
        }

        private static object GetProperties(JToken p)
        {
            var so = LoadSO(p);
            var serialized = new SerializedObject(so);
            var props = new List<object>();
            var iter = serialized.GetIterator();
            iter.Next(true);
            do
            {
                if (iter.depth > 2) continue;
                props.Add(new
                {
                    name = iter.name,
                    displayName = iter.displayName,
                    type = iter.propertyType.ToString(),
                    value = GetPropertyValue(iter),
                    editable = iter.editable,
                });
            } while (iter.Next(iter.depth < 2));

            return new { asset = so.name, type = so.GetType().Name, properties = props };
        }

        private static object GetPropertyValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer: return prop.intValue;
                case SerializedPropertyType.Boolean: return prop.boolValue;
                case SerializedPropertyType.Float: return prop.floatValue;
                case SerializedPropertyType.String: return prop.stringValue;
                case SerializedPropertyType.Enum: return prop.enumDisplayNames.Length > prop.enumValueIndex ? prop.enumDisplayNames[prop.enumValueIndex] : prop.enumValueIndex.ToString();
                case SerializedPropertyType.Color: var c = prop.colorValue; return new { c.r, c.g, c.b, c.a };
                case SerializedPropertyType.Vector2: var v2 = prop.vector2Value; return new { v2.x, v2.y };
                case SerializedPropertyType.Vector3: var v3 = prop.vector3Value; return new { v3.x, v3.y, v3.z };
                case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue?.name;
                default: return prop.propertyType.ToString();
            }
        }

        private static object SetProperty(JToken p)
        {
            var so = LoadSO(p);
            var propName = Validate.Required<string>(p, "propertyName");
            var value = Validate.Required<JToken>(p, "value");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(so));
            var serialized = new SerializedObject(so);
            var prop = serialized.FindProperty(propName);
            if (prop == null) throw new McpException(-32602, $"Property '{propName}' not found on {so.GetType().Name}");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer: prop.intValue = value.Value<int>(); break;
                case SerializedPropertyType.Boolean: prop.boolValue = value.Value<bool>(); break;
                case SerializedPropertyType.Float: prop.floatValue = value.Value<float>(); break;
                case SerializedPropertyType.String: prop.stringValue = value.Value<string>(); break;
                case SerializedPropertyType.Enum: prop.enumValueIndex = value.Value<int>(); break;
                case SerializedPropertyType.Color:
                    var cv = value;
                    prop.colorValue = new Color(
                        cv["r"]?.Value<float>() ?? 0, cv["g"]?.Value<float>() ?? 0,
                        cv["b"]?.Value<float>() ?? 0, cv["a"]?.Value<float>() ?? 1);
                    break;
                case SerializedPropertyType.Vector2:
                    var v2 = value;
                    prop.vector2Value = new Vector2(v2["x"]?.Value<float>() ?? 0, v2["y"]?.Value<float>() ?? 0);
                    break;
                case SerializedPropertyType.Vector3:
                    var v3 = value;
                    prop.vector3Value = new Vector3(
                        v3["x"]?.Value<float>() ?? 0, v3["y"]?.Value<float>() ?? 0, v3["z"]?.Value<float>() ?? 0);
                    break;
                default: throw new McpException(-32602, $"Cannot set property type {prop.propertyType} via MCP");
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            return new { set = true, asset = so.name, property = propName };
        }

        private static object Duplicate(JToken p)
        {
            var so = LoadSO(p);
            var srcPath = AssetDatabase.GetAssetPath(so);
            var newName = p["newName"]?.Value<string>() ?? so.name + "_Copy";
            var dir = Path.GetDirectoryName(srcPath);
            var destPath = Path.Combine(dir, newName + Path.GetExtension(srcPath)).Replace("\\", "/");

            AssetDatabase.CopyAsset(srcPath, destPath);
            AssetDatabase.SaveAssets();
            var copy = AssetDatabase.LoadAssetAtPath<ScriptableObject>(destPath);
            return SOInfo(copy);
        }

        private static object Delete(JToken p)
        {
            var path = Validate.Required<string>(p, "assetPath");
            path = Validate.SafeAssetPath(path);
            var success = AssetDatabase.DeleteAsset(path);
            return new { deleted = success, path };
        }

        private static object ToJson(JToken p)
        {
            var so = LoadSO(p);
            var json = JsonUtility.ToJson(so, true);
            return new { asset = so.name, type = so.GetType().Name, json };
        }

        private static object FromJson(JToken p)
        {
            var so = LoadSO(p);
            var json = Validate.Required<string>(p, "json");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(so));
            Undo.RecordObject(so, "MCP: FromJson");
            JsonUtility.FromJsonOverwrite(json, so);
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            return new { applied = true, asset = so.name };
        }

        private static object GetTypes(JToken p)
        {
            var types = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(t => !t.IsAbstract && !t.IsGenericType && t.IsPublic)
                .Select(t => new { name = t.Name, fullName = t.FullName, assembly = t.Assembly.GetName().Name })
                .OrderBy(t => t.name)
                .Take(200)
                .ToArray();
            return new { count = types.Length, types };
        }

        private static object GetInfo(JToken p)
        {
            var so = LoadSO(p);
            return SOInfo(so);
        }
    }
}
