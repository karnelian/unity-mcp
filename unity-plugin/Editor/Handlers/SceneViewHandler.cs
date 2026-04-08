using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class SceneViewHandler
    {
        public static void Register()
        {
            CommandRouter.Register("sceneView.setCamera", SetCamera);
            CommandRouter.Register("sceneView.frame", Frame);
            CommandRouter.Register("sceneView.toggle2D", Toggle2D);
            CommandRouter.Register("sceneView.setGizmos", SetGizmos);
            CommandRouter.Register("sceneView.align", Align);
            CommandRouter.Register("sceneView.getInfo", GetInfo);
        }

        private static SceneView GetSceneView()
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null) sv = SceneView.sceneViews.Count > 0 ? (SceneView)SceneView.sceneViews[0] : null;
            if (sv == null) throw new McpException(-32000, "No Scene View available");
            return sv;
        }

        private static object SetCamera(JToken p)
        {
            var sv = GetSceneView();

            if (p["position"] != null)
                sv.pivot = JsonHelper.ToVector3(p["position"]);
            if (p["rotation"] != null)
                sv.rotation = Quaternion.Euler(JsonHelper.ToVector3(p["rotation"]));
            if (p["size"] != null)
                sv.size = (float)p["size"];
            if (p["orthographic"] != null)
                sv.orthographic = (bool)p["orthographic"];

            sv.Repaint();
            return new { success = true };
        }

        private static object Frame(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var sv = GetSceneView();

            bool instant = p["instant"]?.Value<bool>() ?? false;

            // Select and frame
            Selection.activeGameObject = go;
            if (instant)
                sv.FrameSelected();
            else
                sv.FrameSelected();

            sv.Repaint();
            return new { success = true, gameObject = go.name };
        }

        private static object Toggle2D(JToken p)
        {
            var sv = GetSceneView();
            sv.in2DMode = (bool)p["enable"];
            sv.Repaint();
            return new { success = true, is2D = sv.in2DMode };
        }

        private static object SetGizmos(JToken p)
        {
            var sv = GetSceneView();

            if (p["showGizmos"] != null) sv.drawGizmos = (bool)p["showGizmos"];
            if (p["showGrid"] != null) sv.showGrid = (bool)p["showGrid"];

            sv.Repaint();
            return new { success = true };
        }

        private static object Align(JToken p)
        {
            var sv = GetSceneView();
            string alignTo = (string)p["alignTo"];

            switch (alignTo)
            {
                case "Selection":
                    if (Selection.activeTransform != null)
                    {
                        sv.AlignViewToObject(Selection.activeTransform);
                    }
                    break;
                case "Front":
                    sv.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case "Back":
                    sv.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case "Left":
                    sv.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case "Right":
                    sv.rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case "Top":
                    sv.rotation = Quaternion.Euler(90, 0, 0);
                    break;
                case "Bottom":
                    sv.rotation = Quaternion.Euler(-90, 0, 0);
                    break;
            }

            sv.Repaint();
            return new { success = true, alignedTo = alignTo };
        }

        private static object GetInfo(JToken p)
        {
            var sv = GetSceneView();
            var pivot = sv.pivot;
            var rot = sv.rotation.eulerAngles;

            return new
            {
                position = new { x = pivot.x, y = pivot.y, z = pivot.z },
                rotation = new { x = rot.x, y = rot.y, z = rot.z },
                size = sv.size,
                orthographic = sv.orthographic,
                is2D = sv.in2DMode,
                drawGizmos = sv.drawGizmos,
                showGrid = sv.showGrid,
                cameraDistance = sv.cameraDistance,
            };
        }
    }
}
