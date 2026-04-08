#if UNITY_2D_ANIMATION
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.U2D.IK;

namespace KarnelLabs.MCP
{
    public static class Skeletal2DHandler
    {
        public static void Register()
        {
            CommandRouter.Register("skeletal2d.addBone", AddBone);
            CommandRouter.Register("skeletal2d.addIK", AddIK);
            CommandRouter.Register("skeletal2d.addSpriteSkin", AddSpriteSkin);
            CommandRouter.Register("skeletal2d.getInfo", GetInfo);
            CommandRouter.Register("skeletal2d.find", Find);
        }

        private static object AddBone(JToken p)
        {
            string boneName = (string)p["name"] ?? "Bone";
            var go = new GameObject(boneName);
            Undo.RegisterCreatedObjectUndo(go, "Add 2D Bone");

            if (p["parent"] != null)
            {
                var parent = GameObjectFinder.FindByName((string)p["parent"]);
                go.transform.SetParent(parent.transform, false);
            }

            go.transform.localPosition = new Vector3(
                p["position"]?["x"]?.Value<float>() ?? 0,
                p["position"]?["y"]?.Value<float>() ?? 0,
                0
            );

            if (p["rotation"] != null)
                go.transform.localRotation = Quaternion.Euler(0, 0, (float)p["rotation"]);

            return new { success = true, gameObject = go.name, instanceId = go.GetInstanceID() };
        }

        private static object AddIK(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            string solverType = (string)p["solverType"];
            Undo.RecordObject(go, "Add IK Solver 2D");

            Component solver = solverType switch
            {
                "Limb" => (Component)Undo.AddComponent<LimbSolver2D>(go),
                "CCD" => Undo.AddComponent<CCDSolver2D>(go),
                "FABRIK" => Undo.AddComponent<FabrikSolver2D>(go),
                _ => throw new McpException(-32000, $"Unknown IK solver type: {solverType}")
            };

            return new { success = true, gameObject = go.name, solverType };
        }

        private static object AddSpriteSkin(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add SpriteSkin");
            var skin = Undo.AddComponent<SpriteSkin>(go);

            if (p["autoRebind"] != null && (bool)p["autoRebind"])
            {
                // Auto-bind bones if available
            }

            return new { success = true, gameObject = go.name, component = "SpriteSkin" };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);

            var spriteSkin = go.GetComponent<SpriteSkin>();
            var ikSolvers = go.GetComponents<Solver2D>();

            return new
            {
                gameObject = go.name,
                hasSpriteSkin = spriteSkin != null,
                boneTransformCount = spriteSkin?.boneTransforms?.Length ?? 0,
                ikSolverCount = ikSolvers.Length,
                ikSolvers = ikSolvers.Select(s => new { type = s.GetType().Name }).ToArray(),
            };
        }

        private static object Find(JToken p)
        {
            string type = (string)p?["type"] ?? "All";
            var results = new System.Collections.Generic.List<object>();

            if (type == "SpriteSkin" || type == "All")
            {
                var skins = Object.FindObjectsByType<SpriteSkin>(FindObjectsSortMode.None);
                foreach (var s in skins)
                    results.Add(new { gameObject = s.gameObject.name, path = GameObjectFinder.GetPath(s.gameObject), type = "SpriteSkin" });
            }
            if (type == "IK" || type == "All")
            {
                var solvers = Object.FindObjectsByType<Solver2D>(FindObjectsSortMode.None);
                foreach (var s in solvers)
                    results.Add(new { gameObject = s.gameObject.name, path = GameObjectFinder.GetPath(s.gameObject), type = s.GetType().Name });
            }

            return new { count = results.Count, components = results };
        }
    }
}
#endif
