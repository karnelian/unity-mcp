using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace KarnelLabs.MCP
{
    public static class MaterialHandler
    {
        public static void Register()
        {
            CommandRouter.Register("material.create", Create);
            CommandRouter.Register("material.assign", Assign);
            CommandRouter.Register("material.createBatch", CreateBatch);
            CommandRouter.Register("material.assignBatch", AssignBatch);
            CommandRouter.Register("material.duplicate", Duplicate);
            CommandRouter.Register("material.setColor", SetColor);
            CommandRouter.Register("material.setColorsBatch", SetColorsBatch);
            CommandRouter.Register("material.setEmission", SetEmission);
            CommandRouter.Register("material.setEmissionBatch", SetEmissionBatch);
            CommandRouter.Register("material.setTexture", SetTexture);
            CommandRouter.Register("material.setFloat", SetFloat);
            CommandRouter.Register("material.setInt", SetInt);
            CommandRouter.Register("material.setVector", SetVector);
            CommandRouter.Register("material.setTextureOffset", SetTextureOffset);
            CommandRouter.Register("material.setTextureScale", SetTextureScale);
            CommandRouter.Register("material.setKeyword", SetKeyword);
            CommandRouter.Register("material.setRenderQueue", SetRenderQueue);
            CommandRouter.Register("material.setShader", SetShader);
            CommandRouter.Register("material.setGIFlags", SetGIFlags);
            CommandRouter.Register("material.getProperties", GetProperties);
            CommandRouter.Register("material.getKeywords", GetKeywords);
        }

        // ── Helpers ──

        private static Material LoadMaterial(JToken p)
        {
            var path = Validate.Required<string>(p, "materialPath");
            path = Validate.SafeAssetPath(path);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) throw new McpException(-32003, $"Material not found: {path}");
            return mat;
        }

        private static Color ParseColor(JToken c)
        {
            if (c == null) return Color.white;
            return new Color(
                c["r"]?.Value<float>() ?? 1f,
                c["g"]?.Value<float>() ?? 1f,
                c["b"]?.Value<float>() ?? 1f,
                c["a"]?.Value<float>() ?? 1f
            );
        }

        private static object MaterialInfo(Material mat)
        {
            var path = AssetDatabase.GetAssetPath(mat);
            return new
            {
                name = mat.name,
                path = path,
                guid = AssetDatabase.AssetPathToGUID(path),
                shader = mat.shader?.name,
                renderQueue = mat.renderQueue,
                enableInstancing = mat.enableInstancing,
                doubleSided = mat.doubleSidedGI,
                passCount = mat.passCount,
            };
        }

        private static void EnsureDirectoryExists(string assetPath)
        {
            var dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir))
            {
                var fullDir = Path.Combine(Application.dataPath, "..", dir);
                if (!Directory.Exists(fullDir)) Directory.CreateDirectory(fullDir);
            }
        }

        // ── Commands ──

        private static object Create(JToken p)
        {
            var matName = Validate.Required<string>(p, "name");
            var savePath = p["savePath"]?.Value<string>() ?? $"Assets/Materials/{matName}.mat";
            savePath = Validate.SafeAssetPath(savePath);
            var shaderName = p["shader"]?.Value<string>() ?? "Standard";

            var shader = Shader.Find(shaderName);
            if (shader == null) throw new McpException(-32602, $"Shader not found: {shaderName}");

            EnsureDirectoryExists(savePath);
            var mat = new Material(shader) { name = matName };

            // 초기 색상 설정
            if (p["color"] != null) mat.color = ParseColor(p["color"]);

            AssetDatabase.CreateAsset(mat, savePath);
            AssetDatabase.SaveAssets();
            return MaterialInfo(mat);
        }

        private static object Assign(JToken p)
        {
            var mat = LoadMaterial(p);
            var go = GameObjectFinder.FindOrThrow(p);
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) throw new McpException(-32602, $"No Renderer on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(renderer, "MCP: Assign Material");

            var index = p["materialIndex"]?.Value<int>() ?? 0;
            var mats = renderer.sharedMaterials;
            if (index < 0 || index >= mats.Length)
                throw new McpException(-32602, $"materialIndex {index} out of range (0-{mats.Length - 1})");
            mats[index] = mat;
            renderer.sharedMaterials = mats;

            return new { assigned = true, gameObject = go.name, material = mat.name, index };
        }

        private static object CreateBatch(JToken p)
        {
            var items = Validate.Required<JArray>(p, "items");
            return BatchExecutor.ExecuteAssetBatch(items, item =>
            {
                return Create(item);
            });
        }

        private static object AssignBatch(JToken p)
        {
            var items = Validate.Required<JArray>(p, "items");
            return BatchExecutor.Execute(items, item => Assign(item));
        }

        private static object Duplicate(JToken p)
        {
            var mat = LoadMaterial(p);
            var newName = p["newName"]?.Value<string>() ?? mat.name + "_Copy";
            var savePath = p["savePath"]?.Value<string>();
            if (string.IsNullOrEmpty(savePath))
            {
                var srcPath = AssetDatabase.GetAssetPath(mat);
                var dir = Path.GetDirectoryName(srcPath);
                savePath = Path.Combine(dir, newName + ".mat").Replace("\\", "/");
            }
            savePath = Validate.SafeAssetPath(savePath);

            EnsureDirectoryExists(savePath);
            var copy = new Material(mat) { name = newName };
            AssetDatabase.CreateAsset(copy, savePath);
            AssetDatabase.SaveAssets();
            return MaterialInfo(copy);
        }

        private static object SetColor(JToken p)
        {
            var mat = LoadMaterial(p);
            var propName = p["property"]?.Value<string>() ?? "_Color";
            var color = ParseColor(p["color"]);

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Material Color");
            mat.SetColor(propName, color);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, property = propName, color = new { color.r, color.g, color.b, color.a } };
        }

        private static object SetColorsBatch(JToken p)
        {
            var items = Validate.Required<JArray>(p, "items");
            return BatchExecutor.Execute(items, item => SetColor(item));
        }

        private static object SetEmission(JToken p)
        {
            var mat = LoadMaterial(p);
            var color = ParseColor(p["color"]);
            var intensity = p["intensity"]?.Value<float>() ?? 1f;

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Emission");
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            mat.SetColor("_EmissionColor", color * intensity);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, emission = true, intensity };
        }

        private static object SetEmissionBatch(JToken p)
        {
            var items = Validate.Required<JArray>(p, "items");
            return BatchExecutor.Execute(items, item => SetEmission(item));
        }

        private static object SetTexture(JToken p)
        {
            var mat = LoadMaterial(p);
            var propName = p["property"]?.Value<string>() ?? "_MainTex";
            var texPath = Validate.Required<string>(p, "texturePath");
            texPath = Validate.SafeAssetPath(texPath);

            var tex = AssetDatabase.LoadAssetAtPath<Texture>(texPath);
            if (tex == null) throw new McpException(-32003, $"Texture not found: {texPath}");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Texture");
            mat.SetTexture(propName, tex);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, property = propName, texture = texPath };
        }

        private static object SetFloat(JToken p)
        {
            var mat = LoadMaterial(p);
            var propName = Validate.Required<string>(p, "property");
            var value = Validate.Required<float>(p, "value");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Float");
            mat.SetFloat(propName, value);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, property = propName, value };
        }

        private static object SetInt(JToken p)
        {
            var mat = LoadMaterial(p);
            var propName = Validate.Required<string>(p, "property");
            var value = Validate.Required<int>(p, "value");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Int");
            mat.SetInt(propName, value);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, property = propName, value };
        }

        private static object SetVector(JToken p)
        {
            var mat = LoadMaterial(p);
            var propName = Validate.Required<string>(p, "property");
            var v = Validate.Required<JToken>(p, "value");
            var vec = new Vector4(
                v["x"]?.Value<float>() ?? 0, v["y"]?.Value<float>() ?? 0,
                v["z"]?.Value<float>() ?? 0, v["w"]?.Value<float>() ?? 0
            );

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Vector");
            mat.SetVector(propName, vec);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, property = propName, value = new { vec.x, vec.y, vec.z, vec.w } };
        }

        private static object SetTextureOffset(JToken p)
        {
            var mat = LoadMaterial(p);
            var propName = p["property"]?.Value<string>() ?? "_MainTex";
            var x = p["x"]?.Value<float>() ?? 0;
            var y = p["y"]?.Value<float>() ?? 0;

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Texture Offset");
            mat.SetTextureOffset(propName, new Vector2(x, y));
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, property = propName, offset = new { x, y } };
        }

        private static object SetTextureScale(JToken p)
        {
            var mat = LoadMaterial(p);
            var propName = p["property"]?.Value<string>() ?? "_MainTex";
            var x = p["x"]?.Value<float>() ?? 1;
            var y = p["y"]?.Value<float>() ?? 1;

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Texture Scale");
            mat.SetTextureScale(propName, new Vector2(x, y));
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, property = propName, scale = new { x, y } };
        }

        private static object SetKeyword(JToken p)
        {
            var mat = LoadMaterial(p);
            var keyword = Validate.Required<string>(p, "keyword");
            var enabled = p["enabled"]?.Value<bool>() ?? true;

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Keyword");
            if (enabled) mat.EnableKeyword(keyword);
            else mat.DisableKeyword(keyword);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, keyword, enabled };
        }

        private static object SetRenderQueue(JToken p)
        {
            var mat = LoadMaterial(p);
            var queue = Validate.Required<int>(p, "renderQueue");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Render Queue");
            mat.renderQueue = queue;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, renderQueue = queue };
        }

        private static object SetShader(JToken p)
        {
            var mat = LoadMaterial(p);
            var shaderName = Validate.Required<string>(p, "shader");
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new McpException(-32602, $"Shader not found: {shaderName}");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set Shader");
            mat.shader = shader;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return MaterialInfo(mat);
        }

        private static object SetGIFlags(JToken p)
        {
            var mat = LoadMaterial(p);
            var flags = Validate.Required<string>(p, "flags");
            var giFlags = Validate.ParseEnum<MaterialGlobalIlluminationFlags>(flags, "flags");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(mat));
            Undo.RecordObject(mat, "MCP: Set GI Flags");
            mat.globalIlluminationFlags = giFlags;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();

            return new { material = mat.name, globalIlluminationFlags = giFlags.ToString() };
        }

        private static object GetProperties(JToken p)
        {
            var mat = LoadMaterial(p);
            var shader = mat.shader;
            var props = new List<object>();

            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                var propName = shader.GetPropertyName(i);
                var propType = shader.GetPropertyType(i);
                object value = null;

                switch (propType)
                {
                    case ShaderPropertyType.Color:
                        var c = mat.GetColor(propName);
                        value = new { c.r, c.g, c.b, c.a };
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        value = mat.GetFloat(propName);
                        break;
                    case ShaderPropertyType.Vector:
                        var v = mat.GetVector(propName);
                        value = new { v.x, v.y, v.z, v.w };
                        break;
                    case ShaderPropertyType.Texture:
                        var tex = mat.GetTexture(propName);
                        value = tex != null ? AssetDatabase.GetAssetPath(tex) : null;
                        break;
                    case ShaderPropertyType.Int:
                        value = mat.GetInt(propName);
                        break;
                }

                props.Add(new
                {
                    name = propName,
                    type = propType.ToString(),
                    description = shader.GetPropertyDescription(i),
                    value,
                });
            }

            return new { material = mat.name, shader = shader.name, properties = props };
        }

        private static object GetKeywords(JToken p)
        {
            var mat = LoadMaterial(p);
            return new
            {
                material = mat.name,
                enabledKeywords = mat.shaderKeywords,
                shader = mat.shader?.name,
            };
        }
    }
}
