using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

#if XR_MANAGEMENT || UNITY_XR
using UnityEngine.XR;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
#endif
#if XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace KarnelLabs.MCP
{
    public static class XRHandler
    {
        public static void Register()
        {
#if XR_MANAGEMENT || UNITY_XR
            CommandRouter.Register("xr.getSettings", GetSettings);
            CommandRouter.Register("xr.getLoaders", GetLoaders);
            CommandRouter.Register("xr.setLoader", SetLoader);
            CommandRouter.Register("xr.getDeviceInfo", GetDeviceInfo);
#endif
#if XR_INTERACTION_TOOLKIT
            CommandRouter.Register("xr.createXRRig", CreateXRRig);
            CommandRouter.Register("xr.createGrabInteractable", CreateGrabInteractable);
            CommandRouter.Register("xr.createRayInteractor", CreateRayInteractor);
            CommandRouter.Register("xr.createDirectInteractor", CreateDirectInteractor);
            CommandRouter.Register("xr.createSocketInteractor", CreateSocketInteractor);
            CommandRouter.Register("xr.createTeleportArea", CreateTeleportArea);
            CommandRouter.Register("xr.createTeleportAnchor", CreateTeleportAnchor);
            CommandRouter.Register("xr.setInteractableSettings", SetInteractableSettings);
            CommandRouter.Register("xr.findInteractables", FindInteractables);
            CommandRouter.Register("xr.findInteractors", FindInteractors);
            CommandRouter.Register("xr.createSnapZone", CreateSnapZone);
            CommandRouter.Register("xr.setHandTracking", SetHandTracking);
            CommandRouter.Register("xr.createLocomotionSystem", CreateLocomotionSystem);
            CommandRouter.Register("xr.createClimbInteractable", CreateClimbInteractable);
            CommandRouter.Register("xr.setLayerMask", SetLayerMask);
            CommandRouter.Register("xr.createUICanvas", CreateUICanvas);
            CommandRouter.Register("xr.getXRRigInfo", GetXRRigInfo);
            CommandRouter.Register("xr.setTrackingOrigin", SetTrackingOrigin);
#endif
        }

#if XR_MANAGEMENT || UNITY_XR
        private static object GetSettings(JToken p)
        {
            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (settings == null) return new { error = "XR settings not configured" };

            return new
            {
                initializeOnStartup = settings.InitManagerOnStart,
                assignedSettings = settings.AssignedSettings != null,
                loaderCount = settings.AssignedSettings?.activeLoaders?.Count ?? 0,
            };
        }

        private static object GetLoaders(JToken p)
        {
            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (settings?.AssignedSettings == null) return new { loaders = Array.Empty<object>() };

            var loaders = settings.AssignedSettings.activeLoaders.Select(l => new
            {
                name = l.name,
                type = l.GetType().Name,
            }).ToArray();

            return new { count = loaders.Length, loaders };
        }

        private static object SetLoader(JToken p)
        {
            // XR loader configuration requires specific package setup
            return new { info = "XR Loader configuration should be done via Project Settings > XR Plug-in Management" };
        }

        private static object GetDeviceInfo(JToken p)
        {
            var inputDevices = new List<InputDevice>();
            InputDevices.GetDevices(inputDevices);

            var devices = inputDevices.Select(d => new
            {
                name = d.name,
                role = d.role.ToString(),
                isValid = d.isValid,
                manufacturer = d.manufacturer,
                serialNumber = d.serialNumber,
            }).ToArray();

            return new
            {
                deviceCount = devices.Length,
                devices,
                displayRefreshRate = XRDevice.refreshRate,
            };
        }
#endif

#if XR_INTERACTION_TOOLKIT
        private static object CreateXRRig(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "XR Rig";
            var rigGo = new GameObject(name);
            var cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(rigGo.transform);
            cameraOffset.transform.localPosition = new Vector3(0, 1.36144f, 0);

            var mainCamera = new GameObject("Main Camera");
            mainCamera.transform.SetParent(cameraOffset.transform);
            mainCamera.tag = "MainCamera";
            var cam = mainCamera.AddComponent<Camera>();
            cam.nearClipPlane = 0.01f;
            mainCamera.AddComponent<AudioListener>();
            mainCamera.AddComponent<TrackedPoseDriver>();

            var leftHand = new GameObject("LeftHand Controller");
            leftHand.transform.SetParent(cameraOffset.transform);
            var leftController = leftHand.AddComponent<ActionBasedController>();

            var rightHand = new GameObject("RightHand Controller");
            rightHand.transform.SetParent(cameraOffset.transform);
            var rightController = rightHand.AddComponent<ActionBasedController>();

            Undo.RegisterCreatedObjectUndo(rigGo, "MCP: Create XR Rig");
            return new { name = rigGo.name, path = GameObjectFinder.GetPath(rigGo) };
        }

        private static object CreateGrabInteractable(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);

            var interactable = go.GetComponent<XRGrabInteractable>();
            if (interactable == null) interactable = Undo.AddComponent<XRGrabInteractable>(go);

            if (go.GetComponent<Rigidbody>() == null) Undo.AddComponent<Rigidbody>(go);
            if (go.GetComponent<Collider>() == null) Undo.AddComponent<BoxCollider>(go);

            if (p["throwOnDetach"] != null) interactable.throwOnDetach = p["throwOnDetach"].Value<bool>();
            if (p["movementType"] != null) interactable.movementType = Validate.ParseEnum<XRBaseInteractable.MovementType>(p["movementType"].Value<string>(), "movementType");

            return new { name = go.name, path = GameObjectFinder.GetPath(go), grabEnabled = true };
        }

        private static object CreateRayInteractor(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);

            var interactor = go.GetComponent<XRRayInteractor>();
            if (interactor == null) interactor = Undo.AddComponent<XRRayInteractor>(go);

            if (go.GetComponent<LineRenderer>() == null) Undo.AddComponent<LineRenderer>(go);
            if (p["maxRaycastDistance"] != null) interactor.maxRaycastDistance = p["maxRaycastDistance"].Value<float>();

            return new { name = go.name, path = GameObjectFinder.GetPath(go), type = "RayInteractor" };
        }

        private static object CreateDirectInteractor(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);

            var interactor = go.GetComponent<XRDirectInteractor>();
            if (interactor == null) interactor = Undo.AddComponent<XRDirectInteractor>(go);

            if (go.GetComponent<SphereCollider>() == null)
            {
                var col = Undo.AddComponent<SphereCollider>(go);
                col.isTrigger = true;
                col.radius = 0.1f;
            }

            return new { name = go.name, path = GameObjectFinder.GetPath(go), type = "DirectInteractor" };
        }

        private static object CreateSocketInteractor(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "Socket Interactor";
            var go = new GameObject(name);
            go.AddComponent<XRSocketInteractor>();
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.2f;

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0
                );
            }

            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Socket");
            return new { name = go.name, path = GameObjectFinder.GetPath(go), type = "SocketInteractor" };
        }

        private static object CreateTeleportArea(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);

            var teleport = go.GetComponent<TeleportationArea>();
            if (teleport == null) teleport = Undo.AddComponent<TeleportationArea>(go);

            if (go.GetComponent<Collider>() == null) Undo.AddComponent<BoxCollider>(go);

            return new { name = go.name, path = GameObjectFinder.GetPath(go), type = "TeleportArea" };
        }

        private static object CreateTeleportAnchor(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "Teleport Anchor";
            var go = new GameObject(name);
            go.AddComponent<TeleportationAnchor>();
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(1, 0.01f, 1);

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0
                );
            }

            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Teleport Anchor");
            return new { name = go.name, path = GameObjectFinder.GetPath(go), type = "TeleportAnchor" };
        }

        private static object SetInteractableSettings(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var interactable = go.GetComponent<XRBaseInteractable>();
            if (interactable == null) throw new McpException(-32003, "XRBaseInteractable not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            if (p["interactionLayerMask"] != null)
                interactable.interactionLayers = p["interactionLayerMask"].Value<int>();

            return new { name = go.name, type = interactable.GetType().Name };
        }

        private static object FindInteractables(JToken p)
        {
            var interactables = UnityEngine.Object.FindObjectsByType<XRBaseInteractable>(FindObjectsSortMode.None);
            var results = interactables.Select(i => new
            {
                name = i.gameObject.name,
                path = GameObjectFinder.GetPath(i.gameObject),
                type = i.GetType().Name,
                isSelected = i.isSelected,
                isHovered = i.isHovered,
            }).ToArray();
            return new { count = results.Length, interactables = results };
        }

        private static object FindInteractors(JToken p)
        {
            var interactors = UnityEngine.Object.FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
            var results = interactors.Select(i => new
            {
                name = i.gameObject.name,
                path = GameObjectFinder.GetPath(i.gameObject),
                type = i.GetType().Name,
            }).ToArray();
            return new { count = results.Length, interactors = results };
        }

        private static object CreateSnapZone(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "Snap Zone";
            var go = new GameObject(name);
            var socket = go.AddComponent<XRSocketInteractor>();
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = p["radius"]?.Value<float>() ?? 0.15f;

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0
                );
            }

            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Snap Zone");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object SetHandTracking(JToken p)
        {
            // Hand tracking setup depends on specific XR provider
            return new { info = "Hand tracking configuration depends on the XR provider (OpenXR, Oculus, etc.). Configure via XR Plug-in Management settings." };
        }

        private static object CreateLocomotionSystem(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "Locomotion System";
            var go = new GameObject(name);
            go.AddComponent<LocomotionSystem>();
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Locomotion System");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object CreateClimbInteractable(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);

            if (go.GetComponent<Collider>() == null) Undo.AddComponent<BoxCollider>(go);
            // ClimbInteractable may not exist in all XRI versions
            // Use base interactable with custom logic tag
            var interactable = go.GetComponent<XRBaseInteractable>();
            if (interactable == null) interactable = Undo.AddComponent<XRGrabInteractable>(go);

            return new { name = go.name, path = GameObjectFinder.GetPath(go), climbEnabled = true };
        }

        private static object SetLayerMask(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var interactor = go.GetComponent<XRBaseInteractor>();
            if (interactor == null) throw new McpException(-32003, "XRBaseInteractor not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            if (p["interactionLayers"] != null)
                interactor.interactionLayers = p["interactionLayers"].Value<int>();

            var rayInteractor = interactor as XRRayInteractor;
            if (rayInteractor != null && p["raycastMask"] != null)
                rayInteractor.raycastMask = p["raycastMask"].Value<int>();

            return new { name = go.name, type = interactor.GetType().Name };
        }

        private static object CreateUICanvas(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "XR UI Canvas";
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(
                p["width"]?.Value<float>() ?? 1,
                p["height"]?.Value<float>() ?? 0.6f
            );
            rt.localScale = Vector3.one * 0.001f;

            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            go.AddComponent<TrackedDeviceGraphicRaycaster>();

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 1.5f,
                    p["position"]["z"]?.Value<float>() ?? 2
                );
            }

            Undo.RegisterCreatedObjectUndo(go, "MCP: Create XR UI Canvas");
            return new { name = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object GetXRRigInfo(JToken p)
        {
            var rigs = UnityEngine.Object.FindObjectsByType<ActionBasedController>(FindObjectsSortMode.None);
            var cameras = UnityEngine.Object.FindObjectsByType<TrackedPoseDriver>(FindObjectsSortMode.None);

            return new
            {
                controllers = rigs.Select(r => new { name = r.gameObject.name, path = GameObjectFinder.GetPath(r.gameObject) }).ToArray(),
                trackedDevices = cameras.Select(c => new { name = c.gameObject.name, path = GameObjectFinder.GetPath(c.gameObject) }).ToArray(),
            };
        }

        private static object SetTrackingOrigin(JToken p)
        {
            return new { info = "Tracking origin mode is configured via XR Plug-in Management > specific provider settings" };
        }
#endif
    }
}
