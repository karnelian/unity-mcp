using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class RenderTextureHandler
    {
        public static void Register()
        {
            CommandRouter.Register("renderTexture.create", Create);
            CommandRouter.Register("renderTexture.set", Set);
            CommandRouter.Register("renderTexture.assign", Assign);
            CommandRouter.Register("renderTexture.getInfo", GetInfo);
            CommandRouter.Register("renderTexture.find", Find);
        }

        private static object Create(JToken p)
        {
            string path = (string)p["path"];
            int width = p["width"]?.Value<int>() ?? 256;
            int height = p["height"]?.Value<int>() ?? 256;
            int depth = p["depth"]?.Value<int>() ?? 24;

            var rt = new RenderTexture(width, height, depth);

            if (p["antiAliasing"] != null) rt.antiAliasing = (int)p["antiAliasing"];
            if (p["enableRandomWrite"] != null) rt.enableRandomWrite = (bool)p["enableRandomWrite"];
            if (p["useMipMap"] != null) rt.useMipMap = (bool)p["useMipMap"];

            AssetDatabase.CreateAsset(rt, path);
            AssetDatabase.SaveAssets();

            return new { success = true, path, width, height, depth };
        }

        private static object Set(JToken p)
        {
            string path = (string)p["path"];
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (rt == null) throw new McpException(-32000, $"RenderTexture not found at '{path}'");

            Undo.RecordObject(rt, "Set RenderTexture");

            bool needsRecreate = false;
            int width = p["width"]?.Value<int>() ?? rt.width;
            int height = p["height"]?.Value<int>() ?? rt.height;
            int depth = p["depth"]?.Value<int>() ?? rt.depth;

            if (width != rt.width || height != rt.height || depth != rt.depth)
            {
                rt.Release();
                rt.width = width;
                rt.height = height;
                rt.depth = depth;
                rt.Create();
                needsRecreate = true;
            }

            if (p["antiAliasing"] != null) rt.antiAliasing = (int)p["antiAliasing"];
            if (p["filterMode"] != null)
            {
                rt.filterMode = (string)p["filterMode"] switch
                {
                    "Point" => FilterMode.Point,
                    "Bilinear" => FilterMode.Bilinear,
                    "Trilinear" => FilterMode.Trilinear,
                    _ => rt.filterMode
                };
            }
            if (p["wrapMode"] != null)
            {
                rt.wrapMode = (string)p["wrapMode"] switch
                {
                    "Repeat" => TextureWrapMode.Repeat,
                    "Clamp" => TextureWrapMode.Clamp,
                    "Mirror" => TextureWrapMode.Mirror,
                    "MirrorOnce" => TextureWrapMode.MirrorOnce,
                    _ => rt.wrapMode
                };
            }

            EditorUtility.SetDirty(rt);
            AssetDatabase.SaveAssets();

            return new { success = true, path, recreated = needsRecreate };
        }

        private static object Assign(JToken p)
        {
            string rtPath = (string)p["renderTexturePath"];
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
            if (rt == null) throw new McpException(-32000, $"RenderTexture not found at '{rtPath}'");

            if (p["targetCamera"] != null)
            {
                var camGo = GameObjectFinder.FindByName((string)p["targetCamera"]);
                var cam = camGo.GetComponent<Camera>();
                if (cam == null) throw new McpException(-32000, $"No Camera on '{camGo.name}'");
                Undo.RecordObject(cam, "Assign RenderTexture to Camera");
                cam.targetTexture = rt;
                EditorUtility.SetDirty(cam);
            }

            if (p["targetMaterial"] != null)
            {
                string matPath = (string)p["targetMaterial"];
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null) throw new McpException(-32000, $"Material not found at '{matPath}'");
                string prop = (string)p["materialProperty"] ?? "_MainTex";
                Undo.RecordObject(mat, "Assign RenderTexture to Material");
                mat.SetTexture(prop, rt);
                EditorUtility.SetDirty(mat);
            }

            return new { success = true, renderTexture = rtPath };
        }

        private static object GetInfo(JToken p)
        {
            string path = (string)p["path"];
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (rt == null) throw new McpException(-32000, $"RenderTexture not found at '{path}'");

            return new
            {
                path,
                width = rt.width,
                height = rt.height,
                depth = rt.depth,
                format = rt.format.ToString(),
                antiAliasing = rt.antiAliasing,
                filterMode = rt.filterMode.ToString(),
                wrapMode = rt.wrapMode.ToString(),
                useMipMap = rt.useMipMap,
                enableRandomWrite = rt.enableRandomWrite,
                isCreated = rt.IsCreated(),
            };
        }

        private static object Find(JToken p)
        {
            string nameFilter = (string)p?["nameFilter"];
            string folder = (string)p?["folder"];
            string[] searchFolders = !string.IsNullOrEmpty(folder) ? new[] { folder } : null;

            var guids = searchFolders != null
                ? AssetDatabase.FindAssets("t:RenderTexture", searchFolders)
                : AssetDatabase.FindAssets("t:RenderTexture");

            var rts = guids.Select(g =>
            {
                string rtPath = AssetDatabase.GUIDToAssetPath(g);
                string rtName = System.IO.Path.GetFileNameWithoutExtension(rtPath);
                return new { name = rtName, path = rtPath };
            });

            if (!string.IsNullOrEmpty(nameFilter))
                rts = rts.Where(r => r.name.Contains(nameFilter, System.StringComparison.OrdinalIgnoreCase));

            var result = rts.ToArray();
            return new { count = result.Length, renderTextures = result };
        }
    }
}
