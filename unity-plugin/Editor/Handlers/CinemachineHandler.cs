using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

#if CINEMACHINE_V3
using Unity.Cinemachine;
using Unity.Mathematics;
#elif UNITY_CINEMACHINE || PACKAGE_CINEMACHINE
using Cinemachine;
#endif

namespace KarnelLabs.MCP
{
    public static class CinemachineHandler
    {
        public static void Register()
        {
#if UNITY_CINEMACHINE || PACKAGE_CINEMACHINE
            CommandRouter.Register("cinemachine.createVirtualCamera", CreateVirtualCamera);
            CommandRouter.Register("cinemachine.createFreeLook", CreateFreeLook);
            CommandRouter.Register("cinemachine.setBrain", SetBrain);
            CommandRouter.Register("cinemachine.getBrain", GetBrain);
            CommandRouter.Register("cinemachine.setFollow", SetFollow);
            CommandRouter.Register("cinemachine.setLookAt", SetLookAt);
            CommandRouter.Register("cinemachine.setBody", SetBody);
            CommandRouter.Register("cinemachine.setAim", SetAim);
            CommandRouter.Register("cinemachine.setNoise", SetNoise);
            CommandRouter.Register("cinemachine.setPriority", SetPriority);
            CommandRouter.Register("cinemachine.getInfo", GetInfo);
            CommandRouter.Register("cinemachine.findCameras", FindCameras);
            CommandRouter.Register("cinemachine.createBlendList", CreateBlendList);
            CommandRouter.Register("cinemachine.setLens", SetLens);
            CommandRouter.Register("cinemachine.createDollyTrack", CreateDollyTrack);
            CommandRouter.Register("cinemachine.addDollyPoint", AddDollyPoint);
            CommandRouter.Register("cinemachine.setConfiner", SetConfiner);
            CommandRouter.Register("cinemachine.createMixingCamera", CreateMixingCamera);
            CommandRouter.Register("cinemachine.createClearShot", CreateClearShot);
            CommandRouter.Register("cinemachine.setDeadZone", SetDeadZone);
            CommandRouter.Register("cinemachine.createGroup", CreateGroup);
            CommandRouter.Register("cinemachine.addGroupTarget", AddGroupTarget);
            CommandRouter.Register("cinemachine.setGroupFraming", SetGroupFraming);
#endif
        }

#if CINEMACHINE_V3

        private static object CreateVirtualCamera(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "VirtualCamera";
            var go = new GameObject(name);
            var vcam = go.AddComponent<CinemachineCamera>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create VirtualCamera");

            if (p["priority"] != null) vcam.Priority = p["priority"].Value<int>();
            if (p["fov"] != null)
            {
                var lens = vcam.Lens;
                lens.FieldOfView = p["fov"].Value<float>();
                vcam.Lens = lens;
            }

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0
                );
            }

            if (p["follow"] != null)
            {
                var target = GameObjectFinder.FindOrThrow(name: p["follow"].Value<string>());
                vcam.Follow = target.transform;
            }
            if (p["lookAt"] != null)
            {
                var target = GameObjectFinder.FindOrThrow(name: p["lookAt"].Value<string>());
                vcam.LookAt = target.transform;
            }

            return new { name = go.name, path = GameObjectFinder.GetPath(go), priority = vcam.Priority, fov = vcam.Lens.FieldOfView };
        }

        private static object CreateFreeLook(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "FreeLookCamera";
            var go = new GameObject(name);
            var vcam = go.AddComponent<CinemachineCamera>();
            go.AddComponent<CinemachineOrbitalFollow>();
            go.AddComponent<CinemachineRotationComposer>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create FreeLook");

            if (p["follow"] != null)
            {
                var target = GameObjectFinder.FindOrThrow(name: p["follow"].Value<string>());
                vcam.Follow = target.transform;
            }
            if (p["lookAt"] != null)
            {
                var target = GameObjectFinder.FindOrThrow(name: p["lookAt"].Value<string>());
                vcam.LookAt = target.transform;
            }

            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object SetBrain(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var brain = go.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = Undo.AddComponent<CinemachineBrain>(go);
            }
            WorkflowManager.SnapshotObject(go);

            if (p["defaultBlend"] != null)
            {
                var blend = brain.DefaultBlend;
                blend.Time = p["defaultBlend"].Value<float>();
                brain.DefaultBlend = blend;
            }
            if (p["updateMethod"] != null)
                brain.UpdateMethod = Validate.ParseEnum<CinemachineBrain.UpdateMethods>(p["updateMethod"].Value<string>(), "updateMethod");

            return new { name = go.name, defaultBlendTime = brain.DefaultBlend.Time, updateMethod = brain.UpdateMethod.ToString() };
        }

        private static object GetBrain(JToken p)
        {
            var brain = UnityEngine.Object.FindFirstObjectByType<CinemachineBrain>();
            if (brain == null) throw new McpException(-32003, "CinemachineBrain not found in scene");

            var activeCam = brain.ActiveVirtualCamera;
            return new
            {
                gameObject = brain.gameObject.name,
                path = GameObjectFinder.GetPath(brain.gameObject),
                defaultBlendTime = brain.DefaultBlend.Time,
                updateMethod = brain.UpdateMethod.ToString(),
                activeCamera = activeCam?.Name,
                isBlending = brain.IsBlending,
            };
        }

        private static object SetFollow(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);

            var targetName = Validate.Required<string>(p, "target");
            var target = GameObjectFinder.FindOrThrow(name: targetName);
            WorkflowManager.SnapshotObject(go);
            vcam.Follow = target.transform;

            return new { camera = go.name, follow = target.name };
        }

        private static object SetLookAt(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);

            var targetName = Validate.Required<string>(p, "target");
            var target = GameObjectFinder.FindOrThrow(name: targetName);
            WorkflowManager.SnapshotObject(go);
            vcam.LookAt = target.transform;

            return new { camera = go.name, lookAt = target.name };
        }

        private static object SetBody(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var bodyType = Validate.Required<string>(p, "bodyType");

            switch (bodyType.ToLower())
            {
                case "transposer":
                case "follow":
                    RemoveBodyComponents(go);
                    var follow = go.GetComponent<CinemachineFollow>() ?? Undo.AddComponent<CinemachineFollow>(go);
                    if (p["followOffset"] != null)
                    {
                        follow.FollowOffset = new Vector3(
                            p["followOffset"]["x"]?.Value<float>() ?? 0,
                            p["followOffset"]["y"]?.Value<float>() ?? 5,
                            p["followOffset"]["z"]?.Value<float>() ?? -10
                        );
                    }
                    if (p["damping"] != null)
                    {
                        var d = p["damping"].Value<float>();
                        follow.TrackerSettings.PositionDamping = new Vector3(d, d, d);
                    }
                    break;
                case "framingtransposer":
                case "positioncomposer":
                    RemoveBodyComponents(go);
                    var posComposer = go.GetComponent<CinemachinePositionComposer>() ?? Undo.AddComponent<CinemachinePositionComposer>(go);
                    if (p["cameraDistance"] != null) posComposer.CameraDistance = p["cameraDistance"].Value<float>();
                    if (p["screenX"] != null || p["screenY"] != null)
                    {
                        var comp = posComposer.Composition;
                        comp.ScreenPosition = new Vector2(
                            p["screenX"]?.Value<float>() ?? comp.ScreenPosition.x,
                            p["screenY"]?.Value<float>() ?? comp.ScreenPosition.y
                        );
                        posComposer.Composition = comp;
                    }
                    break;
                case "hardlocktotarget":
                    RemoveBodyComponents(go);
                    if (go.GetComponent<CinemachineHardLockToTarget>() == null)
                        Undo.AddComponent<CinemachineHardLockToTarget>(go);
                    break;
            }

            return new { camera = go.name, bodyType };
        }

        private static void RemoveBodyComponents(GameObject go)
        {
            var follow = go.GetComponent<CinemachineFollow>();
            if (follow != null) Undo.DestroyObjectImmediate(follow);
            var posComposer = go.GetComponent<CinemachinePositionComposer>();
            if (posComposer != null) Undo.DestroyObjectImmediate(posComposer);
            var hardLock = go.GetComponent<CinemachineHardLockToTarget>();
            if (hardLock != null) Undo.DestroyObjectImmediate(hardLock);
            var orbital = go.GetComponent<CinemachineOrbitalFollow>();
            if (orbital != null) Undo.DestroyObjectImmediate(orbital);
        }

        private static void RemoveAimComponents(GameObject go)
        {
            var rotComposer = go.GetComponent<CinemachineRotationComposer>();
            if (rotComposer != null) Undo.DestroyObjectImmediate(rotComposer);
            var hardLook = go.GetComponent<CinemachineHardLookAt>();
            if (hardLook != null) Undo.DestroyObjectImmediate(hardLook);
            var panTilt = go.GetComponent<CinemachinePanTilt>();
            if (panTilt != null) Undo.DestroyObjectImmediate(panTilt);
        }

        private static object SetAim(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var aimType = Validate.Required<string>(p, "aimType");

            switch (aimType.ToLower())
            {
                case "composer":
                case "rotationcomposer":
                    RemoveAimComponents(go);
                    var rotComposer = go.GetComponent<CinemachineRotationComposer>() ?? Undo.AddComponent<CinemachineRotationComposer>(go);
                    if (p["trackedObjectOffset"] != null)
                    {
                        rotComposer.TargetOffset = new Vector3(
                            p["trackedObjectOffset"]["x"]?.Value<float>() ?? 0,
                            p["trackedObjectOffset"]["y"]?.Value<float>() ?? 0,
                            p["trackedObjectOffset"]["z"]?.Value<float>() ?? 0
                        );
                    }
                    if (p["lookaheadTime"] != null)
                    {
                        var lookahead = rotComposer.Lookahead;
                        lookahead.Time = p["lookaheadTime"].Value<float>();
                        rotComposer.Lookahead = lookahead;
                    }
                    break;
                case "hardlookat":
                    RemoveAimComponents(go);
                    if (go.GetComponent<CinemachineHardLookAt>() == null)
                        Undo.AddComponent<CinemachineHardLookAt>(go);
                    break;
                case "pov":
                case "pantilt":
                    RemoveAimComponents(go);
                    if (go.GetComponent<CinemachinePanTilt>() == null)
                        Undo.AddComponent<CinemachinePanTilt>(go);
                    break;
            }

            return new { camera = go.name, aimType };
        }

        private static object SetNoise(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var noise = go.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise == null)
                noise = Undo.AddComponent<CinemachineBasicMultiChannelPerlin>(go);

            if (p["amplitudeGain"] != null) noise.AmplitudeGain = p["amplitudeGain"].Value<float>();
            if (p["frequencyGain"] != null) noise.FrequencyGain = p["frequencyGain"].Value<float>();

            if (p["profileName"] != null)
            {
                var profileName = p["profileName"].Value<string>();
                var guids = AssetDatabase.FindAssets("t:NoiseSettings " + profileName);
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    noise.NoiseProfile = AssetDatabase.LoadAssetAtPath<NoiseSettings>(path);
                }
            }

            return new { camera = go.name, amplitudeGain = noise.AmplitudeGain, frequencyGain = noise.FrequencyGain };
        }

        private static object SetPriority(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);

            var priority = Validate.Required<int>(p, "priority");
            WorkflowManager.SnapshotObject(go);
            vcam.Priority = priority;

            return new { camera = go.name, priority };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);

            var lens = vcam.Lens;
            var followTarget = vcam.Follow;
            var lookAtTarget = vcam.LookAt;

            return new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
                priority = vcam.Priority.Value,
                follow = followTarget != null ? followTarget.name : (string)null,
                lookAt = lookAtTarget != null ? lookAtTarget.name : (string)null,
                fov = lens.FieldOfView,
                nearClip = lens.NearClipPlane,
                farClip = lens.FarClipPlane,
                body = GetBodyComponentName(go),
                aim = GetAimComponentName(go),
                noise = go.GetComponent<CinemachineBasicMultiChannelPerlin>() != null ? "CinemachineBasicMultiChannelPerlin" : null,
            };
        }

        private static string GetBodyComponentName(GameObject go)
        {
            if (go.GetComponent<CinemachineFollow>() != null) return "CinemachineFollow";
            if (go.GetComponent<CinemachinePositionComposer>() != null) return "CinemachinePositionComposer";
            if (go.GetComponent<CinemachineHardLockToTarget>() != null) return "CinemachineHardLockToTarget";
            if (go.GetComponent<CinemachineOrbitalFollow>() != null) return "CinemachineOrbitalFollow";
            return null;
        }

        private static string GetAimComponentName(GameObject go)
        {
            if (go.GetComponent<CinemachineRotationComposer>() != null) return "CinemachineRotationComposer";
            if (go.GetComponent<CinemachineHardLookAt>() != null) return "CinemachineHardLookAt";
            if (go.GetComponent<CinemachinePanTilt>() != null) return "CinemachinePanTilt";
            return null;
        }

        private static object FindCameras(JToken p)
        {
            var vcams = UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

            var results = new List<object>();
            foreach (var v in vcams)
            {
                var hasOrbital = v.GetComponent<CinemachineOrbitalFollow>() != null;
                results.Add(new
                {
                    name = v.gameObject.name,
                    path = GameObjectFinder.GetPath(v.gameObject),
                    type = hasOrbital ? "FreeLook" : "VirtualCamera",
                    priority = v.Priority.Value
                });
            }

            return new { count = results.Count, cameras = results };
        }

        private static object CreateBlendList(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "BlendListCamera";
            var go = new GameObject(name);
            go.AddComponent<CinemachineSequencerCamera>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create BlendList");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object SetLens(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var lens = vcam.Lens;
            if (p["fieldOfView"] != null) lens.FieldOfView = p["fieldOfView"].Value<float>();
            if (p["nearClipPlane"] != null) lens.NearClipPlane = p["nearClipPlane"].Value<float>();
            if (p["farClipPlane"] != null) lens.FarClipPlane = p["farClipPlane"].Value<float>();
            if (p["orthographicSize"] != null) lens.OrthographicSize = p["orthographicSize"].Value<float>();
            if (p["dutch"] != null) lens.Dutch = p["dutch"].Value<float>();
            vcam.Lens = lens;

            return new { camera = go.name, fov = lens.FieldOfView, near = lens.NearClipPlane, far = lens.FarClipPlane };
        }

        private static object CreateDollyTrack(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "DollyTrack";
            var go = new GameObject(name);
            var spline = go.AddComponent<UnityEngine.Splines.SplineContainer>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create DollyTrack");

            var points = p["points"]?.ToObject<float[][]>();
            if (points != null && points.Length > 0)
            {
                var s = spline.Spline;
                s.Clear();
                for (int i = 0; i < points.Length; i++)
                {
                    var pos = new Vector3(
                        points[i].Length > 0 ? points[i][0] : 0,
                        points[i].Length > 1 ? points[i][1] : 0,
                        points[i].Length > 2 ? points[i][2] : 0
                    );
                    s.Add(new UnityEngine.Splines.BezierKnot(pos));
                }
            }

            return new { name = go.name, path = GameObjectFinder.GetPath(go), pointCount = spline.Spline.Count };
        }

        private static object AddDollyPoint(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var spline = go.GetComponent<UnityEngine.Splines.SplineContainer>();
            if (spline == null) throw new McpException(-32003, "SplineContainer not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var position = new Vector3(
                p["position"]?["x"]?.Value<float>() ?? 0,
                p["position"]?["y"]?.Value<float>() ?? 0,
                p["position"]?["z"]?.Value<float>() ?? 0
            );

            spline.Spline.Add(new UnityEngine.Splines.BezierKnot(position));

            return new { track = go.name, pointCount = spline.Spline.Count, addedPosition = new { x = position.x, y = position.y, z = position.z } };
        }

        private static object SetConfiner(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var boundsName = p["boundsObject"]?.Value<string>();
            if (boundsName != null)
            {
                var boundsGo = GameObjectFinder.FindOrThrow(name: boundsName);
                var collider = boundsGo.GetComponent<Collider>();
                if (collider != null)
                {
                    var confiner3d = go.GetComponent<CinemachineConfiner3D>();
                    if (confiner3d == null) confiner3d = Undo.AddComponent<CinemachineConfiner3D>(go);
                    confiner3d.BoundingVolume = collider;
                }
                var collider2d = boundsGo.GetComponent<Collider2D>();
                if (collider2d != null)
                {
                    var confiner2d = go.GetComponent<CinemachineConfiner2D>();
                    if (confiner2d == null) confiner2d = Undo.AddComponent<CinemachineConfiner2D>(go);
                    confiner2d.BoundingShape2D = collider2d;
                }
            }

            return new { camera = go.name, confinerAdded = true };
        }

        private static object CreateMixingCamera(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "MixingCamera";
            var go = new GameObject(name);
            go.AddComponent<CinemachineMixingCamera>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create MixingCamera");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object CreateClearShot(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "ClearShotCamera";
            var go = new GameObject(name);
            go.AddComponent<CinemachineClearShot>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create ClearShot");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object SetDeadZone(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var rotComposer = go.GetComponent<CinemachineRotationComposer>();
            if (rotComposer == null) throw new McpException(-32003, "CinemachineRotationComposer not found -- set aim to Composer first");

            var comp = rotComposer.Composition;
            var deadZone = comp.DeadZone;
            if (p["width"] != null) deadZone.Size = new Vector2(p["width"].Value<float>(), deadZone.Size.y);
            if (p["height"] != null) deadZone.Size = new Vector2(deadZone.Size.x, p["height"].Value<float>());
            deadZone.Enabled = true;
            comp.DeadZone = deadZone;
            var hardLimits = comp.HardLimits;
            if (p["softZoneWidth"] != null) hardLimits.Size = new Vector2(p["softZoneWidth"].Value<float>(), hardLimits.Size.y);
            if (p["softZoneHeight"] != null) hardLimits.Size = new Vector2(hardLimits.Size.x, p["softZoneHeight"].Value<float>());
            if (p["softZoneWidth"] != null || p["softZoneHeight"] != null) hardLimits.Enabled = true;
            comp.HardLimits = hardLimits;
            rotComposer.Composition = comp;

            return new { camera = go.name, deadZoneWidth = comp.DeadZone.Size.x, deadZoneHeight = comp.DeadZone.Size.y };
        }

        private static object CreateGroup(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "TargetGroup";
            var go = new GameObject(name);
            var group = go.AddComponent<CinemachineTargetGroup>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create TargetGroup");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object AddGroupTarget(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var group = go.GetComponent<CinemachineTargetGroup>();
            if (group == null) throw new McpException(-32003, "CinemachineTargetGroup not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var targetName = Validate.Required<string>(p, "target");
            var target = GameObjectFinder.FindOrThrow(name: targetName);
            var weight = p["weight"]?.Value<float>() ?? 1f;
            var radius = p["radius"]?.Value<float>() ?? 1f;

            group.Targets.Add(new CinemachineTargetGroup.Target
            {
                Object = target.transform,
                Weight = weight,
                Radius = radius
            });

            return new { group = go.name, targetAdded = target.name, totalTargets = group.Targets.Count };
        }

        private static object SetGroupFraming(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var groupFraming = go.GetComponent<CinemachineGroupFraming>();
            if (groupFraming == null) throw new McpException(-32003, "CinemachineGroupFraming not found -- add GroupFraming component first");

            if (p["groupFramingSize"] != null) groupFraming.FramingSize = p["groupFramingSize"].Value<float>();
            if (p["damping"] != null)
            {
                groupFraming.Damping = p["damping"].Value<float>();
            }

            return new { camera = go.name, groupFramingSize = groupFraming.FramingSize };
        }

#elif UNITY_CINEMACHINE || PACKAGE_CINEMACHINE

        private static object CreateVirtualCamera(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "VirtualCamera";
            var go = new GameObject(name);
            var vcam = go.AddComponent<CinemachineVirtualCamera>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create VirtualCamera");

            if (p["priority"] != null) vcam.Priority = p["priority"].Value<int>();
            if (p["fov"] != null) vcam.m_Lens.FieldOfView = p["fov"].Value<float>();

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0
                );
            }

            if (p["follow"] != null)
            {
                var target = GameObjectFinder.FindOrThrow(name: p["follow"].Value<string>());
                vcam.Follow = target.transform;
            }
            if (p["lookAt"] != null)
            {
                var target = GameObjectFinder.FindOrThrow(name: p["lookAt"].Value<string>());
                vcam.LookAt = target.transform;
            }

            return new { name = go.name, path = GameObjectFinder.GetPath(go), priority = vcam.Priority, fov = vcam.m_Lens.FieldOfView };
        }

        private static object CreateFreeLook(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "FreeLookCamera";
            var go = new GameObject(name);
            var freeLook = go.AddComponent<CinemachineFreeLook>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create FreeLook");

            if (p["follow"] != null)
            {
                var target = GameObjectFinder.FindOrThrow(name: p["follow"].Value<string>());
                freeLook.Follow = target.transform;
            }
            if (p["lookAt"] != null)
            {
                var target = GameObjectFinder.FindOrThrow(name: p["lookAt"].Value<string>());
                freeLook.LookAt = target.transform;
            }

            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object SetBrain(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var brain = go.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = Undo.AddComponent<CinemachineBrain>(go);
            }
            WorkflowManager.SnapshotObject(go);

            if (p["defaultBlend"] != null)
            {
                brain.m_DefaultBlend.m_Time = p["defaultBlend"].Value<float>();
            }
            if (p["updateMethod"] != null)
                brain.m_UpdateMethod = Validate.ParseEnum<CinemachineBrain.UpdateMethod>(p["updateMethod"].Value<string>(), "updateMethod");

            return new { name = go.name, defaultBlendTime = brain.m_DefaultBlend.m_Time, updateMethod = brain.m_UpdateMethod.ToString() };
        }

        private static object GetBrain(JToken p)
        {
            var brain = UnityEngine.Object.FindObjectOfType<CinemachineBrain>();
            if (brain == null) throw new McpException(-32003, "CinemachineBrain not found in scene");

            var activeCam = brain.ActiveVirtualCamera;
            return new
            {
                gameObject = brain.gameObject.name,
                path = GameObjectFinder.GetPath(brain.gameObject),
                defaultBlendTime = brain.m_DefaultBlend.m_Time,
                updateMethod = brain.m_UpdateMethod.ToString(),
                activeCamera = activeCam?.Name,
                isBlending = brain.IsBlending,
            };
        }

        private static object SetFollow(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);

            var targetName = Validate.Required<string>(p, "target");
            var target = GameObjectFinder.FindOrThrow(name: targetName);
            WorkflowManager.SnapshotObject(go);
            vcam.Follow = target.transform;

            return new { camera = go.name, follow = target.name };
        }

        private static object SetLookAt(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);

            var targetName = Validate.Required<string>(p, "target");
            var target = GameObjectFinder.FindOrThrow(name: targetName);
            WorkflowManager.SnapshotObject(go);
            vcam.LookAt = target.transform;

            return new { camera = go.name, lookAt = target.name };
        }

        private static object SetBody(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var bodyType = Validate.Required<string>(p, "bodyType");
            var body = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body);

            switch (bodyType.ToLower())
            {
                case "transposer":
                    if (body == null || !(body is CinemachineTransposer))
                    {
                        if (body != null) vcam.DestroyCinemachineComponent(CinemachineCore.Stage.Body);
                        body = vcam.AddCinemachineComponent<CinemachineTransposer>();
                    }
                    var transposer = body as CinemachineTransposer;
                    if (p["followOffset"] != null)
                    {
                        transposer.m_FollowOffset = new Vector3(
                            p["followOffset"]["x"]?.Value<float>() ?? 0,
                            p["followOffset"]["y"]?.Value<float>() ?? 5,
                            p["followOffset"]["z"]?.Value<float>() ?? -10
                        );
                    }
                    if (p["damping"] != null)
                    {
                        transposer.m_XDamping = p["damping"].Value<float>();
                        transposer.m_YDamping = p["damping"].Value<float>();
                        transposer.m_ZDamping = p["damping"].Value<float>();
                    }
                    break;
                case "framingTransposer":
                case "framingtransposer":
                    if (body == null || !(body is CinemachineFramingTransposer))
                    {
                        if (body != null) vcam.DestroyCinemachineComponent(CinemachineCore.Stage.Body);
                        body = vcam.AddCinemachineComponent<CinemachineFramingTransposer>();
                    }
                    var framing = body as CinemachineFramingTransposer;
                    if (p["cameraDistance"] != null) framing.m_CameraDistance = p["cameraDistance"].Value<float>();
                    if (p["screenX"] != null) framing.m_ScreenX = p["screenX"].Value<float>();
                    if (p["screenY"] != null) framing.m_ScreenY = p["screenY"].Value<float>();
                    break;
                case "hardlocktoTarget":
                case "hardlocktotarget":
                    if (body != null) vcam.DestroyCinemachineComponent(CinemachineCore.Stage.Body);
                    vcam.AddCinemachineComponent<CinemachineHardLockToTarget>();
                    break;
            }

            return new { camera = go.name, bodyType };
        }

        private static object SetAim(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var aimType = Validate.Required<string>(p, "aimType");
            var aim = vcam.GetCinemachineComponent(CinemachineCore.Stage.Aim);

            switch (aimType.ToLower())
            {
                case "composer":
                    if (aim == null || !(aim is CinemachineComposer))
                    {
                        if (aim != null) vcam.DestroyCinemachineComponent(CinemachineCore.Stage.Aim);
                        aim = vcam.AddCinemachineComponent<CinemachineComposer>();
                    }
                    var composer = aim as CinemachineComposer;
                    if (p["trackedObjectOffset"] != null)
                    {
                        composer.m_TrackedObjectOffset = new Vector3(
                            p["trackedObjectOffset"]["x"]?.Value<float>() ?? 0,
                            p["trackedObjectOffset"]["y"]?.Value<float>() ?? 0,
                            p["trackedObjectOffset"]["z"]?.Value<float>() ?? 0
                        );
                    }
                    if (p["lookaheadTime"] != null) composer.m_LookaheadTime = p["lookaheadTime"].Value<float>();
                    break;
                case "hardlookAt":
                case "hardlookat":
                    if (aim != null) vcam.DestroyCinemachineComponent(CinemachineCore.Stage.Aim);
                    vcam.AddCinemachineComponent<CinemachineHardLookAt>();
                    break;
                case "pov":
                    if (aim == null || !(aim is CinemachinePOV))
                    {
                        if (aim != null) vcam.DestroyCinemachineComponent(CinemachineCore.Stage.Aim);
                        aim = vcam.AddCinemachineComponent<CinemachinePOV>();
                    }
                    break;
            }

            return new { camera = go.name, aimType };
        }

        private static object SetNoise(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var noise = vcam.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
            if (noise == null)
                noise = vcam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            if (p["amplitudeGain"] != null) noise.m_AmplitudeGain = p["amplitudeGain"].Value<float>();
            if (p["frequencyGain"] != null) noise.m_FrequencyGain = p["frequencyGain"].Value<float>();

            if (p["profileName"] != null)
            {
                var profileName = p["profileName"].Value<string>();
                var guids = AssetDatabase.FindAssets("t:NoiseSettings " + profileName);
                if (guids.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    noise.m_NoiseProfile = AssetDatabase.LoadAssetAtPath<NoiseSettings>(path);
                }
            }

            return new { camera = go.name, amplitudeGain = noise.m_AmplitudeGain, frequencyGain = noise.m_FrequencyGain };
        }

        private static object SetPriority(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);

            var priority = Validate.Required<int>(p, "priority");
            WorkflowManager.SnapshotObject(go);
            vcam.Priority = priority;

            return new { camera = go.name, priority };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);

            return new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
                priority = vcam.Priority,
                follow = vcam.Follow?.name,
                lookAt = vcam.LookAt?.name,
                fov = vcam.m_Lens.FieldOfView,
                nearClip = vcam.m_Lens.NearClipPlane,
                farClip = vcam.m_Lens.FarClipPlane,
                body = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body)?.GetType().Name,
                aim = vcam.GetCinemachineComponent(CinemachineCore.Stage.Aim)?.GetType().Name,
                noise = vcam.GetCinemachineComponent(CinemachineCore.Stage.Noise)?.GetType().Name,
            };
        }

        private static object FindCameras(JToken p)
        {
            var vcams = UnityEngine.Object.FindObjectsOfType<CinemachineVirtualCamera>();
            var freeLooks = UnityEngine.Object.FindObjectsOfType<CinemachineFreeLook>();

            var results = new List<object>();
            foreach (var v in vcams)
                results.Add(new { name = v.gameObject.name, path = GameObjectFinder.GetPath(v.gameObject), type = "VirtualCamera", priority = v.Priority });
            foreach (var f in freeLooks)
                results.Add(new { name = f.gameObject.name, path = GameObjectFinder.GetPath(f.gameObject), type = "FreeLook", priority = f.Priority });

            return new { count = results.Count, cameras = results };
        }

        private static object CreateBlendList(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "BlendListCamera";
            var go = new GameObject(name);
            var blendList = go.AddComponent<CinemachineBlendListCamera>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create BlendList");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object SetLens(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var lens = vcam.m_Lens;
            if (p["fieldOfView"] != null) lens.FieldOfView = p["fieldOfView"].Value<float>();
            if (p["nearClipPlane"] != null) lens.NearClipPlane = p["nearClipPlane"].Value<float>();
            if (p["farClipPlane"] != null) lens.FarClipPlane = p["farClipPlane"].Value<float>();
            if (p["orthographicSize"] != null) lens.OrthographicSize = p["orthographicSize"].Value<float>();
            if (p["dutch"] != null) lens.Dutch = p["dutch"].Value<float>();
            vcam.m_Lens = lens;

            return new { camera = go.name, fov = lens.FieldOfView, near = lens.NearClipPlane, far = lens.FarClipPlane };
        }

        private static object CreateDollyTrack(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "DollyTrack";
            var go = new GameObject(name);
            var track = go.AddComponent<CinemachineSmoothPath>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create DollyTrack");

            var points = p["points"]?.ToObject<float[][]>();
            if (points != null && points.Length > 0)
            {
                var waypoints = new CinemachineSmoothPath.Waypoint[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    waypoints[i] = new CinemachineSmoothPath.Waypoint
                    {
                        position = new Vector3(
                            points[i].Length > 0 ? points[i][0] : 0,
                            points[i].Length > 1 ? points[i][1] : 0,
                            points[i].Length > 2 ? points[i][2] : 0
                        )
                    };
                }
                track.m_Waypoints = waypoints;
            }

            return new { name = go.name, path = GameObjectFinder.GetPath(go), pointCount = track.m_Waypoints.Length };
        }

        private static object AddDollyPoint(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var track = go.GetComponent<CinemachineSmoothPath>();
            if (track == null) throw new McpException(-32003, "CinemachineSmoothPath not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var position = new Vector3(
                p["position"]?["x"]?.Value<float>() ?? 0,
                p["position"]?["y"]?.Value<float>() ?? 0,
                p["position"]?["z"]?.Value<float>() ?? 0
            );

            var waypoints = track.m_Waypoints.ToList();
            waypoints.Add(new CinemachineSmoothPath.Waypoint { position = position });
            track.m_Waypoints = waypoints.ToArray();

            return new { track = go.name, pointCount = track.m_Waypoints.Length, addedPosition = new { x = position.x, y = position.y, z = position.z } };
        }

        private static object SetConfiner(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var confiner = go.GetComponent<CinemachineConfiner>();
            if (confiner == null) confiner = Undo.AddComponent<CinemachineConfiner>(go);

            var boundsName = p["boundsObject"]?.Value<string>();
            if (boundsName != null)
            {
                var boundsGo = GameObjectFinder.FindOrThrow(name: boundsName);
                var collider = boundsGo.GetComponent<Collider>();
                if (collider != null) confiner.m_BoundingVolume = collider;
                var collider2d = boundsGo.GetComponent<Collider2D>();
                if (collider2d != null) confiner.m_BoundingShape2D = collider2d;
            }

            return new { camera = go.name, confinerAdded = true };
        }

        private static object CreateMixingCamera(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "MixingCamera";
            var go = new GameObject(name);
            go.AddComponent<CinemachineMixingCamera>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create MixingCamera");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object CreateClearShot(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "ClearShotCamera";
            var go = new GameObject(name);
            go.AddComponent<CinemachineClearShot>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create ClearShot");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object SetDeadZone(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var composer = vcam.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachineComposer;
            if (composer == null) throw new McpException(-32003, "CinemachineComposer not found -- set aim to Composer first");

            if (p["width"] != null) composer.m_DeadZoneWidth = p["width"].Value<float>();
            if (p["height"] != null) composer.m_DeadZoneHeight = p["height"].Value<float>();
            if (p["softZoneWidth"] != null) composer.m_SoftZoneWidth = p["softZoneWidth"].Value<float>();
            if (p["softZoneHeight"] != null) composer.m_SoftZoneHeight = p["softZoneHeight"].Value<float>();

            return new { camera = go.name, deadZoneWidth = composer.m_DeadZoneWidth, deadZoneHeight = composer.m_DeadZoneHeight };
        }

        private static object CreateGroup(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "TargetGroup";
            var go = new GameObject(name);
            var group = go.AddComponent<CinemachineTargetGroup>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create TargetGroup");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object AddGroupTarget(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var group = go.GetComponent<CinemachineTargetGroup>();
            if (group == null) throw new McpException(-32003, "CinemachineTargetGroup not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var targetName = Validate.Required<string>(p, "target");
            var target = GameObjectFinder.FindOrThrow(name: targetName);
            var weight = p["weight"]?.Value<float>() ?? 1f;
            var radius = p["radius"]?.Value<float>() ?? 1f;

            var targets = group.m_Targets.ToList();
            targets.Add(new CinemachineTargetGroup.Target { target = target.transform, weight = weight, radius = radius });
            group.m_Targets = targets.ToArray();

            return new { group = go.name, targetAdded = target.name, totalTargets = group.m_Targets.Length };
        }

        private static object SetGroupFraming(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var vcam = go.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null) throw new McpException(-32003, "CinemachineVirtualCamera not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var composer = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineFramingTransposer;
            if (composer == null) throw new McpException(-32003, "CinemachineFramingTransposer not found -- set body to framingTransposer first");

            if (p["groupFramingSize"] != null) composer.m_GroupFramingSize = p["groupFramingSize"].Value<float>();
            if (p["damping"] != null)
            {
                composer.m_XDamping = p["damping"].Value<float>();
                composer.m_YDamping = p["damping"].Value<float>();
                composer.m_ZDamping = p["damping"].Value<float>();
            }

            return new { camera = go.name, groupFramingSize = composer.m_GroupFramingSize };
        }
#endif
    }
}
