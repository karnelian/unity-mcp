using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class GridLayoutHandler
    {
        public static void Register()
        {
            CommandRouter.Register("gridLayout.create", Create);
            CommandRouter.Register("gridLayout.set", Set);
            CommandRouter.Register("gridLayout.getInfo", GetInfo);
            CommandRouter.Register("gridLayout.find", Find);
        }

        private static object Create(JToken p)
        {
            string goName = (string)p["name"] ?? "Grid";
            var go = new GameObject(goName);
            Undo.RegisterCreatedObjectUndo(go, "Create Grid");
            var grid = go.AddComponent<Grid>();

            if (p["cellSize"] != null) grid.cellSize = JsonHelper.ToVector3(p["cellSize"]);
            if (p["cellGap"] != null) grid.cellGap = JsonHelper.ToVector3(p["cellGap"]);
            if (p["cellLayout"] != null)
            {
                grid.cellLayout = (string)p["cellLayout"] switch
                {
                    "Hexagon" => GridLayout.CellLayout.Hexagon,
                    "Isometric" => GridLayout.CellLayout.Isometric,
                    "IsometricZAsY" => GridLayout.CellLayout.IsometricZAsY,
                    _ => GridLayout.CellLayout.Rectangle
                };
            }
            if (p["cellSwizzle"] != null)
            {
                grid.cellSwizzle = (string)p["cellSwizzle"] switch
                {
                    "XZY" => GridLayout.CellSwizzle.XZY,
                    "YXZ" => GridLayout.CellSwizzle.YXZ,
                    "YZX" => GridLayout.CellSwizzle.YZX,
                    "ZXY" => GridLayout.CellSwizzle.ZXY,
                    "ZYX" => GridLayout.CellSwizzle.ZYX,
                    _ => GridLayout.CellSwizzle.XYZ
                };
            }

            return new { success = true, gameObject = go.name, instanceId = go.GetInstanceID() };
        }

        private static object Set(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var grid = go.GetComponent<Grid>();
            if (grid == null) throw new McpException(-32000, $"No Grid on '{go.name}'");

            Undo.RecordObject(grid, "Set Grid");
            if (p["cellSize"] != null) grid.cellSize = JsonHelper.ToVector3(p["cellSize"]);
            if (p["cellGap"] != null) grid.cellGap = JsonHelper.ToVector3(p["cellGap"]);
            if (p["cellLayout"] != null)
            {
                grid.cellLayout = (string)p["cellLayout"] switch
                {
                    "Hexagon" => GridLayout.CellLayout.Hexagon,
                    "Isometric" => GridLayout.CellLayout.Isometric,
                    "IsometricZAsY" => GridLayout.CellLayout.IsometricZAsY,
                    _ => GridLayout.CellLayout.Rectangle
                };
            }
            if (p["cellSwizzle"] != null)
            {
                grid.cellSwizzle = (string)p["cellSwizzle"] switch
                {
                    "XZY" => GridLayout.CellSwizzle.XZY,
                    "YXZ" => GridLayout.CellSwizzle.YXZ,
                    "YZX" => GridLayout.CellSwizzle.YZX,
                    "ZXY" => GridLayout.CellSwizzle.ZXY,
                    "ZYX" => GridLayout.CellSwizzle.ZYX,
                    _ => GridLayout.CellSwizzle.XYZ
                };
            }

            EditorUtility.SetDirty(grid);
            return new { success = true, gameObject = go.name };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var grid = go.GetComponent<Grid>();
            if (grid == null) throw new McpException(-32000, $"No Grid on '{go.name}'");

            return new
            {
                gameObject = go.name,
                cellSize = new { x = grid.cellSize.x, y = grid.cellSize.y, z = grid.cellSize.z },
                cellGap = new { x = grid.cellGap.x, y = grid.cellGap.y, z = grid.cellGap.z },
                cellLayout = grid.cellLayout.ToString(),
                cellSwizzle = grid.cellSwizzle.ToString(),
            };
        }

        private static object Find(JToken p)
        {
            var grids = Object.FindObjectsByType<Grid>(FindObjectsSortMode.None);
            var result = grids.Select(g => new
            {
                gameObject = g.gameObject.name,
                path = GameObjectFinder.GetPath(g.gameObject),
                cellLayout = g.cellLayout.ToString(),
                cellSize = new { x = g.cellSize.x, y = g.cellSize.y, z = g.cellSize.z },
            }).ToArray();

            return new { count = result.Length, grids = result };
        }
    }
}
