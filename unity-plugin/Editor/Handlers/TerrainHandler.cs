using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class TerrainHandler
    {
        public static void Register()
        {
            CommandRouter.Register("terrain.create", Create);
            CommandRouter.Register("terrain.getInfo", GetInfo);
            CommandRouter.Register("terrain.setHeight", SetHeight);
            CommandRouter.Register("terrain.getHeight", GetHeight);
            CommandRouter.Register("terrain.flatten", Flatten);
            CommandRouter.Register("terrain.perlinNoise", PerlinNoise);
            CommandRouter.Register("terrain.smooth", Smooth);
            CommandRouter.Register("terrain.addLayer", AddTerrainLayer);
            CommandRouter.Register("terrain.paintTexture", PaintTexture);
            CommandRouter.Register("terrain.setSize", SetSize);
            CommandRouter.Register("terrain.addTree", AddTree);
            CommandRouter.Register("terrain.removeTree", RemoveTree);
            CommandRouter.Register("terrain.getTreePrototypes", GetTreePrototypes);
            CommandRouter.Register("terrain.setTreePrototypes", SetTreePrototypes);
            CommandRouter.Register("terrain.setDetailLayer", SetDetailLayer);
            CommandRouter.Register("terrain.getDetailPrototypes", GetDetailPrototypes);
            CommandRouter.Register("terrain.setHoles", SetHoles);
            CommandRouter.Register("terrain.getSteepness", GetSteepness);
        }

        private static Terrain FindTerrain(JToken p)
        {
            if (p["path"] != null || p["name"] != null || p["instanceId"] != null)
            {
                var go = GameObjectFinder.FindOrThrow(p);
                var terrain = go.GetComponent<Terrain>();
                if (terrain == null) throw new McpException(-32602, $"No Terrain on '{go.name}'");
                return terrain;
            }
            var active = Terrain.activeTerrain;
            if (active == null) throw new McpException(-32003, "No active terrain found");
            return active;
        }

        private static object TerrainInfo(Terrain terrain)
        {
            var td = terrain.terrainData;
            return new
            {
                name = terrain.gameObject.name,
                instanceId = terrain.gameObject.GetInstanceID(),
                path = GameObjectFinder.GetPath(terrain.gameObject),
                size = new { td.size.x, td.size.y, td.size.z },
                heightmapResolution = td.heightmapResolution,
                alphamapResolution = td.alphamapResolution,
                detailResolution = td.detailResolution,
                terrainLayerCount = td.terrainLayers?.Length ?? 0,
                treeInstanceCount = td.treeInstanceCount,
                position = new { terrain.transform.position.x, terrain.transform.position.y, terrain.transform.position.z },
            };
        }

        private static object Create(JToken p)
        {
            var terrainName = p["name"]?.Value<string>() ?? "Terrain";
            var width = p["width"]?.Value<float>() ?? 1000f;
            var height = p["height"]?.Value<float>() ?? 600f;
            var length = p["length"]?.Value<float>() ?? 1000f;
            var resolution = p["heightmapResolution"]?.Value<int>() ?? 513;

            var td = new TerrainData();
            td.heightmapResolution = resolution;
            td.size = new Vector3(width, height, length);

            var go = Terrain.CreateTerrainGameObject(td);
            go.name = terrainName;
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Terrain");

            // Save terrain data as asset
            var savePath = p["savePath"]?.Value<string>() ?? $"Assets/Terrain/{terrainName}_Data.asset";
            savePath = Validate.SafeAssetPath(savePath);
            var dir = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(Application.dataPath, "..", savePath));
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(td, savePath);
            AssetDatabase.SaveAssets();

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0);
            }

            return TerrainInfo(go.GetComponent<Terrain>());
        }

        private static object GetInfo(JToken p)
        {
            return TerrainInfo(FindTerrain(p));
        }

        private static object SetHeight(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var x = Validate.Required<int>(p, "x");
            var y = Validate.Required<int>(p, "y");
            var heightVal = Validate.Required<float>(p, "height");
            var radius = p["radius"]?.Value<int>() ?? 1;

            WorkflowManager.SnapshotObject(terrain.gameObject);
            var heights = td.GetHeights(x - radius, y - radius, radius * 2 + 1, radius * 2 + 1);
            for (int i = 0; i < heights.GetLength(0); i++)
                for (int j = 0; j < heights.GetLength(1); j++)
                    heights[i, j] = heightVal;
            td.SetHeights(x - radius, y - radius, heights);

            return new { set = true, x, y, height = heightVal, radius };
        }

        private static object GetHeight(JToken p)
        {
            var terrain = FindTerrain(p);
            var worldX = p["worldX"]?.Value<float>() ?? 0;
            var worldZ = p["worldZ"]?.Value<float>() ?? 0;
            var height = terrain.SampleHeight(new Vector3(worldX, 0, worldZ));
            return new { worldX, worldZ, height };
        }

        private static object Flatten(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var heightVal = p["height"]?.Value<float>() ?? 0f;

            WorkflowManager.SnapshotObject(terrain.gameObject);
            var res = td.heightmapResolution;
            var heights = new float[res, res];
            for (int i = 0; i < res; i++)
                for (int j = 0; j < res; j++)
                    heights[i, j] = heightVal;
            td.SetHeights(0, 0, heights);

            return new { flattened = true, height = heightVal, resolution = res };
        }

        private static object PerlinNoise(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var scale = p["scale"]?.Value<float>() ?? 20f;
            var amplitude = p["amplitude"]?.Value<float>() ?? 0.1f;
            var offsetX = p["offsetX"]?.Value<float>() ?? 0f;
            var offsetY = p["offsetY"]?.Value<float>() ?? 0f;
            var additive = p["additive"]?.Value<bool>() ?? false;

            WorkflowManager.SnapshotObject(terrain.gameObject);
            var res = td.heightmapResolution;
            var heights = additive ? td.GetHeights(0, 0, res, res) : new float[res, res];

            for (int i = 0; i < res; i++)
            {
                for (int j = 0; j < res; j++)
                {
                    float nx = (float)j / res * scale + offsetX;
                    float ny = (float)i / res * scale + offsetY;
                    float noise = Mathf.PerlinNoise(nx, ny) * amplitude;
                    heights[i, j] = additive ? heights[i, j] + noise : noise;
                }
            }
            td.SetHeights(0, 0, heights);

            return new { applied = true, scale, amplitude, resolution = res };
        }

        private static object Smooth(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var iterations = p["iterations"]?.Value<int>() ?? 1;

            WorkflowManager.SnapshotObject(terrain.gameObject);
            var res = td.heightmapResolution;

            for (int iter = 0; iter < iterations; iter++)
            {
                var heights = td.GetHeights(0, 0, res, res);
                var smoothed = new float[res, res];
                for (int i = 1; i < res - 1; i++)
                {
                    for (int j = 1; j < res - 1; j++)
                    {
                        smoothed[i, j] = (
                            heights[i - 1, j] + heights[i + 1, j] +
                            heights[i, j - 1] + heights[i, j + 1] +
                            heights[i, j]) / 5f;
                    }
                }
                td.SetHeights(0, 0, smoothed);
            }

            return new { smoothed = true, iterations };
        }

        private static object AddTerrainLayer(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var texturePath = Validate.Required<string>(p, "texturePath");

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Validate.SafeAssetPath(texturePath));
            if (tex == null) throw new McpException(-32003, $"Texture not found: {texturePath}");

            var layer = new TerrainLayer { diffuseTexture = tex };
            if (p["tileSize"] != null)
                layer.tileSize = new Vector2(p["tileSize"]["x"]?.Value<float>() ?? 15, p["tileSize"]["y"]?.Value<float>() ?? 15);
            if (p["normalMapPath"] != null)
            {
                var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(p["normalMapPath"].Value<string>());
                if (normal != null) layer.normalMapTexture = normal;
            }

            // Save layer asset
            var layerPath = $"Assets/Terrain/Layer_{tex.name}.terrainlayer";
            var dir = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(Application.dataPath, "..", layerPath));
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(layer, layerPath);

            var layers = td.terrainLayers?.ToList() ?? new List<TerrainLayer>();
            layers.Add(layer);
            td.terrainLayers = layers.ToArray();
            AssetDatabase.SaveAssets();

            return new { added = true, layerIndex = layers.Count - 1, texture = tex.name };
        }

        private static object PaintTexture(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var layerIndex = Validate.Required<int>(p, "layerIndex");
            var x = Validate.Required<int>(p, "x");
            var y = Validate.Required<int>(p, "y");
            var radius = p["radius"]?.Value<int>() ?? 5;
            var opacity = p["opacity"]?.Value<float>() ?? 1f;

            if (td.terrainLayers == null || layerIndex >= td.terrainLayers.Length)
                throw new McpException(-32602, $"Layer index {layerIndex} out of range");

            WorkflowManager.SnapshotObject(terrain.gameObject);
            var size = radius * 2 + 1;
            var alphamaps = td.GetAlphamaps(
                Mathf.Clamp(x - radius, 0, td.alphamapResolution - 1),
                Mathf.Clamp(y - radius, 0, td.alphamapResolution - 1),
                Mathf.Min(size, td.alphamapResolution), Mathf.Min(size, td.alphamapResolution));

            var layerCount = td.terrainLayers.Length;
            for (int i = 0; i < alphamaps.GetLength(0); i++)
            {
                for (int j = 0; j < alphamaps.GetLength(1); j++)
                {
                    for (int l = 0; l < layerCount; l++)
                        alphamaps[i, j, l] = l == layerIndex ? opacity : alphamaps[i, j, l] * (1 - opacity);
                }
            }

            td.SetAlphamaps(
                Mathf.Clamp(x - radius, 0, td.alphamapResolution - 1),
                Mathf.Clamp(y - radius, 0, td.alphamapResolution - 1),
                alphamaps);

            return new { painted = true, layerIndex, x, y, radius, opacity };
        }

        private static object AddTree(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var prefabPath = Validate.Required<string>(p, "prefabPath");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) throw new McpException(-32003, $"Prefab not found: {prefabPath}");

            // Ensure tree prototype exists
            var protos = td.treePrototypes.ToList();
            int protoIndex = protos.FindIndex(tp => tp.prefab == prefab);
            if (protoIndex < 0)
            {
                protos.Add(new TreePrototype { prefab = prefab });
                td.treePrototypes = protos.ToArray();
                protoIndex = protos.Count - 1;
            }

            Undo.RecordObject(td, "Add Trees");
            var positions = p["positions"] as JArray;
            int count = 0;
            if (positions != null)
            {
                foreach (var pos in positions)
                {
                    var inst = new TreeInstance
                    {
                        prototypeIndex = protoIndex,
                        position = new Vector3(
                            pos["x"]?.Value<float>() ?? 0,
                            pos["y"]?.Value<float>() ?? 0,
                            pos["z"]?.Value<float>() ?? 0),
                        widthScale = pos["widthScale"]?.Value<float>() ?? 1f,
                        heightScale = pos["heightScale"]?.Value<float>() ?? 1f,
                        color = Color.white,
                        lightmapColor = Color.white,
                    };
                    var instances = td.treeInstances.ToList();
                    instances.Add(inst);
                    td.treeInstances = instances.ToArray();
                    count++;
                }
            }
            else
            {
                var inst = new TreeInstance
                {
                    prototypeIndex = protoIndex,
                    position = new Vector3(
                        p["x"]?.Value<float>() ?? 0.5f,
                        p["y"]?.Value<float>() ?? 0f,
                        p["z"]?.Value<float>() ?? 0.5f),
                    widthScale = p["widthScale"]?.Value<float>() ?? 1f,
                    heightScale = p["heightScale"]?.Value<float>() ?? 1f,
                    color = Color.white,
                    lightmapColor = Color.white,
                };
                var instances = td.treeInstances.ToList();
                instances.Add(inst);
                td.treeInstances = instances.ToArray();
                count = 1;
            }
            return new { success = true, added = count, totalTrees = td.treeInstanceCount };
        }

        private static object RemoveTree(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            Undo.RecordObject(td, "Remove Trees");

            var protoIndex = p["prototypeIndex"]?.Value<int>();
            if (protoIndex != null)
            {
                var instances = td.treeInstances.Where(t => t.prototypeIndex != protoIndex.Value).ToArray();
                int removed = td.treeInstanceCount - instances.Length;
                td.treeInstances = instances;
                return new { success = true, removed, remaining = instances.Length };
            }
            else
            {
                int count = td.treeInstanceCount;
                td.treeInstances = Array.Empty<TreeInstance>();
                return new { success = true, removed = count, remaining = 0 };
            }
        }

        private static object GetTreePrototypes(JToken p)
        {
            var terrain = FindTerrain(p);
            var protos = terrain.terrainData.treePrototypes;
            return new
            {
                count = protos.Length,
                prototypes = protos.Select((tp, i) => new
                {
                    index = i,
                    prefab = tp.prefab?.name,
                    prefabPath = tp.prefab != null ? AssetDatabase.GetAssetPath(tp.prefab) : null,
                }).ToArray(),
                treeInstanceCount = terrain.terrainData.treeInstanceCount,
            };
        }

        private static object SetTreePrototypes(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var items = Validate.Required<JArray>(p, "prototypes");
            Undo.RecordObject(td, "Set Tree Prototypes");

            var protos = new List<TreePrototype>();
            foreach (var item in items)
            {
                var path = Validate.Required<string>(item, "prefabPath");
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) throw new McpException(-32003, $"Prefab not found: {path}");
                protos.Add(new TreePrototype { prefab = prefab });
            }
            td.treePrototypes = protos.ToArray();
            return new { success = true, count = protos.Count };
        }

        private static object SetDetailLayer(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var layerIndex = p["layerIndex"]?.Value<int>() ?? 0;
            var x = Validate.Required<int>(p, "x");
            var y = Validate.Required<int>(p, "y");
            var radius = p["radius"]?.Value<int>() ?? 5;
            var density = p["density"]?.Value<int>() ?? 1;

            if (td.detailResolution == 0)
                throw new McpException(-32602, "Terrain has zero detail resolution. Set detail resolution first.");
            if (layerIndex >= td.detailPrototypes.Length)
                throw new McpException(-32602, $"Detail layer index {layerIndex} out of range (count: {td.detailPrototypes.Length}). Add detail prototypes first.");

            Undo.RecordObject(td, "Set Detail Layer");
            var size = radius * 2 + 1;
            var details = td.GetDetailLayer(
                Mathf.Clamp(x - radius, 0, td.detailResolution - 1),
                Mathf.Clamp(y - radius, 0, td.detailResolution - 1),
                Mathf.Min(size, td.detailResolution),
                Mathf.Min(size, td.detailResolution),
                layerIndex);

            for (int i = 0; i < details.GetLength(0); i++)
                for (int j = 0; j < details.GetLength(1); j++)
                    details[i, j] = density;

            td.SetDetailLayer(
                Mathf.Clamp(x - radius, 0, td.detailResolution - 1),
                Mathf.Clamp(y - radius, 0, td.detailResolution - 1),
                layerIndex, details);

            return new { success = true, layerIndex, x, y, radius, density };
        }

        private static object GetDetailPrototypes(JToken p)
        {
            var terrain = FindTerrain(p);
            var protos = terrain.terrainData.detailPrototypes;
            return new
            {
                count = protos.Length,
                prototypes = protos.Select((dp, i) => new
                {
                    index = i,
                    prototype = dp.prototype?.name,
                    prototypeTexture = dp.prototypeTexture?.name,
                    renderMode = dp.renderMode.ToString(),
                    minWidth = dp.minWidth,
                    maxWidth = dp.maxWidth,
                    minHeight = dp.minHeight,
                    maxHeight = dp.maxHeight,
                }).ToArray(),
            };
        }

        private static object SetHoles(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var x = Validate.Required<int>(p, "x");
            var y = Validate.Required<int>(p, "y");
            var radius = p["radius"]?.Value<int>() ?? 1;
            var isHole = p["isHole"]?.Value<bool>() ?? true;

            Undo.RecordObject(td, "Set Terrain Holes");
            var size = radius * 2 + 1;
            var holes = new bool[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    holes[i, j] = !isHole;

            td.SetHoles(
                Mathf.Clamp(x - radius, 0, td.holesResolution - 1),
                Mathf.Clamp(y - radius, 0, td.holesResolution - 1),
                holes);

            return new { success = true, x, y, radius, isHole };
        }

        private static object GetSteepness(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;
            var normX = Validate.Required<float>(p, "x");
            var normY = Validate.Required<float>(p, "y");
            var steepness = td.GetSteepness(normX, normY);
            var normal = td.GetInterpolatedNormal(normX, normY);
            return new
            {
                x = normX, y = normY,
                steepness,
                normal = new { normal.x, normal.y, normal.z },
            };
        }

        private static object SetSize(JToken p)
        {
            var terrain = FindTerrain(p);
            var td = terrain.terrainData;

            WorkflowManager.SnapshotObject(terrain.gameObject);
            var size = td.size;
            if (p["width"] != null) size.x = p["width"].Value<float>();
            if (p["height"] != null) size.y = p["height"].Value<float>();
            if (p["length"] != null) size.z = p["length"].Value<float>();
            td.size = size;

            return new { size = new { td.size.x, td.size.y, td.size.z } };
        }
    }
}
