using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace KarnelLabs.MCP
{
    public static class Tilemap2DHandler
    {
        public static void Register()
        {
            CommandRouter.Register("tilemap.create", Create);
            CommandRouter.Register("tilemap.setTile", SetTile);
            CommandRouter.Register("tilemap.setTilesBatch", SetTilesBatch);
            CommandRouter.Register("tilemap.clearTiles", ClearTiles);
            CommandRouter.Register("tilemap.getTile", GetTile);
            CommandRouter.Register("tilemap.getInfo", GetInfo);
            CommandRouter.Register("tilemap.setRenderer", SetRenderer);
            CommandRouter.Register("tilemap.find", Find);
            CommandRouter.Register("sprite.create", CreateSprite);
            CommandRouter.Register("sprite.setProperties", SetSpriteProperties);
            CommandRouter.Register("sprite.find", FindSprites);
            CommandRouter.Register("sprite.setSortingOrder", SetSortingOrder);
        }

        private static Tilemap FindTilemap(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var tm = go.GetComponent<Tilemap>();
            if (tm == null) throw new McpException(-32010, $"No Tilemap on '{go.name}'");
            return tm;
        }

        private static object Create(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "Tilemap";
            var cellSize = p["cellSize"]?.Value<float>() ?? 1f;

            // Create Grid parent
            var gridGo = new GameObject(name + "_Grid");
            Undo.RegisterCreatedObjectUndo(gridGo, "Create Tilemap");
            var grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(cellSize, cellSize, 0);

            if (p["position"] != null)
                gridGo.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0);

            // Create Tilemap child
            var tmGo = new GameObject(name);
            tmGo.transform.SetParent(gridGo.transform);
            var tilemap = tmGo.AddComponent<Tilemap>();
            var renderer = tmGo.AddComponent<TilemapRenderer>();

            if (p["sortingLayerName"] != null) renderer.sortingLayerName = p["sortingLayerName"].Value<string>();
            if (p["sortingOrder"] != null) renderer.sortingOrder = p["sortingOrder"].Value<int>();

            return new
            {
                success = true,
                grid = new { name = gridGo.name, instanceId = gridGo.GetInstanceID(), path = GameObjectFinder.GetPath(gridGo) },
                tilemap = new { name = tmGo.name, instanceId = tmGo.GetInstanceID(), path = GameObjectFinder.GetPath(tmGo) },
            };
        }

        private static object SetTile(JToken p)
        {
            var tm = FindTilemap(p);
            var x = Validate.Required<int>(p, "x");
            var y = Validate.Required<int>(p, "y");
            var tilePath = p["tilePath"]?.Value<string>();

            Undo.RecordObject(tm, "Set Tile");
            if (string.IsNullOrEmpty(tilePath))
            {
                tm.SetTile(new Vector3Int(x, y, 0), null);
                return new { success = true, x, y, cleared = true };
            }

            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath);
            if (tile == null) throw new McpException(-32003, $"Tile not found: {tilePath}");
            tm.SetTile(new Vector3Int(x, y, 0), tile);
            return new { success = true, x, y, tile = tile.name };
        }

        private static object SetTilesBatch(JToken p)
        {
            var tm = FindTilemap(p);
            var items = Validate.Required<JArray>(p, "tiles");

            Undo.RecordObject(tm, "Set Tiles Batch");
            int count = 0;
            foreach (var item in items)
            {
                var x = item["x"]?.Value<int>() ?? 0;
                var y = item["y"]?.Value<int>() ?? 0;
                var tilePath = item["tilePath"]?.Value<string>();
                TileBase tile = null;
                if (!string.IsNullOrEmpty(tilePath))
                {
                    tile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath);
                    if (tile == null) continue;
                }
                tm.SetTile(new Vector3Int(x, y, 0), tile);
                count++;
            }
            return new { success = true, tilesSet = count };
        }

        private static object ClearTiles(JToken p)
        {
            var tm = FindTilemap(p);
            Undo.RecordObject(tm, "Clear Tiles");

            var bounds = p["bounds"];
            if (bounds != null)
            {
                var xMin = bounds["xMin"]?.Value<int>() ?? 0;
                var yMin = bounds["yMin"]?.Value<int>() ?? 0;
                var xMax = bounds["xMax"]?.Value<int>() ?? 0;
                var yMax = bounds["yMax"]?.Value<int>() ?? 0;
                for (int x = xMin; x <= xMax; x++)
                    for (int y = yMin; y <= yMax; y++)
                        tm.SetTile(new Vector3Int(x, y, 0), null);
                return new { success = true, message = "Region cleared" };
            }

            tm.ClearAllTiles();
            return new { success = true, message = "All tiles cleared" };
        }

        private static object GetTile(JToken p)
        {
            var tm = FindTilemap(p);
            var x = Validate.Required<int>(p, "x");
            var y = Validate.Required<int>(p, "y");
            var tile = tm.GetTile(new Vector3Int(x, y, 0));
            return new
            {
                x, y,
                hasTile = tile != null,
                tileName = tile?.name,
                tilePath = tile != null ? AssetDatabase.GetAssetPath(tile) : null,
            };
        }

        private static object GetInfo(JToken p)
        {
            var tm = FindTilemap(p);
            var bounds = tm.cellBounds;
            var renderer = tm.GetComponent<TilemapRenderer>();
            var grid = tm.layoutGrid;
            return new
            {
                name = tm.gameObject.name,
                path = GameObjectFinder.GetPath(tm.gameObject),
                cellBounds = new { bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax, bounds.size.x, bounds.size.y },
                cellSize = grid != null ? new { grid.cellSize.x, grid.cellSize.y } : null,
                tileCount = GetTileCount(tm),
                sortingLayer = renderer?.sortingLayerName,
                sortingOrder = renderer?.sortingOrder ?? 0,
            };
        }

        private static int GetTileCount(Tilemap tm)
        {
            int count = 0;
            var bounds = tm.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
                if (tm.HasTile(pos)) count++;
            return count;
        }

        private static object SetRenderer(JToken p)
        {
            var tm = FindTilemap(p);
            var renderer = tm.GetComponent<TilemapRenderer>();
            if (renderer == null) throw new McpException(-32010, "No TilemapRenderer found");
            Undo.RecordObject(renderer, "Set TilemapRenderer");

            if (p["sortingLayerName"] != null) renderer.sortingLayerName = p["sortingLayerName"].Value<string>();
            if (p["sortingOrder"] != null) renderer.sortingOrder = p["sortingOrder"].Value<int>();
            if (p["mode"] != null && Enum.TryParse<TilemapRenderer.Mode>(p["mode"].Value<string>(), true, out var mode))
                renderer.mode = mode;

            EditorUtility.SetDirty(renderer);
            return new { success = true, sortingLayer = renderer.sortingLayerName, sortingOrder = renderer.sortingOrder };
        }

        private static object Find(JToken p)
        {
            var tilemaps = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            var filter = p?["nameFilter"]?.Value<string>();
            if (!string.IsNullOrEmpty(filter))
                tilemaps = tilemaps.Where(t => t.gameObject.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            return new
            {
                count = tilemaps.Length,
                tilemaps = tilemaps.Select(t => new
                {
                    name = t.gameObject.name,
                    path = GameObjectFinder.GetPath(t.gameObject),
                    instanceId = t.gameObject.GetInstanceID(),
                    tileCount = GetTileCount(t),
                }).ToArray(),
            };
        }

        // ── Sprite ──────────────────────────────────────────────────

        private static object CreateSprite(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "Sprite";
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create Sprite");

            if (p["position"] != null)
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0);

            var sr = go.AddComponent<SpriteRenderer>();
            if (p["spritePath"] != null)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p["spritePath"].Value<string>());
                if (sprite != null) sr.sprite = sprite;
            }
            if (p["color"] != null)
                sr.color = new Color(
                    p["color"]["r"]?.Value<float>() ?? 1, p["color"]["g"]?.Value<float>() ?? 1,
                    p["color"]["b"]?.Value<float>() ?? 1, p["color"]["a"]?.Value<float>() ?? 1);
            if (p["sortingOrder"] != null) sr.sortingOrder = p["sortingOrder"].Value<int>();
            if (p["sortingLayerName"] != null) sr.sortingLayerName = p["sortingLayerName"].Value<string>();

            return new
            {
                success = true, name = go.name, instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
            };
        }

        private static object SetSpriteProperties(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) throw new McpException(-32010, $"No SpriteRenderer on '{go.name}'");
            Undo.RecordObject(sr, "Set Sprite Properties");

            if (p["spritePath"] != null)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p["spritePath"].Value<string>());
                if (sprite != null) sr.sprite = sprite;
            }
            if (p["color"] != null)
                sr.color = new Color(
                    p["color"]["r"]?.Value<float>() ?? 1, p["color"]["g"]?.Value<float>() ?? 1,
                    p["color"]["b"]?.Value<float>() ?? 1, p["color"]["a"]?.Value<float>() ?? 1);
            if (p["flipX"] != null) sr.flipX = p["flipX"].Value<bool>();
            if (p["flipY"] != null) sr.flipY = p["flipY"].Value<bool>();
            if (p["drawMode"] != null && Enum.TryParse<SpriteDrawMode>(p["drawMode"].Value<string>(), true, out var dm))
                sr.drawMode = dm;
            if (p["sortingOrder"] != null) sr.sortingOrder = p["sortingOrder"].Value<int>();
            if (p["sortingLayerName"] != null) sr.sortingLayerName = p["sortingLayerName"].Value<string>();
            if (p["materialPath"] != null)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(p["materialPath"].Value<string>());
                if (mat != null) sr.sharedMaterial = mat;
            }

            EditorUtility.SetDirty(sr);
            return new { success = true, gameObject = go.name };
        }

        private static object FindSprites(JToken p)
        {
            var sprites = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            var filter = p?["nameFilter"]?.Value<string>();
            if (!string.IsNullOrEmpty(filter))
                sprites = sprites.Where(s => s.gameObject.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            return new
            {
                count = sprites.Length,
                sprites = sprites.Select(s => new
                {
                    name = s.gameObject.name,
                    path = GameObjectFinder.GetPath(s.gameObject),
                    sprite = s.sprite?.name,
                    sortingLayer = s.sortingLayerName,
                    sortingOrder = s.sortingOrder,
                }).ToArray(),
            };
        }

        private static object SetSortingOrder(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) throw new McpException(-32010, $"No SpriteRenderer on '{go.name}'");
            Undo.RecordObject(sr, "Set Sorting Order");
            if (p["sortingOrder"] != null) sr.sortingOrder = p["sortingOrder"].Value<int>();
            if (p["sortingLayerName"] != null) sr.sortingLayerName = p["sortingLayerName"].Value<string>();
            EditorUtility.SetDirty(sr);
            return new { success = true, gameObject = go.name, sortingOrder = sr.sortingOrder, sortingLayer = sr.sortingLayerName };
        }
    }
}
