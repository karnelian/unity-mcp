using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class AsmdefHandler
    {
        public static void Register()
        {
            CommandRouter.Register("asmdef.create", Create);
            CommandRouter.Register("asmdef.getInfo", GetInfo);
            CommandRouter.Register("asmdef.set", Set);
            CommandRouter.Register("asmdef.find", Find);
        }

        private static object Create(JToken p)
        {
            string path = (string)p["path"];
            string asmName = (string)p["name"];

            var asmdef = new JObject
            {
                ["name"] = asmName,
                ["rootNamespace"] = "",
                ["references"] = p["references"] != null ? new JArray(((JArray)p["references"]).Select(r => (string)r)) : new JArray(),
                ["includePlatforms"] = p["includePlatforms"] != null ? new JArray(((JArray)p["includePlatforms"]).Select(r => (string)r)) : new JArray(),
                ["excludePlatforms"] = p["excludePlatforms"] != null ? new JArray(((JArray)p["excludePlatforms"]).Select(r => (string)r)) : new JArray(),
                ["allowUnsafeCode"] = p["allowUnsafeCode"]?.Value<bool>() ?? false,
                ["overrideReferences"] = false,
                ["precompiledReferences"] = new JArray(),
                ["autoReferenced"] = p["autoReferenced"]?.Value<bool>() ?? true,
                ["defineConstraints"] = p["defineConstraints"] != null ? new JArray(((JArray)p["defineConstraints"]).Select(r => (string)r)) : new JArray(),
                ["versionDefines"] = new JArray(),
                ["noEngineReferences"] = p["noEngineReferences"]?.Value<bool>() ?? false,
            };

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, asmdef.ToString());
            AssetDatabase.Refresh();

            return new { success = true, path, name = asmName };
        }

        private static object GetInfo(JToken p)
        {
            string path = (string)p["path"];
            if (!File.Exists(path))
                throw new McpException(-32000, $"Asmdef not found at '{path}'");

            string content = File.ReadAllText(path);
            var asmdef = JObject.Parse(content);

            return new
            {
                path,
                name = (string)asmdef["name"],
                references = asmdef["references"]?.ToObject<string[]>() ?? new string[0],
                includePlatforms = asmdef["includePlatforms"]?.ToObject<string[]>() ?? new string[0],
                excludePlatforms = asmdef["excludePlatforms"]?.ToObject<string[]>() ?? new string[0],
                allowUnsafeCode = asmdef["allowUnsafeCode"]?.Value<bool>() ?? false,
                autoReferenced = asmdef["autoReferenced"]?.Value<bool>() ?? true,
                defineConstraints = asmdef["defineConstraints"]?.ToObject<string[]>() ?? new string[0],
                noEngineReferences = asmdef["noEngineReferences"]?.Value<bool>() ?? false,
            };
        }

        private static object Set(JToken p)
        {
            string path = (string)p["path"];
            if (!File.Exists(path))
                throw new McpException(-32000, $"Asmdef not found at '{path}'");

            string content = File.ReadAllText(path);
            var asmdef = JObject.Parse(content);

            if (p["references"] != null) asmdef["references"] = new JArray(((JArray)p["references"]).Select(r => (string)r));
            if (p["includePlatforms"] != null) asmdef["includePlatforms"] = new JArray(((JArray)p["includePlatforms"]).Select(r => (string)r));
            if (p["excludePlatforms"] != null) asmdef["excludePlatforms"] = new JArray(((JArray)p["excludePlatforms"]).Select(r => (string)r));
            if (p["allowUnsafeCode"] != null) asmdef["allowUnsafeCode"] = (bool)p["allowUnsafeCode"];
            if (p["autoReferenced"] != null) asmdef["autoReferenced"] = (bool)p["autoReferenced"];
            if (p["defineConstraints"] != null) asmdef["defineConstraints"] = new JArray(((JArray)p["defineConstraints"]).Select(r => (string)r));

            File.WriteAllText(path, asmdef.ToString());
            AssetDatabase.Refresh();

            return new { success = true, path };
        }

        private static object Find(JToken p)
        {
            string nameFilter = (string)p?["nameFilter"];
            var guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");

            var asmdefs = guids.Select(g =>
            {
                string asmPath = AssetDatabase.GUIDToAssetPath(g);
                string asmName = Path.GetFileNameWithoutExtension(asmPath);
                return new { name = asmName, path = asmPath };
            });

            if (!string.IsNullOrEmpty(nameFilter))
                asmdefs = asmdefs.Where(a => a.name.Contains(nameFilter, System.StringComparison.OrdinalIgnoreCase));

            var result = asmdefs.ToArray();
            return new { count = result.Length, assemblyDefinitions = result };
        }
    }
}
