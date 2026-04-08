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
    public static class ShaderHandler
    {
        public static void Register()
        {
            CommandRouter.Register("shader.find", FindShaders);
            CommandRouter.Register("shader.getProperties", GetProperties);
            CommandRouter.Register("shader.getKeywords", GetKeywords);
            CommandRouter.Register("shader.getInfo", GetInfo);
            CommandRouter.Register("shader.listAll", ListAll);
            CommandRouter.Register("shader.getSource", GetSource);
            CommandRouter.Register("shader.getPassCount", GetPassCount);
            CommandRouter.Register("shader.isSupported", IsSupported);
            CommandRouter.Register("shader.findMaterials", FindMaterialsUsingShader);
            CommandRouter.Register("shader.getGlobalProperties", GetGlobalProperties);
            CommandRouter.Register("shader.setGlobalFloat", SetGlobalFloat);
        }

        private static object ShaderInfo(Shader shader)
        {
            return new
            {
                name = shader.name,
                isSupported = shader.isSupported,
                renderQueue = shader.renderQueue,
                passCount = shader.passCount,
                propertyCount = shader.GetPropertyCount(),
            };
        }

        private static object FindShaders(JToken p)
        {
            var nameFilter = p["nameFilter"]?.Value<string>();
            var guids = AssetDatabase.FindAssets("t:Shader");
            var results = new List<object>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader == null) continue;
                if (!string.IsNullOrEmpty(nameFilter) && !shader.name.ToLower().Contains(nameFilter.ToLower())) continue;
                results.Add(new { name = shader.name, path });
            }

            // Also include built-in shaders
            if (string.IsNullOrEmpty(nameFilter) || "Standard".ToLower().Contains(nameFilter.ToLower()))
                results.Add(new { name = "Standard", path = "Built-in" });

            return new { count = results.Count, shaders = results };
        }

        private static object GetProperties(JToken p)
        {
            var shaderName = Validate.Required<string>(p, "shaderName");
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new McpException(-32003, $"Shader not found: {shaderName}");

            var props = new List<object>();
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                props.Add(new
                {
                    name = shader.GetPropertyName(i),
                    type = shader.GetPropertyType(i).ToString(),
                    description = shader.GetPropertyDescription(i),
                    flags = shader.GetPropertyFlags(i).ToString(),
                });
            }
            return new { shader = shaderName, properties = props };
        }

        private static object GetKeywords(JToken p)
        {
            var shaderName = Validate.Required<string>(p, "shaderName");
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new McpException(-32003, $"Shader not found: {shaderName}");

            var keywords = shader.keywordSpace.keywords.Select(k => new
            {
                name = k.name,
                type = k.type.ToString(),
            }).ToArray();

            return new { shader = shaderName, keywords };
        }

        private static object GetInfo(JToken p)
        {
            var shaderName = Validate.Required<string>(p, "shaderName");
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new McpException(-32003, $"Shader not found: {shaderName}");
            return ShaderInfo(shader);
        }

        private static object ListAll(JToken p)
        {
            var count = ShaderUtil.GetAllShaderInfo().Length;
            var shaderInfos = ShaderUtil.GetAllShaderInfo();
            var shaders = shaderInfos.Select(s => new { name = s.name, supported = s.supported }).ToArray();
            return new { count = shaders.Length, shaders };
        }

        private static object GetSource(JToken p)
        {
            var shaderPath = Validate.Required<string>(p, "shaderPath");
            shaderPath = Validate.SafeAssetPath(shaderPath);
            var fullPath = Path.Combine(Application.dataPath, "..", shaderPath);
            if (!File.Exists(fullPath)) throw new McpException(-32003, $"Shader file not found: {shaderPath}");

            var source = File.ReadAllText(fullPath);
            var maxLines = p["maxLines"]?.Value<int>() ?? 200;
            var lines = source.Split('\n');
            if (lines.Length > maxLines)
                source = string.Join("\n", lines.Take(maxLines)) + $"\n... ({lines.Length - maxLines} more lines)";

            return new { path = shaderPath, lineCount = lines.Length, source };
        }

        private static object GetPassCount(JToken p)
        {
            var shaderName = Validate.Required<string>(p, "shaderName");
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new McpException(-32003, $"Shader not found: {shaderName}");
            return new { shader = shaderName, passCount = shader.passCount };
        }

        private static object IsSupported(JToken p)
        {
            var shaderName = Validate.Required<string>(p, "shaderName");
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new McpException(-32003, $"Shader not found: {shaderName}");
            return new { shader = shaderName, isSupported = shader.isSupported };
        }

        private static object FindMaterialsUsingShader(JToken p)
        {
            var shaderName = Validate.Required<string>(p, "shaderName");
            var guids = AssetDatabase.FindAssets("t:Material");
            var results = new List<object>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat?.shader?.name == shaderName)
                    results.Add(new { name = mat.name, path });
            }

            return new { shader = shaderName, count = results.Count, materials = results };
        }

        private static object GetGlobalProperties(JToken p)
        {
            var propertyNames = p["properties"]?.ToObject<string[]>();
            if (propertyNames == null || propertyNames.Length == 0)
                return new { message = "Provide property names to query" };

            var results = new Dictionary<string, object>();
            foreach (var name in propertyNames)
            {
                results[name] = Shader.GetGlobalFloat(name);
            }
            return new { properties = results };
        }

        private static object SetGlobalFloat(JToken p)
        {
            var propName = Validate.Required<string>(p, "property");
            var value = Validate.Required<float>(p, "value");
            Shader.SetGlobalFloat(propName, value);
            return new { property = propName, value };
        }
    }
}
