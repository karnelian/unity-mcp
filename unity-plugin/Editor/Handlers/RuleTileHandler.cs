#if UNITY_2D_TILEMAP_EXTRAS
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace KarnelLabs.MCP
{
    public static class RuleTileHandler
    {
        public static void Register()
        {
            CommandRouter.Register("ruleTile.create", Create);
            CommandRouter.Register("ruleTile.addRule", AddRule);
            CommandRouter.Register("ruleTile.getInfo", GetInfo);
            CommandRouter.Register("ruleTile.find", Find);
        }

        private static object Create(JToken p)
        {
            string path = (string)p["path"];
            var tile = ScriptableObject.CreateInstance<RuleTile>();

            if (p["defaultSprite"] != null)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>((string)p["defaultSprite"]);
                if (sprite != null) tile.m_DefaultSprite = sprite;
            }
            if (p["defaultColliderType"] != null)
            {
                tile.m_DefaultColliderType = (string)p["defaultColliderType"] switch
                {
                    "Sprite" => Tile.ColliderType.Sprite,
                    "Grid" => Tile.ColliderType.Grid,
                    _ => Tile.ColliderType.None
                };
            }

            AssetDatabase.CreateAsset(tile, path);
            AssetDatabase.SaveAssets();

            return new { success = true, path };
        }

        private static object AddRule(JToken p)
        {
            string path = (string)p["path"];
            var tile = AssetDatabase.LoadAssetAtPath<RuleTile>(path);
            if (tile == null) throw new McpException(-32000, $"RuleTile not found at '{path}'");

            Undo.RecordObject(tile, "Add Rule to RuleTile");

            var rule = new RuleTile.TilingRule();

            if (p["sprite"] != null)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>((string)p["sprite"]);
                if (sprite != null) rule.m_Sprites = new[] { sprite };
            }
            if (p["colliderType"] != null)
            {
                rule.m_ColliderType = (string)p["colliderType"] switch
                {
                    "Sprite" => Tile.ColliderType.Sprite,
                    "Grid" => Tile.ColliderType.Grid,
                    _ => Tile.ColliderType.None
                };
            }
            if (p["output"] != null)
            {
                rule.m_Output = (string)p["output"] switch
                {
                    "Random" => RuleTile.TilingRuleOutput.OutputSprite.Random,
                    "Animation" => RuleTile.TilingRuleOutput.OutputSprite.Animation,
                    _ => RuleTile.TilingRuleOutput.OutputSprite.Single
                };
            }
            if (p["neighbors"] is JArray neighbors)
            {
                rule.m_Neighbors = neighbors.Select(n => n.Value<int>()).ToList();
            }

            tile.m_TilingRules.Add(rule);
            EditorUtility.SetDirty(tile);
            AssetDatabase.SaveAssets();

            return new { success = true, path, ruleCount = tile.m_TilingRules.Count };
        }

        private static object GetInfo(JToken p)
        {
            string path = (string)p["path"];
            var tile = AssetDatabase.LoadAssetAtPath<RuleTile>(path);
            if (tile == null) throw new McpException(-32000, $"RuleTile not found at '{path}'");

            var rules = tile.m_TilingRules.Select((r, i) => new
            {
                index = i,
                spriteCount = r.m_Sprites?.Length ?? 0,
                output = r.m_Output.ToString(),
                colliderType = r.m_ColliderType.ToString(),
                neighborCount = r.m_Neighbors?.Count ?? 0,
            }).ToArray();

            return new
            {
                path,
                defaultSprite = tile.m_DefaultSprite != null ? tile.m_DefaultSprite.name : null,
                defaultColliderType = tile.m_DefaultColliderType.ToString(),
                ruleCount = tile.m_TilingRules.Count,
                rules,
            };
        }

        private static object Find(JToken p)
        {
            string nameFilter = (string)p?["nameFilter"];
            string folder = (string)p?["folder"];
            string[] searchFolders = !string.IsNullOrEmpty(folder) ? new[] { folder } : null;

            var guids = searchFolders != null
                ? AssetDatabase.FindAssets("t:RuleTile", searchFolders)
                : AssetDatabase.FindAssets("t:RuleTile");

            var tiles = guids.Select(g =>
            {
                string tilePath = AssetDatabase.GUIDToAssetPath(g);
                string tileName = System.IO.Path.GetFileNameWithoutExtension(tilePath);
                return new { name = tileName, path = tilePath };
            });

            if (!string.IsNullOrEmpty(nameFilter))
                tiles = tiles.Where(t => t.name.Contains(nameFilter, System.StringComparison.OrdinalIgnoreCase));

            var result = tiles.ToArray();
            return new { count = result.Length, ruleTiles = result };
        }
    }
}
#endif
