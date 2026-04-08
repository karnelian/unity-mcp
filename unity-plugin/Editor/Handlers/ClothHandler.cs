using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class ClothHandler
    {
        public static void Register()
        {
            CommandRouter.Register("cloth.add", Add);
            CommandRouter.Register("cloth.set", Set);
            CommandRouter.Register("cloth.getInfo", GetInfo);
            CommandRouter.Register("cloth.find", Find);
            CommandRouter.Register("cloth.setColliders", SetColliders);
        }

        private static object Add(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            if (go.GetComponent<SkinnedMeshRenderer>() == null)
                throw new McpException(-32000, $"'{go.name}' needs a SkinnedMeshRenderer for Cloth");

            Undo.RecordObject(go, "Add Cloth");
            var cloth = Undo.AddComponent<Cloth>(go);

            if (p["stretchingStiffness"] != null) cloth.stretchingStiffness = (float)p["stretchingStiffness"];
            if (p["bendingStiffness"] != null) cloth.bendingStiffness = (float)p["bendingStiffness"];
            if (p["damping"] != null) cloth.damping = (float)p["damping"];
            if (p["friction"] != null) cloth.friction = (float)p["friction"];
            if (p["worldVelocityScale"] != null) cloth.worldVelocityScale = (float)p["worldVelocityScale"];
            if (p["worldAccelerationScale"] != null) cloth.worldAccelerationScale = (float)p["worldAccelerationScale"];
            if (p["useGravity"] != null) cloth.useGravity = (bool)p["useGravity"];

            return new { success = true, gameObject = go.name, component = "Cloth" };
        }

        private static object Set(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var cloth = go.GetComponent<Cloth>();
            if (cloth == null) throw new McpException(-32000, $"No Cloth on '{go.name}'");

            Undo.RecordObject(cloth, "Set Cloth");
            if (p["stretchingStiffness"] != null) cloth.stretchingStiffness = (float)p["stretchingStiffness"];
            if (p["bendingStiffness"] != null) cloth.bendingStiffness = (float)p["bendingStiffness"];
            if (p["damping"] != null) cloth.damping = (float)p["damping"];
            if (p["friction"] != null) cloth.friction = (float)p["friction"];
            if (p["worldVelocityScale"] != null) cloth.worldVelocityScale = (float)p["worldVelocityScale"];
            if (p["worldAccelerationScale"] != null) cloth.worldAccelerationScale = (float)p["worldAccelerationScale"];
            if (p["useGravity"] != null) cloth.useGravity = (bool)p["useGravity"];
            if (p["externalAcceleration"] != null) cloth.externalAcceleration = JsonHelper.ToVector3(p["externalAcceleration"]);
            if (p["randomAcceleration"] != null) cloth.randomAcceleration = JsonHelper.ToVector3(p["randomAcceleration"]);
            if (p["clothSolverFrequency"] != null) cloth.clothSolverFrequency = (float)p["clothSolverFrequency"];
            if (p["sleepThreshold"] != null) cloth.sleepThreshold = (float)p["sleepThreshold"];

            EditorUtility.SetDirty(cloth);
            return new { success = true, gameObject = go.name };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var cloth = go.GetComponent<Cloth>();
            if (cloth == null) throw new McpException(-32000, $"No Cloth on '{go.name}'");

            return new
            {
                gameObject = go.name,
                stretchingStiffness = cloth.stretchingStiffness,
                bendingStiffness = cloth.bendingStiffness,
                damping = cloth.damping,
                friction = cloth.friction,
                worldVelocityScale = cloth.worldVelocityScale,
                worldAccelerationScale = cloth.worldAccelerationScale,
                useGravity = cloth.useGravity,
                externalAcceleration = new { x = cloth.externalAcceleration.x, y = cloth.externalAcceleration.y, z = cloth.externalAcceleration.z },
                vertexCount = cloth.vertices?.Length ?? 0,
                capsuleColliderCount = cloth.capsuleColliders?.Length ?? 0,
                sphereColliderCount = cloth.sphereColliders?.Length ?? 0,
            };
        }

        private static object Find(JToken p)
        {
            var cloths = Object.FindObjectsByType<Cloth>(FindObjectsSortMode.None);
            var result = cloths.Select(c => new
            {
                gameObject = c.gameObject.name,
                path = GameObjectFinder.GetPath(c.gameObject),
                vertexCount = c.vertices?.Length ?? 0,
            }).ToArray();

            return new { count = result.Length, cloths = result };
        }

        private static object SetColliders(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var cloth = go.GetComponent<Cloth>();
            if (cloth == null) throw new McpException(-32000, $"No Cloth on '{go.name}'");

            Undo.RecordObject(cloth, "Set Cloth Colliders");

            if (p["capsuleColliders"] is JArray capsuleNames)
            {
                var colliders = capsuleNames.Select(n =>
                {
                    var cgo = GameObjectFinder.FindByName((string)n);
                    return cgo.GetComponent<CapsuleCollider>();
                }).Where(c => c != null).ToArray();
                cloth.capsuleColliders = colliders;
            }

            if (p["sphereColliders"] is JArray sphereNames)
            {
                var pairs = new ClothSphereColliderPair[sphereNames.Count];
                for (int i = 0; i < sphereNames.Count; i++)
                {
                    var sgo = GameObjectFinder.FindByName((string)sphereNames[i]);
                    var sc = sgo.GetComponent<SphereCollider>();
                    pairs[i] = new ClothSphereColliderPair(sc);
                }
                cloth.sphereColliders = pairs;
            }

            EditorUtility.SetDirty(cloth);
            return new { success = true, gameObject = go.name };
        }
    }
}
