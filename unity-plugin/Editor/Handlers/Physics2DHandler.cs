using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class Physics2DHandler
    {
        public static void Register()
        {
            CommandRouter.Register("physics2d.addRigidbody", AddRigidbody);
            CommandRouter.Register("physics2d.setRigidbody", SetRigidbody);
            CommandRouter.Register("physics2d.addCollider", AddCollider);
            CommandRouter.Register("physics2d.setCollider", SetCollider);
            CommandRouter.Register("physics2d.addJoint", AddJoint);
            CommandRouter.Register("physics2d.raycast", Raycast);
            CommandRouter.Register("physics2d.overlapCircle", OverlapCircle);
            CommandRouter.Register("physics2d.overlapBox", OverlapBox);
            CommandRouter.Register("physics2d.createMaterial", CreateMaterial);
            CommandRouter.Register("physics2d.getGravity", GetGravity);
            CommandRouter.Register("physics2d.setGravity", SetGravity);
            CommandRouter.Register("physics2d.addEffector", AddEffector);
            CommandRouter.Register("physics2d.setEffector", SetEffector);
        }

        private static object AddRigidbody(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add Rigidbody2D");
            var rb = Undo.AddComponent<Rigidbody2D>(go);

            if (p["bodyType"] != null)
            {
                string bt = (string)p["bodyType"];
                rb.bodyType = bt switch
                {
                    "Dynamic" => RigidbodyType2D.Dynamic,
                    "Kinematic" => RigidbodyType2D.Kinematic,
                    "Static" => RigidbodyType2D.Static,
                    _ => RigidbodyType2D.Dynamic
                };
            }
            if (p["mass"] != null) rb.mass = (float)p["mass"];
            if (p["linearDamping"] != null) rb.linearDamping = (float)p["linearDamping"];
            if (p["angularDamping"] != null) rb.angularDamping = (float)p["angularDamping"];
            if (p["gravityScale"] != null) rb.gravityScale = (float)p["gravityScale"];
            if (p["freezeRotation"] != null) rb.freezeRotation = (bool)p["freezeRotation"];

            return new { success = true, gameObject = go.name, component = "Rigidbody2D" };
        }

        private static object SetRigidbody(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) throw new McpException(-32000, $"No Rigidbody2D on '{go.name}'");

            Undo.RecordObject(rb, "Set Rigidbody2D");
            if (p["bodyType"] != null)
            {
                rb.bodyType = (string)p["bodyType"] switch
                {
                    "Dynamic" => RigidbodyType2D.Dynamic,
                    "Kinematic" => RigidbodyType2D.Kinematic,
                    "Static" => RigidbodyType2D.Static,
                    _ => rb.bodyType
                };
            }
            if (p["mass"] != null) rb.mass = (float)p["mass"];
            if (p["linearDamping"] != null) rb.linearDamping = (float)p["linearDamping"];
            if (p["angularDamping"] != null) rb.angularDamping = (float)p["angularDamping"];
            if (p["gravityScale"] != null) rb.gravityScale = (float)p["gravityScale"];
            if (p["freezeRotation"] != null) rb.freezeRotation = (bool)p["freezeRotation"];

            EditorUtility.SetDirty(rb);
            return new { success = true, gameObject = go.name };
        }

        private static object AddCollider(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            string type = (string)p["colliderType"] ?? "Box";
            Undo.RecordObject(go, "Add Collider2D");

            Collider2D col = type switch
            {
                "Box" => (Collider2D)Undo.AddComponent<BoxCollider2D>(go),
                "Circle" => Undo.AddComponent<CircleCollider2D>(go),
                "Capsule" => Undo.AddComponent<CapsuleCollider2D>(go),
                "Polygon" => Undo.AddComponent<PolygonCollider2D>(go),
                "Edge" => Undo.AddComponent<EdgeCollider2D>(go),
                "Composite" => Undo.AddComponent<CompositeCollider2D>(go),
                _ => throw new McpException(-32000, $"Unknown 2D collider type: {type}")
            };

            if (p["isTrigger"] != null) col.isTrigger = (bool)p["isTrigger"];
            if (p["offset"] != null) col.offset = new Vector2((float)p["offset"]["x"], (float)p["offset"]["y"]);

            if (col is BoxCollider2D box && p["size"] != null)
                box.size = new Vector2((float)p["size"]["x"], (float)p["size"]["y"]);
            if (col is CircleCollider2D circle && p["radius"] != null)
                circle.radius = (float)p["radius"];
            if (col is CapsuleCollider2D capsule)
            {
                if (p["size"] != null) capsule.size = new Vector2((float)p["size"]["x"], (float)p["size"]["y"]);
            }

            return new { success = true, gameObject = go.name, colliderType = type };
        }

        private static object SetCollider(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var col = go.GetComponent<Collider2D>();
            if (col == null) throw new McpException(-32000, $"No Collider2D on '{go.name}'");

            Undo.RecordObject(col, "Set Collider2D");
            if (p["isTrigger"] != null) col.isTrigger = (bool)p["isTrigger"];
            if (p["offset"] != null) col.offset = new Vector2((float)p["offset"]["x"], (float)p["offset"]["y"]);

            if (col is BoxCollider2D box && p["size"] != null)
                box.size = new Vector2((float)p["size"]["x"], (float)p["size"]["y"]);
            if (col is CircleCollider2D circle && p["radius"] != null)
                circle.radius = (float)p["radius"];

            EditorUtility.SetDirty(col);
            return new { success = true, gameObject = go.name, colliderType = col.GetType().Name };
        }

        private static object AddJoint(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            string type = (string)p["jointType"] ?? "Distance";
            Undo.RecordObject(go, "Add Joint2D");

            Joint2D joint = type switch
            {
                "Distance" => (Joint2D)Undo.AddComponent<DistanceJoint2D>(go),
                "Fixed" => Undo.AddComponent<FixedJoint2D>(go),
                "Friction" => Undo.AddComponent<FrictionJoint2D>(go),
                "Hinge" => Undo.AddComponent<HingeJoint2D>(go),
                "Relative" => Undo.AddComponent<RelativeJoint2D>(go),
                "Slider" => Undo.AddComponent<SliderJoint2D>(go),
                "Spring" => Undo.AddComponent<SpringJoint2D>(go),
                "Target" => Undo.AddComponent<TargetJoint2D>(go),
                "Wheel" => Undo.AddComponent<WheelJoint2D>(go),
                _ => throw new McpException(-32000, $"Unknown 2D joint type: {type}")
            };

            if (p["connectedBody"] != null)
            {
                var target = GameObjectFinder.FindByName((string)p["connectedBody"]);
                var rb2d = target.GetComponent<Rigidbody2D>();
                if (rb2d != null) joint.connectedBody = rb2d;
            }

            return new { success = true, gameObject = go.name, jointType = type };
        }

        private static object Raycast(JToken p)
        {
            var origin = new Vector2((float)p["origin"]["x"], (float)p["origin"]["y"]);
            var direction = new Vector2((float)p["direction"]["x"], (float)p["direction"]["y"]);
            float distance = p["distance"]?.Value<float>() ?? Mathf.Infinity;

            var hit = Physics2D.Raycast(origin, direction, distance);

            if (hit.collider == null)
                return new { hit = false };

            return new
            {
                hit = true,
                point = new { x = hit.point.x, y = hit.point.y },
                normal = new { x = hit.normal.x, y = hit.normal.y },
                distance = hit.distance,
                gameObject = hit.collider.gameObject.name,
                path = GameObjectFinder.GetPath(hit.collider.gameObject),
            };
        }

        private static object OverlapCircle(JToken p)
        {
            var point = new Vector2((float)p["point"]["x"], (float)p["point"]["y"]);
            float radius = (float)p["radius"];

            var hits = Physics2D.OverlapCircleAll(point, radius);
            var result = hits.Select(c => new
            {
                gameObject = c.gameObject.name,
                path = GameObjectFinder.GetPath(c.gameObject),
                colliderType = c.GetType().Name,
            }).ToArray();

            return new { count = result.Length, results = result };
        }

        private static object OverlapBox(JToken p)
        {
            var point = new Vector2((float)p["point"]["x"], (float)p["point"]["y"]);
            var size = new Vector2((float)p["size"]["x"], (float)p["size"]["y"]);
            float angle = p["angle"]?.Value<float>() ?? 0f;

            var hits = Physics2D.OverlapBoxAll(point, size, angle);
            var result = hits.Select(c => new
            {
                gameObject = c.gameObject.name,
                path = GameObjectFinder.GetPath(c.gameObject),
                colliderType = c.GetType().Name,
            }).ToArray();

            return new { count = result.Length, results = result };
        }

        private static object CreateMaterial(JToken p)
        {
            string path = (string)p["path"] ?? "Assets/New Physics2D Material.physicsMaterial2D";
            var mat = new PhysicsMaterial2D();
            if (p["friction"] != null) mat.friction = (float)p["friction"];
            if (p["bounciness"] != null) mat.bounciness = (float)p["bounciness"];

            AssetDatabase.CreateAsset(mat, path);
            AssetDatabase.SaveAssets();

            return new { success = true, path };
        }

        private static object GetGravity(JToken p)
        {
            var g = Physics2D.gravity;
            return new { x = g.x, y = g.y };
        }

        private static object SetGravity(JToken p)
        {
            var gravity = new Vector2((float)p["x"], (float)p["y"]);
            Physics2D.gravity = gravity;
            return new { success = true, gravity = new { x = gravity.x, y = gravity.y } };
        }

        private static object AddEffector(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            string type = (string)p["effectorType"] ?? "Area";
            Undo.RecordObject(go, "Add Effector2D");

            Effector2D effector = type switch
            {
                "Area" => (Effector2D)Undo.AddComponent<AreaEffector2D>(go),
                "Buoyancy" => Undo.AddComponent<BuoyancyEffector2D>(go),
                "Point" => Undo.AddComponent<PointEffector2D>(go),
                "Platform" => Undo.AddComponent<PlatformEffector2D>(go),
                "Surface" => Undo.AddComponent<SurfaceEffector2D>(go),
                _ => throw new McpException(-32000, $"Unknown effector type: {type}")
            };

            // Ensure collider is set as trigger for effectors
            var col = go.GetComponent<Collider2D>();
            if (col != null)
            {
                Undo.RecordObject(col, "Set Trigger for Effector");
                col.usedByEffector = true;
            }

            return new { success = true, gameObject = go.name, effectorType = type };
        }

        private static object SetEffector(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var effector = go.GetComponent<Effector2D>();
            if (effector == null) throw new McpException(-32000, $"No Effector2D on '{go.name}'");

            Undo.RecordObject(effector, "Set Effector2D");

            if (effector is AreaEffector2D area)
            {
                if (p["forceAngle"] != null) area.forceAngle = (float)p["forceAngle"];
                if (p["forceMagnitude"] != null) area.forceMagnitude = (float)p["forceMagnitude"];
                if (p["drag"] != null) area.linearDamping = (float)p["drag"];
                if (p["angularDrag"] != null) area.angularDamping = (float)p["angularDrag"];
            }
            else if (effector is BuoyancyEffector2D buoy)
            {
                if (p["density"] != null) buoy.density = (float)p["density"];
                if (p["surfaceLevel"] != null) buoy.surfaceLevel = (float)p["surfaceLevel"];
                if (p["angularDrag"] != null) buoy.angularDamping = (float)p["angularDrag"];
                if (p["flowAngle"] != null) buoy.flowAngle = (float)p["flowAngle"];
                if (p["flowMagnitude"] != null) buoy.flowMagnitude = (float)p["flowMagnitude"];
            }
            else if (effector is PointEffector2D point)
            {
                if (p["forceMagnitude"] != null) point.forceMagnitude = (float)p["forceMagnitude"];
                if (p["forceVariation"] != null) point.forceVariation = (float)p["forceVariation"];
                if (p["distanceScale"] != null) point.distanceScale = (float)p["distanceScale"];
                if (p["drag"] != null) point.linearDamping = (float)p["drag"];
                if (p["angularDrag"] != null) point.angularDamping = (float)p["angularDrag"];
            }
            else if (effector is PlatformEffector2D platform)
            {
                if (p["surfaceArc"] != null) platform.surfaceArc = (float)p["surfaceArc"];
                if (p["useOneWay"] != null) platform.useOneWay = (bool)p["useOneWay"];
                if (p["useSideBounce"] != null) platform.useSideBounce = (bool)p["useSideBounce"];
                if (p["useSideFriction"] != null) platform.useSideFriction = (bool)p["useSideFriction"];
            }
            else if (effector is SurfaceEffector2D surface)
            {
                if (p["speed"] != null) surface.speed = (float)p["speed"];
                if (p["speedVariation"] != null) surface.speedVariation = (float)p["speedVariation"];
                if (p["forceScale"] != null) surface.forceScale = (float)p["forceScale"];
            }

            EditorUtility.SetDirty(effector);
            return new { success = true, gameObject = go.name, effectorType = effector.GetType().Name };
        }
    }
}
