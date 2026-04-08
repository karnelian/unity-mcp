using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class ParticleHandler
    {
        public static void Register()
        {
            CommandRouter.Register("particle.create", Create);
            CommandRouter.Register("particle.getInfo", GetInfo);
            CommandRouter.Register("particle.setMain", SetMain);
            CommandRouter.Register("particle.setEmission", SetEmission);
            CommandRouter.Register("particle.setShape", SetShape);
            CommandRouter.Register("particle.setRenderer", SetRenderer);
            CommandRouter.Register("particle.setColorOverLifetime", SetColorOverLifetime);
            CommandRouter.Register("particle.setSizeOverLifetime", SetSizeOverLifetime);
            CommandRouter.Register("particle.setRotationOverLifetime", SetRotationOverLifetime);
            CommandRouter.Register("particle.setNoise", SetNoise);
            CommandRouter.Register("particle.setCollision", SetCollision);
            CommandRouter.Register("particle.setTrails", SetTrails);
            CommandRouter.Register("particle.setVelocityOverLifetime", SetVelocityOverLifetime);
            CommandRouter.Register("particle.play", Play);
            CommandRouter.Register("particle.find", Find);
            CommandRouter.Register("particle.setSubEmitters", SetSubEmitters);
            CommandRouter.Register("particle.setTextureSheetAnimation", SetTextureSheetAnimation);
        }

        private static Vector3 ParseVec3(JToken t, float defX = 0, float defY = 0, float defZ = 0)
        {
            if (t == null) return new Vector3(defX, defY, defZ);
            return new Vector3(
                t["x"]?.Value<float>() ?? defX,
                t["y"]?.Value<float>() ?? defY,
                t["z"]?.Value<float>() ?? defZ);
        }

        private static ParticleSystem FindPS(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null) throw new McpException(-32010, $"No ParticleSystem on '{go.name}'");
            return ps;
        }

        private static object Create(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "ParticleSystem";
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create ParticleSystem");

            var parentPath = p["parent"]?.Value<string>();
            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = GameObject.Find(parentPath);
                if (parent != null) Undo.SetTransformParent(go.transform, parent.transform, "Set Parent");
            }

            var pos = p["position"];
            if (pos != null) go.transform.position = ParseVec3(pos);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;

            if (p["startLifetime"] != null) main.startLifetime = p["startLifetime"].Value<float>();
            if (p["startSpeed"] != null) main.startSpeed = p["startSpeed"].Value<float>();
            if (p["startSize"] != null) main.startSize = p["startSize"].Value<float>();
            if (p["maxParticles"] != null) main.maxParticles = p["maxParticles"].Value<int>();
            if (p["duration"] != null) main.duration = p["duration"].Value<float>();
            if (p["loop"] != null) main.loop = p["loop"].Value<bool>();
            if (p["startColor"] != null)
            {
                var c = p["startColor"];
                main.startColor = new Color(
                    c["r"]?.Value<float>() ?? 1, c["g"]?.Value<float>() ?? 1,
                    c["b"]?.Value<float>() ?? 1, c["a"]?.Value<float>() ?? 1);
            }

            return new
            {
                success = true,
                name = go.name,
                instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go)
            };
        }

        private static object GetInfo(JToken p)
        {
            var ps = FindPS(p);
            var main = ps.main;
            var emission = ps.emission;
            var shape = ps.shape;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();

            return new
            {
                name = ps.gameObject.name,
                path = GameObjectFinder.GetPath(ps.gameObject),
                instanceId = ps.gameObject.GetInstanceID(),
                isPlaying = ps.isPlaying,
                isPaused = ps.isPaused,
                isStopped = ps.isStopped,
                particleCount = ps.particleCount,
                main = new
                {
                    duration = main.duration,
                    loop = main.loop,
                    startLifetime = main.startLifetime.constant,
                    startSpeed = main.startSpeed.constant,
                    startSize = main.startSize.constant,
                    startColor = ColorToObj(main.startColor.color),
                    gravityModifier = main.gravityModifier.constant,
                    simulationSpace = main.simulationSpace.ToString(),
                    maxParticles = main.maxParticles,
                    playOnAwake = main.playOnAwake,
                    scalingMode = main.scalingMode.ToString(),
                },
                emission = new
                {
                    enabled = emission.enabled,
                    rateOverTime = emission.rateOverTime.constant,
                    rateOverDistance = emission.rateOverDistance.constant,
                    burstCount = emission.burstCount,
                },
                shape = new
                {
                    enabled = shape.enabled,
                    shapeType = shape.shapeType.ToString(),
                    radius = shape.radius,
                    angle = shape.angle,
                    arc = shape.arc,
                },
                renderer = renderer != null ? new
                {
                    renderMode = renderer.renderMode.ToString(),
                    material = renderer.sharedMaterial?.name,
                    sortingOrder = renderer.sortingOrder,
                } : null,
                modules = new
                {
                    colorOverLifetime = ps.colorOverLifetime.enabled,
                    sizeOverLifetime = ps.sizeOverLifetime.enabled,
                    rotationOverLifetime = ps.rotationOverLifetime.enabled,
                    velocityOverLifetime = ps.velocityOverLifetime.enabled,
                    noise = ps.noise.enabled,
                    collision = ps.collision.enabled,
                    trails = ps.trails.enabled,
                    subEmitters = ps.subEmitters.enabled,
                    textureSheetAnimation = ps.textureSheetAnimation.enabled,
                }
            };
        }

        private static object SetMain(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem Main");
            var main = ps.main;

            if (p["duration"] != null) main.duration = p["duration"].Value<float>();
            if (p["loop"] != null) main.loop = p["loop"].Value<bool>();
            if (p["startLifetime"] != null) main.startLifetime = p["startLifetime"].Value<float>();
            if (p["startSpeed"] != null) main.startSpeed = p["startSpeed"].Value<float>();
            if (p["startSize"] != null) main.startSize = p["startSize"].Value<float>();
            if (p["gravityModifier"] != null) main.gravityModifier = p["gravityModifier"].Value<float>();
            if (p["maxParticles"] != null) main.maxParticles = p["maxParticles"].Value<int>();
            if (p["playOnAwake"] != null) main.playOnAwake = p["playOnAwake"].Value<bool>();
            if (p["startColor"] != null)
            {
                var c = p["startColor"];
                main.startColor = new Color(
                    c["r"]?.Value<float>() ?? 1, c["g"]?.Value<float>() ?? 1,
                    c["b"]?.Value<float>() ?? 1, c["a"]?.Value<float>() ?? 1);
            }
            if (p["simulationSpace"] != null)
            {
                if (Enum.TryParse<ParticleSystemSimulationSpace>(p["simulationSpace"].Value<string>(), true, out var ss))
                    main.simulationSpace = ss;
            }
            if (p["scalingMode"] != null)
            {
                if (Enum.TryParse<ParticleSystemScalingMode>(p["scalingMode"].Value<string>(), true, out var sm))
                    main.scalingMode = sm;
            }

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "Main module updated" };
        }

        private static object SetEmission(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem Emission");
            var emission = ps.emission;

            if (p["enabled"] != null) emission.enabled = p["enabled"].Value<bool>();
            if (p["rateOverTime"] != null) emission.rateOverTime = p["rateOverTime"].Value<float>();
            if (p["rateOverDistance"] != null) emission.rateOverDistance = p["rateOverDistance"].Value<float>();

            var bursts = p["bursts"] as JArray;
            if (bursts != null)
            {
                var burstList = new List<ParticleSystem.Burst>();
                foreach (var b in bursts)
                {
                    var time = b["time"]?.Value<float>() ?? 0f;
                    var count = b["count"]?.Value<int>() ?? 10;
                    var cycles = b["cycles"]?.Value<int>() ?? 1;
                    var interval = b["interval"]?.Value<float>() ?? 0.01f;
                    burstList.Add(new ParticleSystem.Burst(time, (short)count, (short)count, cycles, interval));
                }
                emission.SetBursts(burstList.ToArray());
            }

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "Emission module updated" };
        }

        private static object SetShape(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem Shape");
            var shape = ps.shape;

            if (p["enabled"] != null) shape.enabled = p["enabled"].Value<bool>();
            if (p["shapeType"] != null)
            {
                if (Enum.TryParse<ParticleSystemShapeType>(p["shapeType"].Value<string>(), true, out var st))
                    shape.shapeType = st;
            }
            if (p["radius"] != null) shape.radius = p["radius"].Value<float>();
            if (p["angle"] != null) shape.angle = p["angle"].Value<float>();
            if (p["arc"] != null) shape.arc = p["arc"].Value<float>();
            if (p["position"] != null) shape.position = ParseVec3(p["position"]);
            if (p["rotation"] != null) shape.rotation = ParseVec3(p["rotation"]);
            if (p["scale"] != null) shape.scale = ParseVec3(p["scale"], 1, 1, 1);
            if (p["radiusThickness"] != null) shape.radiusThickness = p["radiusThickness"].Value<float>();
            if (p["length"] != null) shape.length = p["length"].Value<float>();

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "Shape module updated" };
        }

        private static object SetRenderer(JToken p)
        {
            var ps = FindPS(p);
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer == null) throw new McpException(-32010, "No ParticleSystemRenderer found");
            Undo.RecordObject(renderer, "Set ParticleSystem Renderer");

            if (p["renderMode"] != null)
            {
                if (Enum.TryParse<ParticleSystemRenderMode>(p["renderMode"].Value<string>(), true, out var rm))
                    renderer.renderMode = rm;
            }
            if (p["materialPath"] != null)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(p["materialPath"].Value<string>());
                if (mat != null) renderer.sharedMaterial = mat;
            }
            if (p["sortingOrder"] != null) renderer.sortingOrder = p["sortingOrder"].Value<int>();
            if (p["sortingLayerName"] != null) renderer.sortingLayerName = p["sortingLayerName"].Value<string>();
            if (p["minParticleSize"] != null) renderer.minParticleSize = p["minParticleSize"].Value<float>();
            if (p["maxParticleSize"] != null) renderer.maxParticleSize = p["maxParticleSize"].Value<float>();

            EditorUtility.SetDirty(renderer);
            return new { success = true, message = "Renderer updated" };
        }

        private static object SetColorOverLifetime(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem ColorOverLifetime");
            var col = ps.colorOverLifetime;

            if (p["enabled"] != null) col.enabled = p["enabled"].Value<bool>();

            var keys = p["colorKeys"] as JArray;
            var alphaKeys = p["alphaKeys"] as JArray;
            if (keys != null || alphaKeys != null)
            {
                var gKeys = new List<GradientColorKey>();
                var aKeys = new List<GradientAlphaKey>();

                if (keys != null)
                {
                    foreach (var k in keys)
                    {
                        var c = new Color(k["r"]?.Value<float>() ?? 1, k["g"]?.Value<float>() ?? 1,
                            k["b"]?.Value<float>() ?? 1);
                        gKeys.Add(new GradientColorKey(c, k["time"]?.Value<float>() ?? 0));
                    }
                }
                else
                {
                    gKeys.Add(new GradientColorKey(Color.white, 0));
                    gKeys.Add(new GradientColorKey(Color.white, 1));
                }

                if (alphaKeys != null)
                {
                    foreach (var k in alphaKeys)
                        aKeys.Add(new GradientAlphaKey(k["alpha"]?.Value<float>() ?? 1, k["time"]?.Value<float>() ?? 0));
                }
                else
                {
                    aKeys.Add(new GradientAlphaKey(1, 0));
                    aKeys.Add(new GradientAlphaKey(1, 1));
                }

                var gradient = new Gradient();
                gradient.SetKeys(gKeys.ToArray(), aKeys.ToArray());
                col.color = new ParticleSystem.MinMaxGradient(gradient);
            }

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "ColorOverLifetime module updated" };
        }

        private static object SetSizeOverLifetime(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem SizeOverLifetime");
            var sol = ps.sizeOverLifetime;

            if (p["enabled"] != null) sol.enabled = p["enabled"].Value<bool>();
            if (p["sizeMultiplier"] != null) sol.sizeMultiplier = p["sizeMultiplier"].Value<float>();

            var curve = p["curve"] as JArray;
            if (curve != null)
            {
                var keys = curve.Select(k => new Keyframe(
                    k["time"]?.Value<float>() ?? 0,
                    k["value"]?.Value<float>() ?? 1)).ToArray();
                sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(keys));
            }

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "SizeOverLifetime module updated" };
        }

        private static object SetRotationOverLifetime(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem RotationOverLifetime");
            var rot = ps.rotationOverLifetime;

            if (p["enabled"] != null) rot.enabled = p["enabled"].Value<bool>();
            if (p["angularVelocity"] != null) rot.z = p["angularVelocity"].Value<float>() * Mathf.Deg2Rad;

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "RotationOverLifetime module updated" };
        }

        private static object SetNoise(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem Noise");
            var noise = ps.noise;

            if (p["enabled"] != null) noise.enabled = p["enabled"].Value<bool>();
            if (p["strength"] != null) noise.strength = p["strength"].Value<float>();
            if (p["frequency"] != null) noise.frequency = p["frequency"].Value<float>();
            if (p["scrollSpeed"] != null) noise.scrollSpeed = p["scrollSpeed"].Value<float>();
            if (p["octaveCount"] != null) noise.octaveCount = p["octaveCount"].Value<int>();
            if (p["damping"] != null) noise.damping = p["damping"].Value<bool>();

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "Noise module updated" };
        }

        private static object SetCollision(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem Collision");
            var collision = ps.collision;

            if (p["enabled"] != null) collision.enabled = p["enabled"].Value<bool>();
            if (p["type"] != null)
            {
                if (Enum.TryParse<ParticleSystemCollisionType>(p["type"].Value<string>(), true, out var ct))
                    collision.type = ct;
            }
            if (p["mode"] != null)
            {
                if (Enum.TryParse<ParticleSystemCollisionMode>(p["mode"].Value<string>(), true, out var cm))
                    collision.mode = cm;
            }
            if (p["bounce"] != null) collision.bounce = p["bounce"].Value<float>();
            if (p["dampen"] != null) collision.dampen = p["dampen"].Value<float>();
            if (p["lifetimeLoss"] != null) collision.lifetimeLoss = p["lifetimeLoss"].Value<float>();
            if (p["enableDynamicColliders"] != null) collision.enableDynamicColliders = p["enableDynamicColliders"].Value<bool>();

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "Collision module updated" };
        }

        private static object SetTrails(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem Trails");
            var trails = ps.trails;

            if (p["enabled"] != null) trails.enabled = p["enabled"].Value<bool>();
            if (p["ratio"] != null) trails.ratio = p["ratio"].Value<float>();
            if (p["lifetime"] != null) trails.lifetime = p["lifetime"].Value<float>();
            if (p["minVertexDistance"] != null) trails.minVertexDistance = p["minVertexDistance"].Value<float>();
            if (p["worldSpace"] != null) trails.worldSpace = p["worldSpace"].Value<bool>();
            if (p["dieWithParticles"] != null) trails.dieWithParticles = p["dieWithParticles"].Value<bool>();
            if (p["widthOverTrail"] != null) trails.widthOverTrail = p["widthOverTrail"].Value<float>();
            if (p["textureMode"] != null)
            {
                if (Enum.TryParse<ParticleSystemTrailTextureMode>(p["textureMode"].Value<string>(), true, out var tm))
                    trails.textureMode = tm;
            }

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "Trails module updated" };
        }

        private static object SetVelocityOverLifetime(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem VelocityOverLifetime");
            var vol = ps.velocityOverLifetime;

            if (p["enabled"] != null) vol.enabled = p["enabled"].Value<bool>();
            if (p["x"] != null) vol.x = p["x"].Value<float>();
            if (p["y"] != null) vol.y = p["y"].Value<float>();
            if (p["z"] != null) vol.z = p["z"].Value<float>();
            if (p["space"] != null)
            {
                if (Enum.TryParse<ParticleSystemSimulationSpace>(p["space"].Value<string>(), true, out var ss))
                    vol.space = ss;
            }
            if (p["speedModifier"] != null) vol.speedModifier = p["speedModifier"].Value<float>();

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "VelocityOverLifetime module updated" };
        }

        private static object Play(JToken p)
        {
            var ps = FindPS(p);
            var action = Validate.Required<string>(p, "action").ToLower();
            switch (action)
            {
                case "play": ps.Play(); break;
                case "stop": ps.Stop(); break;
                case "pause": ps.Pause(); break;
                case "clear": ps.Clear(); break;
                case "restart": ps.Stop(); ps.Clear(); ps.Play(); break;
                default: throw new McpException(-32010, $"Unknown action: {action}. Use play/stop/pause/clear/restart");
            }
            return new
            {
                success = true,
                action,
                isPlaying = ps.isPlaying,
                particleCount = ps.particleCount,
            };
        }

        private static object Find(JToken p)
        {
            var filter = p?["nameFilter"]?.Value<string>();
            var particles = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);

            if (!string.IsNullOrEmpty(filter))
                particles = particles.Where(ps => ps.gameObject.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            return new
            {
                count = particles.Length,
                particles = particles.Select(ps => new
                {
                    name = ps.gameObject.name,
                    path = GameObjectFinder.GetPath(ps.gameObject),
                    instanceId = ps.gameObject.GetInstanceID(),
                    isPlaying = ps.isPlaying,
                    particleCount = ps.particleCount,
                    main = new
                    {
                        duration = ps.main.duration,
                        loop = ps.main.loop,
                        maxParticles = ps.main.maxParticles,
                    }
                }).ToArray()
            };
        }

        private static object SetSubEmitters(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem SubEmitters");
            var sub = ps.subEmitters;

            if (p["enabled"] != null) sub.enabled = p["enabled"].Value<bool>();

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "SubEmitters module updated" };
        }

        private static object SetTextureSheetAnimation(JToken p)
        {
            var ps = FindPS(p);
            Undo.RecordObject(ps, "Set ParticleSystem TextureSheetAnimation");
            var tsa = ps.textureSheetAnimation;

            if (p["enabled"] != null) tsa.enabled = p["enabled"].Value<bool>();
            if (p["numTilesX"] != null) tsa.numTilesX = p["numTilesX"].Value<int>();
            if (p["numTilesY"] != null) tsa.numTilesY = p["numTilesY"].Value<int>();
            if (p["cycleCount"] != null) tsa.cycleCount = p["cycleCount"].Value<int>();

            EditorUtility.SetDirty(ps);
            return new { success = true, message = "TextureSheetAnimation module updated" };
        }

        private static object ColorToObj(Color c) => new { r = c.r, g = c.g, b = c.b, a = c.a };
    }
}
