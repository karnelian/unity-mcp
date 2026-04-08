using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class JointHandler
    {
        public static void Register()
        {
            CommandRouter.Register("joint.addHinge", AddHinge);
            CommandRouter.Register("joint.addSpring", AddSpring);
            CommandRouter.Register("joint.addFixed", AddFixed);
            CommandRouter.Register("joint.addCharacter", AddCharacter);
            CommandRouter.Register("joint.addConfigurable", AddConfigurable);
            CommandRouter.Register("joint.getInfo", GetInfo);
            CommandRouter.Register("joint.setProperties", SetProperties);
            CommandRouter.Register("joint.remove", Remove);
            CommandRouter.Register("joint.find", Find);
        }

        private static object AddHinge(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add HingeJoint");
            var hj = Undo.AddComponent<HingeJoint>(go);

            if (p["connectedBody"] != null)
            {
                var target = GameObjectFinder.FindByName((string)p["connectedBody"]);
                var rb = target.GetComponent<Rigidbody>();
                if (rb != null) hj.connectedBody = rb;
            }
            if (p["anchor"] != null) hj.anchor = JsonHelper.ToVector3(p["anchor"]);
            if (p["axis"] != null) hj.axis = JsonHelper.ToVector3(p["axis"]);
            if (p["useSpring"] != null) hj.useSpring = (bool)p["useSpring"];
            if (p["useLimits"] != null) hj.useLimits = (bool)p["useLimits"];
            if (p["useMotor"] != null) hj.useMotor = (bool)p["useMotor"];

            return new { success = true, gameObject = go.name, component = "HingeJoint" };
        }

        private static object AddSpring(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add SpringJoint");
            var sj = Undo.AddComponent<SpringJoint>(go);

            if (p["connectedBody"] != null)
            {
                var target = GameObjectFinder.FindByName((string)p["connectedBody"]);
                var rb = target.GetComponent<Rigidbody>();
                if (rb != null) sj.connectedBody = rb;
            }
            if (p["spring"] != null) sj.spring = (float)p["spring"];
            if (p["damper"] != null) sj.damper = (float)p["damper"];
            if (p["minDistance"] != null) sj.minDistance = (float)p["minDistance"];
            if (p["maxDistance"] != null) sj.maxDistance = (float)p["maxDistance"];
            if (p["tolerance"] != null) sj.tolerance = (float)p["tolerance"];

            return new { success = true, gameObject = go.name, component = "SpringJoint" };
        }

        private static object AddFixed(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add FixedJoint");
            var fj = Undo.AddComponent<FixedJoint>(go);

            if (p["connectedBody"] != null)
            {
                var target = GameObjectFinder.FindByName((string)p["connectedBody"]);
                var rb = target.GetComponent<Rigidbody>();
                if (rb != null) fj.connectedBody = rb;
            }
            if (p["breakForce"] != null) fj.breakForce = (float)p["breakForce"];
            if (p["breakTorque"] != null) fj.breakTorque = (float)p["breakTorque"];

            return new { success = true, gameObject = go.name, component = "FixedJoint" };
        }

        private static object AddCharacter(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add CharacterJoint");
            var cj = Undo.AddComponent<CharacterJoint>(go);

            if (p["connectedBody"] != null)
            {
                var target = GameObjectFinder.FindByName((string)p["connectedBody"]);
                var rb = target.GetComponent<Rigidbody>();
                if (rb != null) cj.connectedBody = rb;
            }
            if (p["anchor"] != null) cj.anchor = JsonHelper.ToVector3(p["anchor"]);
            if (p["axis"] != null) cj.axis = JsonHelper.ToVector3(p["axis"]);
            if (p["swingAxis"] != null) cj.swingAxis = JsonHelper.ToVector3(p["swingAxis"]);

            return new { success = true, gameObject = go.name, component = "CharacterJoint" };
        }

        private static object AddConfigurable(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add ConfigurableJoint");
            var cj = Undo.AddComponent<ConfigurableJoint>(go);

            if (p["connectedBody"] != null)
            {
                var target = GameObjectFinder.FindByName((string)p["connectedBody"]);
                var rb = target.GetComponent<Rigidbody>();
                if (rb != null) cj.connectedBody = rb;
            }
            if (p["anchor"] != null) cj.anchor = JsonHelper.ToVector3(p["anchor"]);
            if (p["axis"] != null) cj.axis = JsonHelper.ToVector3(p["axis"]);

            return new { success = true, gameObject = go.name, component = "ConfigurableJoint" };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var joints = go.GetComponents<Joint>();
            var info = joints.Select(j => new
            {
                type = j.GetType().Name,
                connectedBody = j.connectedBody != null ? j.connectedBody.gameObject.name : null,
                breakForce = j.breakForce,
                breakTorque = j.breakTorque,
                anchor = new { x = j.anchor.x, y = j.anchor.y, z = j.anchor.z },
                axis = new { x = j.axis.x, y = j.axis.y, z = j.axis.z },
            }).ToArray();

            return new { gameObject = go.name, jointCount = info.Length, joints = info };
        }

        private static object SetProperties(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            string jointType = (string)p["jointType"];
            int index = p["index"]?.Value<int>() ?? 0;

            var joints = go.GetComponents<Joint>();
            if (jointType != null)
                joints = joints.Where(j => j.GetType().Name == jointType).ToArray();

            if (index >= joints.Length)
                throw new McpException(-32000, $"Joint index {index} out of range (count: {joints.Length})");

            var joint = joints[index];
            Undo.RecordObject(joint, "Set Joint Properties");

            if (p["breakForce"] != null) joint.breakForce = (float)p["breakForce"];
            if (p["breakTorque"] != null) joint.breakTorque = (float)p["breakTorque"];
            if (p["anchor"] != null) joint.anchor = JsonHelper.ToVector3(p["anchor"]);
            if (p["axis"] != null) joint.axis = JsonHelper.ToVector3(p["axis"]);
            if (p["enableCollision"] != null) joint.enableCollision = (bool)p["enableCollision"];
            if (p["enablePreprocessing"] != null) joint.enablePreprocessing = (bool)p["enablePreprocessing"];

            if (p["connectedBody"] != null)
            {
                var target = GameObjectFinder.FindByName((string)p["connectedBody"]);
                var rb = target.GetComponent<Rigidbody>();
                if (rb != null) joint.connectedBody = rb;
            }

            EditorUtility.SetDirty(joint);
            return new { success = true, gameObject = go.name, jointType = joint.GetType().Name };
        }

        private static object Remove(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            string jointType = (string)p["jointType"];
            int index = p["index"]?.Value<int>() ?? 0;

            var joints = go.GetComponents<Joint>();
            if (jointType != null)
                joints = joints.Where(j => j.GetType().Name == jointType).ToArray();

            if (index >= joints.Length)
                throw new McpException(-32000, $"Joint index {index} out of range (count: {joints.Length})");

            Undo.DestroyObjectImmediate(joints[index]);
            return new { success = true, gameObject = go.name };
        }

        private static object Find(JToken p)
        {
            string jointType = (string)p?["jointType"];
            var allJoints = Object.FindObjectsByType<Joint>(FindObjectsSortMode.None);

            if (jointType != null)
                allJoints = allJoints.Where(j => j.GetType().Name == jointType).ToArray();

            var result = allJoints.Select(j => new
            {
                gameObject = j.gameObject.name,
                path = GameObjectFinder.GetPath(j.gameObject),
                jointType = j.GetType().Name,
                connectedBody = j.connectedBody != null ? j.connectedBody.gameObject.name : null,
            }).ToArray();

            return new { count = result.Length, joints = result };
        }
    }
}
