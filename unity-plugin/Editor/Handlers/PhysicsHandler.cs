using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class PhysicsHandler
    {
        public static void Register()
        {
            CommandRouter.Register("physics.raycast", Raycast);
            CommandRouter.Register("physics.overlapSphere", OverlapSphere);
            CommandRouter.Register("physics.overlapBox", OverlapBox);
            CommandRouter.Register("physics.setGravity", SetGravity);
            CommandRouter.Register("physics.getGravity", GetGravity);
            CommandRouter.Register("physics.addRigidbody", AddRigidbody);
            CommandRouter.Register("physics.setRigidbody", SetRigidbody);
            CommandRouter.Register("physics.addCollider", AddCollider);
            CommandRouter.Register("physics.setCollider", SetCollider);
            CommandRouter.Register("physics.createPhysicsMaterial", CreatePhysicsMaterial);
            CommandRouter.Register("physics.setLayerCollision", SetLayerCollision);
            CommandRouter.Register("physics.getLayerCollisionMatrix", GetLayerCollisionMatrix);
            CommandRouter.Register("physics.sphereCast", SphereCast);
            CommandRouter.Register("physics.boxCast", BoxCast);
            CommandRouter.Register("physics.capsuleCast", CapsuleCast);
            CommandRouter.Register("physics.linecast", Linecast);
            CommandRouter.Register("physics.closestPoint", ClosestPoint);
        }

        private static Vector3 ParseVec3(JToken t, float defX = 0, float defY = 0, float defZ = 0)
        {
            if (t == null) return new Vector3(defX, defY, defZ);
            return new Vector3(t["x"]?.Value<float>() ?? defX, t["y"]?.Value<float>() ?? defY, t["z"]?.Value<float>() ?? defZ);
        }

        private static object HitInfo(RaycastHit hit)
        {
            return new
            {
                gameObject = hit.collider.gameObject.name,
                instanceId = hit.collider.gameObject.GetInstanceID(),
                path = GameObjectFinder.GetPath(hit.collider.gameObject),
                point = new { hit.point.x, hit.point.y, hit.point.z },
                normal = new { hit.normal.x, hit.normal.y, hit.normal.z },
                distance = hit.distance,
            };
        }

        private static object Raycast(JToken p)
        {
            var origin = ParseVec3(Validate.Required<JToken>(p, "origin"));
            var direction = ParseVec3(Validate.Required<JToken>(p, "direction"));
            var maxDistance = p["maxDistance"]?.Value<float>() ?? Mathf.Infinity;
            var layerMask = p["layerMask"]?.Value<int>() ?? -1;

            if (Physics.Raycast(origin, direction, out var hit, maxDistance, layerMask))
                return new { hit = true, result = HitInfo(hit) };
            return new { hit = false };
        }

        private static object OverlapSphere(JToken p)
        {
            var center = ParseVec3(Validate.Required<JToken>(p, "center"));
            var radius = Validate.Required<float>(p, "radius");
            var layerMask = p["layerMask"]?.Value<int>() ?? -1;

            var colliders = Physics.OverlapSphere(center, radius, layerMask);
            return new
            {
                count = colliders.Length,
                objects = colliders.Select(c => new
                {
                    name = c.gameObject.name,
                    instanceId = c.gameObject.GetInstanceID(),
                    path = GameObjectFinder.GetPath(c.gameObject),
                }).ToArray(),
            };
        }

        private static object OverlapBox(JToken p)
        {
            var center = ParseVec3(Validate.Required<JToken>(p, "center"));
            var halfExtents = ParseVec3(Validate.Required<JToken>(p, "halfExtents"));
            var layerMask = p["layerMask"]?.Value<int>() ?? -1;

            var colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, layerMask);
            return new
            {
                count = colliders.Length,
                objects = colliders.Select(c => new
                {
                    name = c.gameObject.name,
                    instanceId = c.gameObject.GetInstanceID(),
                    path = GameObjectFinder.GetPath(c.gameObject),
                }).ToArray(),
            };
        }

        private static object SetGravity(JToken p)
        {
            var gravity = ParseVec3(Validate.Required<JToken>(p, "gravity"), 0, -9.81f, 0);
            Physics.gravity = gravity;
            return new { gravity = new { Physics.gravity.x, Physics.gravity.y, Physics.gravity.z } };
        }

        private static object GetGravity(JToken p)
        {
            return new { gravity = new { Physics.gravity.x, Physics.gravity.y, Physics.gravity.z } };
        }

        private static object AddRigidbody(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (go.GetComponent<Rigidbody>() != null)
                throw new McpException(-32602, $"'{go.name}' already has a Rigidbody");

            WorkflowManager.SnapshotObject(go);
            var rb = Undo.AddComponent<Rigidbody>(go);

            if (p["mass"] != null) rb.mass = p["mass"].Value<float>();
            if (p["drag"] != null) rb.linearDamping = p["drag"].Value<float>();
            if (p["angularDrag"] != null) rb.angularDamping = p["angularDrag"].Value<float>();
            if (p["useGravity"] != null) rb.useGravity = p["useGravity"].Value<bool>();
            if (p["isKinematic"] != null) rb.isKinematic = p["isKinematic"].Value<bool>();

            return new { added = true, gameObject = go.name, mass = rb.mass, useGravity = rb.useGravity, isKinematic = rb.isKinematic };
        }

        private static object SetRigidbody(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) throw new McpException(-32602, $"No Rigidbody on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(rb, "MCP: Set Rigidbody");

            if (p["mass"] != null) rb.mass = p["mass"].Value<float>();
            if (p["drag"] != null) rb.linearDamping = p["drag"].Value<float>();
            if (p["angularDrag"] != null) rb.angularDamping = p["angularDrag"].Value<float>();
            if (p["useGravity"] != null) rb.useGravity = p["useGravity"].Value<bool>();
            if (p["isKinematic"] != null) rb.isKinematic = p["isKinematic"].Value<bool>();
            if (p["constraints"] != null) rb.constraints = (RigidbodyConstraints)p["constraints"].Value<int>();

            return new { gameObject = go.name, mass = rb.mass, useGravity = rb.useGravity, isKinematic = rb.isKinematic };
        }

        private static object AddCollider(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var colliderType = Validate.Required<string>(p, "colliderType");

            WorkflowManager.SnapshotObject(go);
            Collider col;
            switch (colliderType.ToLower())
            {
                case "box": col = Undo.AddComponent<BoxCollider>(go); break;
                case "sphere": col = Undo.AddComponent<SphereCollider>(go); break;
                case "capsule": col = Undo.AddComponent<CapsuleCollider>(go); break;
                case "mesh":
                    var mc = Undo.AddComponent<MeshCollider>(go);
                    if (p["convex"] != null) mc.convex = p["convex"].Value<bool>();
                    col = mc;
                    break;
                default: throw new McpException(-32602, $"Unknown collider type: {colliderType}. Use: box, sphere, capsule, mesh");
            }

            if (p["isTrigger"] != null) col.isTrigger = p["isTrigger"].Value<bool>();

            return new { added = true, gameObject = go.name, colliderType, isTrigger = col.isTrigger };
        }

        private static object SetCollider(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var colliderType = p["colliderType"]?.Value<string>();
            Collider col;

            if (!string.IsNullOrEmpty(colliderType))
            {
                switch (colliderType.ToLower())
                {
                    case "box": col = go.GetComponent<BoxCollider>(); break;
                    case "sphere": col = go.GetComponent<SphereCollider>(); break;
                    case "capsule": col = go.GetComponent<CapsuleCollider>(); break;
                    case "mesh": col = go.GetComponent<MeshCollider>(); break;
                    default: throw new McpException(-32602, $"Unknown collider type: {colliderType}");
                }
            }
            else
            {
                col = go.GetComponent<Collider>();
            }

            if (col == null) throw new McpException(-32602, $"No Collider on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(col, "MCP: Set Collider");
            if (p["isTrigger"] != null) col.isTrigger = p["isTrigger"].Value<bool>();
            if (p["enabled"] != null) col.enabled = p["enabled"].Value<bool>();

            // Type-specific
            if (col is BoxCollider box)
            {
                if (p["center"] != null) box.center = ParseVec3(p["center"]);
                if (p["size"] != null) box.size = ParseVec3(p["size"], 1, 1, 1);
            }
            else if (col is SphereCollider sphere)
            {
                if (p["center"] != null) sphere.center = ParseVec3(p["center"]);
                if (p["radius"] != null) sphere.radius = p["radius"].Value<float>();
            }
            else if (col is CapsuleCollider capsule)
            {
                if (p["center"] != null) capsule.center = ParseVec3(p["center"]);
                if (p["radius"] != null) capsule.radius = p["radius"].Value<float>();
                if (p["height"] != null) capsule.height = p["height"].Value<float>();
            }

            return new { gameObject = go.name, colliderType = col.GetType().Name, isTrigger = col.isTrigger };
        }

        private static object CreatePhysicsMaterial(JToken p)
        {
            var matName = Validate.Required<string>(p, "name");
            var savePath = p["savePath"]?.Value<string>() ?? $"Assets/PhysicsMaterials/{matName}.physicMaterial";
            savePath = Validate.SafeAssetPath(savePath);

            var dir = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(Application.dataPath, "..", savePath));
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

            var mat = new PhysicsMaterial(matName);
            if (p["dynamicFriction"] != null) mat.dynamicFriction = p["dynamicFriction"].Value<float>();
            if (p["staticFriction"] != null) mat.staticFriction = p["staticFriction"].Value<float>();
            if (p["bounciness"] != null) mat.bounciness = p["bounciness"].Value<float>();
            if (p["frictionCombine"] != null) mat.frictionCombine = Validate.ParseEnum<PhysicsMaterialCombine>(p["frictionCombine"].Value<string>(), "frictionCombine");
            if (p["bounceCombine"] != null) mat.bounceCombine = Validate.ParseEnum<PhysicsMaterialCombine>(p["bounceCombine"].Value<string>(), "bounceCombine");

            AssetDatabase.CreateAsset(mat, savePath);
            AssetDatabase.SaveAssets();
            return new { name = matName, path = savePath, dynamicFriction = mat.dynamicFriction, staticFriction = mat.staticFriction, bounciness = mat.bounciness };
        }

        private static object SetLayerCollision(JToken p)
        {
            var layer1 = Validate.Required<int>(p, "layer1");
            var layer2 = Validate.Required<int>(p, "layer2");
            var ignore = p["ignore"]?.Value<bool>() ?? true;

            Physics.IgnoreLayerCollision(layer1, layer2, ignore);
            return new { layer1, layer2, ignore };
        }

        private static object GetLayerCollisionMatrix(JToken p)
        {
            var matrix = new Dictionary<string, object>();
            for (int i = 0; i < 32; i++)
            {
                var layerName = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(layerName)) continue;
                var collisions = new List<string>();
                for (int j = 0; j < 32; j++)
                {
                    var otherName = LayerMask.LayerToName(j);
                    if (string.IsNullOrEmpty(otherName)) continue;
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                        collisions.Add(otherName);
                }
                matrix[layerName] = collisions;
            }
            return new { matrix };
        }

        private static object SphereCast(JToken p)
        {
            var origin = ParseVec3(Validate.Required<JToken>(p, "origin"));
            var direction = ParseVec3(Validate.Required<JToken>(p, "direction"));
            var radius = Validate.Required<float>(p, "radius");
            var maxDistance = p["maxDistance"]?.Value<float>() ?? Mathf.Infinity;
            var layerMask = p["layerMask"]?.Value<int>() ?? -1;

            if (Physics.SphereCast(origin, radius, direction, out var hit, maxDistance, layerMask))
                return new { hit = true, result = HitInfo(hit) };
            return new { hit = false };
        }

        private static object BoxCast(JToken p)
        {
            var center = ParseVec3(Validate.Required<JToken>(p, "center"));
            var halfExtents = ParseVec3(Validate.Required<JToken>(p, "halfExtents"));
            var direction = ParseVec3(Validate.Required<JToken>(p, "direction"));
            var maxDistance = p["maxDistance"]?.Value<float>() ?? Mathf.Infinity;
            var layerMask = p["layerMask"]?.Value<int>() ?? -1;

            if (Physics.BoxCast(center, halfExtents, direction, out var hit, Quaternion.identity, maxDistance, layerMask))
                return new { hit = true, result = HitInfo(hit) };
            return new { hit = false };
        }

        private static object CapsuleCast(JToken p)
        {
            var point1 = ParseVec3(Validate.Required<JToken>(p, "point1"));
            var point2 = ParseVec3(Validate.Required<JToken>(p, "point2"));
            var radius = Validate.Required<float>(p, "radius");
            var direction = ParseVec3(Validate.Required<JToken>(p, "direction"));
            var maxDistance = p["maxDistance"]?.Value<float>() ?? Mathf.Infinity;
            var layerMask = p["layerMask"]?.Value<int>() ?? -1;

            if (Physics.CapsuleCast(point1, point2, radius, direction, out var hit, maxDistance, layerMask))
                return new { hit = true, result = HitInfo(hit) };
            return new { hit = false };
        }

        private static object Linecast(JToken p)
        {
            var start = ParseVec3(Validate.Required<JToken>(p, "start"));
            var end = ParseVec3(Validate.Required<JToken>(p, "end"));
            var layerMask = p["layerMask"]?.Value<int>() ?? -1;

            if (Physics.Linecast(start, end, out var hit, layerMask))
                return new { hit = true, result = HitInfo(hit) };
            return new { hit = false };
        }

        private static object ClosestPoint(JToken p)
        {
            var point = ParseVec3(Validate.Required<JToken>(p, "point"));
            var go = GameObjectFinder.FindOrThrow(p);
            var collider = go.GetComponent<Collider>();
            if (collider == null) throw new McpException(-32010, $"No Collider on '{go.name}'");
            var closest = Physics.ClosestPoint(point, collider, collider.transform.position, collider.transform.rotation);
            return new
            {
                point = new { closest.x, closest.y, closest.z },
                gameObject = go.name,
                distance = Vector3.Distance(point, closest),
            };
        }
    }
}
