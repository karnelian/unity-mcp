using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class CameraHandler
    {
        public static void Register()
        {
            CommandRouter.Register("camera.create", Create);
            CommandRouter.Register("camera.setProperties", SetProperties);
            CommandRouter.Register("camera.getProperties", GetProperties);
            CommandRouter.Register("camera.find", FindCameras);
            CommandRouter.Register("camera.lookAt", LookAt);
            CommandRouter.Register("camera.screenshot", Screenshot);
            CommandRouter.Register("camera.setCullingMask", SetCullingMask);
            CommandRouter.Register("camera.setMain", SetMain);
            CommandRouter.Register("camera.setViewport", SetViewport);
            CommandRouter.Register("camera.setClipPlanes", SetClipPlanes);
            CommandRouter.Register("camera.getMain", GetMain);
        }

        private static object CameraInfo(Camera cam)
        {
            var go = cam.gameObject;
            return new
            {
                name = go.name,
                instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                fieldOfView = cam.fieldOfView,
                nearClipPlane = cam.nearClipPlane,
                farClipPlane = cam.farClipPlane,
                orthographic = cam.orthographic,
                orthographicSize = cam.orthographicSize,
                depth = cam.depth,
                clearFlags = cam.clearFlags.ToString(),
                backgroundColor = new { cam.backgroundColor.r, cam.backgroundColor.g, cam.backgroundColor.b, cam.backgroundColor.a },
                cullingMask = cam.cullingMask,
                viewport = new { cam.rect.x, cam.rect.y, width = cam.rect.width, height = cam.rect.height },
                isMain = cam == Camera.main,
            };
        }

        private static object Create(JToken p)
        {
            var camName = p["name"]?.Value<string>() ?? "New Camera";
            var go = new GameObject(camName);
            Undo.RegisterCreatedObjectUndo(go, "MCP: Create Camera");
            var cam = go.AddComponent<Camera>();

            if (p["orthographic"] != null) cam.orthographic = p["orthographic"].Value<bool>();
            if (p["fieldOfView"] != null) cam.fieldOfView = p["fieldOfView"].Value<float>();
            if (p["depth"] != null) cam.depth = p["depth"].Value<float>();

            if (p["position"] != null)
            {
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0
                );
            }
            if (p["rotation"] != null)
            {
                go.transform.eulerAngles = new Vector3(
                    p["rotation"]["x"]?.Value<float>() ?? 0,
                    p["rotation"]["y"]?.Value<float>() ?? 0,
                    p["rotation"]["z"]?.Value<float>() ?? 0
                );
            }

            var parentPath = p["parent"]?.Value<string>();
            if (!string.IsNullOrEmpty(parentPath))
            {
                var parent = GameObject.Find(parentPath);
                if (parent != null) go.transform.SetParent(parent.transform, true);
            }

            return CameraInfo(cam);
        }

        private static object SetProperties(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var cam = go.GetComponent<Camera>();
            if (cam == null) throw new McpException(-32602, $"No Camera on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(cam, "MCP: Set Camera Properties");

            if (p["fieldOfView"] != null) cam.fieldOfView = p["fieldOfView"].Value<float>();
            if (p["orthographic"] != null) cam.orthographic = p["orthographic"].Value<bool>();
            if (p["orthographicSize"] != null) cam.orthographicSize = p["orthographicSize"].Value<float>();
            if (p["depth"] != null) cam.depth = p["depth"].Value<float>();
            if (p["nearClipPlane"] != null) cam.nearClipPlane = p["nearClipPlane"].Value<float>();
            if (p["farClipPlane"] != null) cam.farClipPlane = p["farClipPlane"].Value<float>();
            if (p["clearFlags"] != null) cam.clearFlags = Validate.ParseEnum<CameraClearFlags>(p["clearFlags"].Value<string>(), "clearFlags");
            if (p["backgroundColor"] != null)
            {
                var c = p["backgroundColor"];
                cam.backgroundColor = new Color(c["r"]?.Value<float>() ?? 0, c["g"]?.Value<float>() ?? 0, c["b"]?.Value<float>() ?? 0, c["a"]?.Value<float>() ?? 1);
            }

            return CameraInfo(cam);
        }

        private static object GetProperties(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var cam = go.GetComponent<Camera>();
            if (cam == null) throw new McpException(-32602, $"No Camera on '{go.name}'");
            return CameraInfo(cam);
        }

        private static object FindCameras(JToken p)
        {
            var cameras = Camera.allCameras;
            return new { cameras = cameras.Select(CameraInfo).ToArray() };
        }

        private static object LookAt(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var cam = go.GetComponent<Camera>();
            if (cam == null) throw new McpException(-32602, $"No Camera on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(go.transform, "MCP: Camera LookAt");

            if (p["target"] != null)
            {
                var target = new Vector3(
                    p["target"]["x"]?.Value<float>() ?? 0,
                    p["target"]["y"]?.Value<float>() ?? 0,
                    p["target"]["z"]?.Value<float>() ?? 0
                );
                go.transform.LookAt(target);
            }
            else if (p["targetObject"] != null)
            {
                var targetGo = GameObject.Find(p["targetObject"].Value<string>());
                if (targetGo == null) throw new McpException(-32003, $"Target not found: {p["targetObject"]}");
                go.transform.LookAt(targetGo.transform);
            }

            return CameraInfo(cam);
        }

        private static object Screenshot(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var cam = go.GetComponent<Camera>();
            if (cam == null) throw new McpException(-32602, $"No Camera on '{go.name}'");

            var width = p["width"]?.Value<int>() ?? 1920;
            var height = p["height"]?.Value<int>() ?? 1080;
            var savePath = p["savePath"]?.Value<string>() ?? "Assets/Screenshots/screenshot.png";
            var superSize = p["superSize"]?.Value<int>() ?? 1;

            var dir = Path.GetDirectoryName(Path.Combine(Application.dataPath, "..", savePath));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;

            var bytes = tex.EncodeToPNG();
            var fullPath = Path.Combine(Application.dataPath, "..", savePath);
            File.WriteAllBytes(fullPath, bytes);

            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.Refresh();

            return new { saved = true, path = savePath, width, height };
        }

        private static object SetCullingMask(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var cam = go.GetComponent<Camera>();
            if (cam == null) throw new McpException(-32602, $"No Camera on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(cam, "MCP: Set Culling Mask");

            if (p["layers"] != null)
            {
                int mask = 0;
                foreach (var layer in p["layers"])
                {
                    var layerName = layer.Value<string>();
                    int layerIndex = LayerMask.NameToLayer(layerName);
                    if (layerIndex >= 0) mask |= 1 << layerIndex;
                }
                cam.cullingMask = mask;
            }
            else if (p["mask"] != null)
            {
                cam.cullingMask = p["mask"].Value<int>();
            }

            return new { name = go.name, cullingMask = cam.cullingMask };
        }

        private static object SetMain(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var cam = go.GetComponent<Camera>();
            if (cam == null) throw new McpException(-32602, $"No Camera on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(go, "MCP: Set Main Camera");
            go.tag = "MainCamera";

            return new { name = go.name, isMain = true };
        }

        private static object SetViewport(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var cam = go.GetComponent<Camera>();
            if (cam == null) throw new McpException(-32602, $"No Camera on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(cam, "MCP: Set Viewport");
            cam.rect = new Rect(
                p["x"]?.Value<float>() ?? 0,
                p["y"]?.Value<float>() ?? 0,
                p["width"]?.Value<float>() ?? 1,
                p["height"]?.Value<float>() ?? 1
            );

            return CameraInfo(cam);
        }

        private static object SetClipPlanes(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var cam = go.GetComponent<Camera>();
            if (cam == null) throw new McpException(-32602, $"No Camera on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(cam, "MCP: Set Clip Planes");
            if (p["near"] != null) cam.nearClipPlane = p["near"].Value<float>();
            if (p["far"] != null) cam.farClipPlane = p["far"].Value<float>();

            return new { name = go.name, nearClipPlane = cam.nearClipPlane, farClipPlane = cam.farClipPlane };
        }

        private static object GetMain(JToken p)
        {
            var main = Camera.main;
            if (main == null) return new { found = false };
            return CameraInfo(main);
        }
    }
}
