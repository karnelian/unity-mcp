using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class ComputeShaderHandler
    {
        public static void Register()
        {
            CommandRouter.Register("computeShader.find", Find);
            CommandRouter.Register("computeShader.getInfo", GetInfo);
            CommandRouter.Register("computeShader.getSource", GetSource);
            CommandRouter.Register("computeShader.dispatch", Dispatch);
        }

        private static object Find(JToken p)
        {
            string nameFilter = (string)p?["nameFilter"];
            string folder = (string)p?["folder"];
            string[] searchFolders = !string.IsNullOrEmpty(folder) ? new[] { folder } : null;

            var guids = searchFolders != null
                ? AssetDatabase.FindAssets("t:ComputeShader", searchFolders)
                : AssetDatabase.FindAssets("t:ComputeShader");

            var shaders = guids.Select(g =>
            {
                string csPath = AssetDatabase.GUIDToAssetPath(g);
                string csName = Path.GetFileNameWithoutExtension(csPath);
                return new { name = csName, path = csPath };
            });

            if (!string.IsNullOrEmpty(nameFilter))
                shaders = shaders.Where(s => s.name.Contains(nameFilter, System.StringComparison.OrdinalIgnoreCase));

            var result = shaders.ToArray();
            return new { count = result.Length, computeShaders = result };
        }

        private static object GetInfo(JToken p)
        {
            string path = (string)p["path"];
            var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
            if (cs == null) throw new McpException(-32000, $"ComputeShader not found at '{path}'");

            // Try to find kernel names by reading source
            var kernels = new System.Collections.Generic.List<string>();
            string sourcePath = Path.GetFullPath(path);
            if (File.Exists(sourcePath))
            {
                var lines = File.ReadAllLines(sourcePath);
                foreach (var line in lines)
                {
                    if (line.Contains("#pragma kernel"))
                    {
                        string kernelName = line.Replace("#pragma kernel", "").Trim().Split(' ')[0];
                        kernels.Add(kernelName);
                    }
                }
            }

            return new
            {
                path,
                name = cs.name,
                kernelNames = kernels.ToArray(),
                kernelCount = kernels.Count,
            };
        }

        private static object GetSource(JToken p)
        {
            string path = (string)p["path"];
            string fullPath = Path.GetFullPath(path);

            if (!File.Exists(fullPath))
                throw new McpException(-32000, $"ComputeShader source not found at '{path}'");

            string source = File.ReadAllText(fullPath);
            if (source.Length > 50000)
                source = source.Substring(0, 50000) + "\n// ... (truncated)";

            return new { path, source };
        }

        private static object Dispatch(JToken p)
        {
            string path = (string)p["path"];
            string kernelName = (string)p["kernel"];
            var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
            if (cs == null) throw new McpException(-32000, $"ComputeShader not found at '{path}'");

            int kernel = cs.FindKernel(kernelName);
            int x = (int)p["threadGroupsX"];
            int y = p["threadGroupsY"]?.Value<int>() ?? 1;
            int z = p["threadGroupsZ"]?.Value<int>() ?? 1;

            cs.Dispatch(kernel, x, y, z);

            return new { success = true, path, kernel = kernelName, threadGroups = new { x, y, z } };
        }
    }
}
