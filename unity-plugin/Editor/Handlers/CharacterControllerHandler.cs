using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class CharacterControllerHandler
    {
        public static void Register()
        {
            CommandRouter.Register("characterController.add", Add);
            CommandRouter.Register("characterController.get", Get);
            CommandRouter.Register("characterController.set", Set);
            CommandRouter.Register("characterController.find", Find);
        }

        private static object Add(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add CharacterController");
            var cc = Undo.AddComponent<CharacterController>(go);

            if (p["height"] != null) cc.height = (float)p["height"];
            if (p["radius"] != null) cc.radius = (float)p["radius"];
            if (p["center"] != null) cc.center = JsonHelper.ToVector3(p["center"]);
            if (p["slopeLimit"] != null) cc.slopeLimit = (float)p["slopeLimit"];
            if (p["stepOffset"] != null) cc.stepOffset = (float)p["stepOffset"];
            if (p["skinWidth"] != null) cc.skinWidth = (float)p["skinWidth"];
            if (p["minMoveDistance"] != null) cc.minMoveDistance = (float)p["minMoveDistance"];

            return new { success = true, gameObject = go.name, component = "CharacterController" };
        }

        private static object Get(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var cc = go.GetComponent<CharacterController>();
            if (cc == null) throw new McpException(-32000, $"No CharacterController on '{go.name}'");

            return new
            {
                gameObject = go.name,
                height = cc.height,
                radius = cc.radius,
                center = new { x = cc.center.x, y = cc.center.y, z = cc.center.z },
                slopeLimit = cc.slopeLimit,
                stepOffset = cc.stepOffset,
                skinWidth = cc.skinWidth,
                minMoveDistance = cc.minMoveDistance,
                isGrounded = cc.isGrounded,
                velocity = new { x = cc.velocity.x, y = cc.velocity.y, z = cc.velocity.z },
            };
        }

        private static object Set(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var cc = go.GetComponent<CharacterController>();
            if (cc == null) throw new McpException(-32000, $"No CharacterController on '{go.name}'");

            Undo.RecordObject(cc, "Set CharacterController");
            if (p["height"] != null) cc.height = (float)p["height"];
            if (p["radius"] != null) cc.radius = (float)p["radius"];
            if (p["center"] != null) cc.center = JsonHelper.ToVector3(p["center"]);
            if (p["slopeLimit"] != null) cc.slopeLimit = (float)p["slopeLimit"];
            if (p["stepOffset"] != null) cc.stepOffset = (float)p["stepOffset"];
            if (p["skinWidth"] != null) cc.skinWidth = (float)p["skinWidth"];
            if (p["minMoveDistance"] != null) cc.minMoveDistance = (float)p["minMoveDistance"];

            EditorUtility.SetDirty(cc);
            return new { success = true, gameObject = go.name };
        }

        private static object Find(JToken p)
        {
            var controllers = Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
            var result = controllers.Select(cc => new
            {
                gameObject = cc.gameObject.name,
                path = GameObjectFinder.GetPath(cc.gameObject),
                height = cc.height,
                radius = cc.radius,
                isGrounded = cc.isGrounded,
            }).ToArray();

            return new { count = result.Length, controllers = result };
        }
    }
}
