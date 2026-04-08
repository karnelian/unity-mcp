using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class ModelHandler
    {
        public static void Register()
        {
            CommandRouter.Register("model.getSettings", GetSettings);
            CommandRouter.Register("model.setSettings", SetSettings);
            CommandRouter.Register("model.getInfo", GetInfo);
            CommandRouter.Register("model.find", Find);
            CommandRouter.Register("model.getMeshInfo", GetMeshInfo);
            CommandRouter.Register("model.setRigType", SetRigType);
            CommandRouter.Register("model.getAnimations", GetAnimations);
            CommandRouter.Register("model.setAnimationSettings", SetAnimationSettings);
            CommandRouter.Register("model.setScale", SetScale);
            CommandRouter.Register("model.setMaterialImport", SetMaterialImport);
        }

        private static ModelImporter GetImporter(JToken p)
        {
            var path = Validate.Required<string>(p, "modelPath");
            path = Validate.SafeAssetPath(path);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new McpException(-32003, $"ModelImporter not found: {path}");
            return importer;
        }

        private static object GetSettings(JToken p)
        {
            var importer = GetImporter(p);
            return new
            {
                path = importer.assetPath,
                globalScale = importer.globalScale,
                useFileScale = importer.useFileScale,
                meshCompression = importer.meshCompression.ToString(),
                isReadable = importer.isReadable,
                importNormals = importer.importNormals.ToString(),
                importTangents = importer.importTangents.ToString(),
                importAnimation = importer.importAnimation,
                animationType = importer.animationType.ToString(),
                materialImportMode = importer.materialImportMode.ToString(),
                optimizeMeshPolygons = importer.optimizeMeshPolygons,
                optimizeMeshVertices = importer.optimizeMeshVertices,
                generateColliders = importer.addCollider,
            };
        }

        private static object SetSettings(JToken p)
        {
            var importer = GetImporter(p);
            WorkflowManager.SnapshotAsset(importer.assetPath);

            if (p["globalScale"] != null) importer.globalScale = p["globalScale"].Value<float>();
            if (p["meshCompression"] != null) importer.meshCompression = Validate.ParseEnum<ModelImporterMeshCompression>(p["meshCompression"].Value<string>(), "meshCompression");
            if (p["isReadable"] != null) importer.isReadable = p["isReadable"].Value<bool>();
            if (p["importNormals"] != null) importer.importNormals = Validate.ParseEnum<ModelImporterNormals>(p["importNormals"].Value<string>(), "importNormals");
            if (p["importAnimation"] != null) importer.importAnimation = p["importAnimation"].Value<bool>();
            if (p["generateColliders"] != null) importer.addCollider = p["generateColliders"].Value<bool>();
            if (p["optimizeMesh"] != null)
            {
                importer.optimizeMeshPolygons = p["optimizeMesh"].Value<bool>();
                importer.optimizeMeshVertices = p["optimizeMesh"].Value<bool>();
            }

            importer.SaveAndReimport();
            return GetSettings(p);
        }

        private static object GetInfo(JToken p)
        {
            var path = Validate.Required<string>(p, "modelPath");
            path = Validate.SafeAssetPath(path);
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj == null) throw new McpException(-32003, $"Model not found: {path}");

            var meshFilters = obj.GetComponentsInChildren<MeshFilter>(true);
            var skinnedMeshes = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            var totalVerts = 0;
            var totalTris = 0;
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;
                totalVerts += mf.sharedMesh.vertexCount;
                totalTris += mf.sharedMesh.triangles.Length / 3;
            }
            foreach (var sm in skinnedMeshes)
            {
                if (sm.sharedMesh == null) continue;
                totalVerts += sm.sharedMesh.vertexCount;
                totalTris += sm.sharedMesh.triangles.Length / 3;
            }

            return new
            {
                name = obj.name,
                path,
                meshCount = meshFilters.Length + skinnedMeshes.Length,
                totalVertices = totalVerts,
                totalTriangles = totalTris,
                hasAnimation = obj.GetComponentInChildren<Animator>(true) != null,
                childCount = obj.transform.childCount,
            };
        }

        private static object Find(JToken p)
        {
            var folder = p["folder"]?.Value<string>() ?? "Assets";
            var nameFilter = p["nameFilter"]?.Value<string>();

            var guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
            var results = new List<object>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(nameFilter) && !name.ToLower().Contains(nameFilter.ToLower())) continue;
                results.Add(new { name, path, guid });
            }
            return new { count = results.Count, models = results };
        }

        private static object GetMeshInfo(JToken p)
        {
            var path = Validate.Required<string>(p, "modelPath");
            path = Validate.SafeAssetPath(path);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var meshes = assets.OfType<Mesh>().Select(m => new
            {
                name = m.name,
                vertexCount = m.vertexCount,
                triangles = m.triangles.Length / 3,
                subMeshCount = m.subMeshCount,
                bounds = new { m.bounds.center.x, m.bounds.center.y, m.bounds.center.z, sizeX = m.bounds.size.x, sizeY = m.bounds.size.y, sizeZ = m.bounds.size.z },
                hasNormals = m.normals.Length > 0,
                hasUVs = m.uv.Length > 0,
                hasTangents = m.tangents.Length > 0,
            }).ToArray();

            return new { path, meshCount = meshes.Length, meshes };
        }

        private static object SetRigType(JToken p)
        {
            var importer = GetImporter(p);
            var rigType = Validate.Required<string>(p, "rigType");
            WorkflowManager.SnapshotAsset(importer.assetPath);

            importer.animationType = Validate.ParseEnum<ModelImporterAnimationType>(rigType, "rigType");
            importer.SaveAndReimport();
            return new { path = importer.assetPath, animationType = importer.animationType.ToString() };
        }

        private static object GetAnimations(JToken p)
        {
            var path = Validate.Required<string>(p, "modelPath");
            path = Validate.SafeAssetPath(path);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var clips = assets.OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__"))
                .Select(c => new
                {
                    name = c.name,
                    length = c.length,
                    frameRate = c.frameRate,
                    isLooping = c.isLooping,
                    isHumanMotion = c.isHumanMotion,
                    events = c.events.Length,
                }).ToArray();

            return new { path, clipCount = clips.Length, clips };
        }

        private static object SetAnimationSettings(JToken p)
        {
            var importer = GetImporter(p);
            var clipName = Validate.Required<string>(p, "clipName");
            WorkflowManager.SnapshotAsset(importer.assetPath);

            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            var clip = clips.FirstOrDefault(c => c.name == clipName);
            if (clip == null) throw new McpException(-32602, $"Animation clip '{clipName}' not found");

            if (p["loop"] != null) { clip.loop = p["loop"].Value<bool>(); clip.loopTime = p["loop"].Value<bool>(); }
            if (p["name"] != null) clip.name = p["name"].Value<string>();

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            return new { path = importer.assetPath, clip = clip.name, loop = clip.loopTime };
        }

        private static object SetScale(JToken p)
        {
            var importer = GetImporter(p);
            var scale = Validate.Required<float>(p, "scale");
            WorkflowManager.SnapshotAsset(importer.assetPath);

            importer.globalScale = scale;
            importer.SaveAndReimport();
            return new { path = importer.assetPath, globalScale = scale };
        }

        private static object SetMaterialImport(JToken p)
        {
            var importer = GetImporter(p);
            var mode = Validate.Required<string>(p, "mode");
            WorkflowManager.SnapshotAsset(importer.assetPath);

            importer.materialImportMode = Validate.ParseEnum<ModelImporterMaterialImportMode>(mode, "mode");
            importer.SaveAndReimport();
            return new { path = importer.assetPath, materialImportMode = importer.materialImportMode.ToString() };
        }
    }
}
