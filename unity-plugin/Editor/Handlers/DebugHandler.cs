using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class DebugHandler
    {
        public static void Register()
        {
            CommandRouter.Register("debug.screenshot", CaptureScreenshot);
            CommandRouter.Register("debug.captureEditorWindow", CaptureEditorWindow);
            CommandRouter.Register("debug.log", Log);
            CommandRouter.Register("debug.clearConsole", ClearConsole);
            CommandRouter.Register("debug.getPrefs", GetPrefs);
            CommandRouter.Register("debug.setPrefs", SetPrefs);
            CommandRouter.Register("debug.deletePrefs", DeletePrefs);
            CommandRouter.Register("debug.getEditorPrefs", GetEditorPrefs);
            CommandRouter.Register("debug.setEditorPrefs", SetEditorPrefs);
            CommandRouter.Register("debug.drawGizmo", DrawGizmo);
            CommandRouter.Register("debug.getSystemInfo", GetSystemInfo);
            CommandRouter.Register("debug.startCapture", StartCapture);
            CommandRouter.Register("debug.stopCapture", StopCapture);
            CommandRouter.Register("debug.getCapturedLogs", GetCapturedLogs);
            CommandRouter.Register("debug.getDefines", GetDefines);
            CommandRouter.Register("debug.setDefines", SetDefines);
            CommandRouter.Register("debug.forceRecompile", ForceRecompile);
        }

        private static object CaptureScreenshot(JToken p)
        {
            string view = (string)p?["view"] ?? "game";
            int width = (int?)p?["width"] ?? 0;
            int height = (int?)p?["height"] ?? 0;

            if (view == "both") view = "scene";
            if (view == "game" && !EditorApplication.isPlaying) view = "scene";

            Texture2D texture = null;
            try
            {
                if (view == "scene")
                {
                    var sceneView = SceneView.lastActiveSceneView;
                    if (sceneView == null) throw new Exception("No active Scene View found.");
                    var camera = sceneView.camera;
                    int w = width > 0 ? width : (int)sceneView.position.width;
                    int h = height > 0 ? height : (int)sceneView.position.height;
                    var rt = new RenderTexture(w, h, 24);
                    camera.targetTexture = rt;
                    camera.Render();
                    RenderTexture.active = rt;
                    texture = new Texture2D(w, h, TextureFormat.RGB24, false);
                    texture.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                    texture.Apply();
                    camera.targetTexture = null;
                    RenderTexture.active = null;
                    UnityEngine.Object.DestroyImmediate(rt);
                }
                else
                {
                    texture = ScreenCapture.CaptureScreenshotAsTexture();
                    if (texture == null) throw new Exception("Failed to capture Game View screenshot.");
                    if (width > 0 && height > 0 && (texture.width != width || texture.height != height))
                    {
                        var resized = new Texture2D(width, height, TextureFormat.RGB24, false);
                        var rt = RenderTexture.GetTemporary(width, height);
                        Graphics.Blit(texture, rt);
                        RenderTexture.active = rt;
                        resized.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                        resized.Apply();
                        RenderTexture.active = null;
                        RenderTexture.ReleaseTemporary(rt);
                        UnityEngine.Object.DestroyImmediate(texture);
                        texture = resized;
                    }
                }

                byte[] pngData = texture.EncodeToPNG();
                return new
                {
                    success = true, view, width = texture.width, height = texture.height,
                    format = "png", size = pngData.Length, data = Convert.ToBase64String(pngData),
                };
            }
            finally
            {
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static object CaptureEditorWindow(JToken p)
        {
            string windowName = (string)p?["window"] ?? "InspectorWindow";
            string savePath = (string)p?["savePath"];

            // Common shortcut aliases
            switch (windowName.ToLower())
            {
                case "inspector": windowName = "InspectorWindow"; break;
                case "hierarchy": windowName = "SceneHierarchyWindow"; break;
                case "project": windowName = "ProjectBrowser"; break;
                case "console": windowName = "ConsoleWindow"; break;
                case "game": windowName = "GameView"; break;
                case "scene": windowName = "SceneView"; break;
                case "animation": windowName = "AnimationWindow"; break;
                case "animator": windowName = "AnimatorControllerTool"; break;
                case "profiler": windowName = "ProfilerWindow"; break;
            }

            var editorAsm = typeof(UnityEditor.Editor).Assembly;
            Type type = editorAsm.GetType($"UnityEditor.{windowName}");
            if (type == null) type = editorAsm.GetType(windowName);
            if (type == null)
            {
                // Try all loaded assemblies for third-party windows
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetTypes().FirstOrDefault(t => t.Name == windowName && typeof(EditorWindow).IsAssignableFrom(t));
                    if (type != null) break;
                }
            }
            if (type == null) throw new Exception($"EditorWindow type not found: {windowName}");

            var windows = Resources.FindObjectsOfTypeAll(type);
            if (windows.Length == 0) throw new Exception($"No {windowName} is currently open. Open it from Window menu first.");

            var window = (EditorWindow)windows[0];
            window.Focus();
            window.Repaint();
            InternalEditorUtility.RepaintAllViews();

            // Access m_Parent (HostView/GUIView) via reflection
            var parentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var parent = parentField?.GetValue(window);
            if (parent == null) throw new Exception("Window has no parent GUIView (m_Parent null)");

            // Walk up to find GUIView type
            Type guiViewType = parent.GetType();
            while (guiViewType != null && guiViewType.Name != "GUIView") guiViewType = guiViewType.BaseType;
            if (guiViewType == null) throw new Exception("GUIView base type not found in parent chain");

            // GrabPixels(RenderTexture rt, Rect rect) — internal instance method
            var grabMethod = guiViewType.GetMethod("GrabPixels", BindingFlags.Instance | BindingFlags.NonPublic);
            if (grabMethod == null) throw new Exception("GUIView.GrabPixels method not found (Unity version may not support this)");

            var rect = window.position;
            int w = Mathf.Max(1, (int)rect.width);
            int h = Mathf.Max(1, (int)rect.height);

            var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32);
            Texture2D tex = null;
            try
            {
                grabMethod.Invoke(parent, new object[] { rt, new Rect(0, 0, w, h) });

                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();
                RenderTexture.active = prev;

                // Flip vertically (RenderTexture uses bottom-left origin, PNG uses top-left)
                var pixels = tex.GetPixels();
                var flipped = new Color[pixels.Length];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        flipped[(h - 1 - y) * w + x] = pixels[y * w + x];
                    }
                }
                tex.SetPixels(flipped);
                tex.Apply();

                byte[] png = tex.EncodeToPNG();
                if (!string.IsNullOrEmpty(savePath))
                {
                    var dir = System.IO.Path.GetDirectoryName(savePath);
                    if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.WriteAllBytes(savePath, png);
                }
                return new
                {
                    success = true,
                    window = windowName,
                    width = w,
                    height = h,
                    format = "png",
                    size = png.Length,
                    savedTo = savePath,
                    data = Convert.ToBase64String(png),
                };
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static object Log(JToken p)
        {
            var message = Validate.Required<string>(p, "message");
            var level = p["level"]?.Value<string>()?.ToLower() ?? "info";
            switch (level)
            {
                case "warning": Debug.LogWarning($"[MCP] {message}"); break;
                case "error": Debug.LogError($"[MCP] {message}"); break;
                default: Debug.Log($"[MCP] {message}"); break;
            }
            return new { logged = true, level, message };
        }

        private static object ClearConsole(JToken p)
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var logEntries = assembly.GetType("UnityEditor.LogEntries");
            var clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null, null);
            return new { cleared = true };
        }

        private static object GetPrefs(JToken p)
        {
            var key = Validate.Required<string>(p, "key");
            var type = p["type"]?.Value<string>()?.ToLower() ?? "string";
            object value;
            switch (type)
            {
                case "int": value = PlayerPrefs.GetInt(key); break;
                case "float": value = PlayerPrefs.GetFloat(key); break;
                default: value = PlayerPrefs.GetString(key); break;
            }
            return new { key, type, value };
        }

        private static object SetPrefs(JToken p)
        {
            var key = Validate.Required<string>(p, "key");
            var type = p["type"]?.Value<string>()?.ToLower() ?? "string";
            switch (type)
            {
                case "int": PlayerPrefs.SetInt(key, Validate.Required<int>(p, "value")); break;
                case "float": PlayerPrefs.SetFloat(key, Validate.Required<float>(p, "value")); break;
                default: PlayerPrefs.SetString(key, Validate.Required<string>(p, "value")); break;
            }
            PlayerPrefs.Save();
            return new { set = true, key, type };
        }

        private static object DeletePrefs(JToken p)
        {
            var key = p["key"]?.Value<string>();
            if (string.IsNullOrEmpty(key))
            {
                PlayerPrefs.DeleteAll();
                return new { deletedAll = true };
            }
            PlayerPrefs.DeleteKey(key);
            return new { deleted = true, key };
        }

        private static object GetEditorPrefs(JToken p)
        {
            var key = Validate.Required<string>(p, "key");
            var type = p["type"]?.Value<string>()?.ToLower() ?? "string";
            object value;
            switch (type)
            {
                case "int": value = EditorPrefs.GetInt(key); break;
                case "float": value = EditorPrefs.GetFloat(key); break;
                case "bool": value = EditorPrefs.GetBool(key); break;
                default: value = EditorPrefs.GetString(key); break;
            }
            return new { key, type, value };
        }

        private static object SetEditorPrefs(JToken p)
        {
            var key = Validate.Required<string>(p, "key");
            var type = p["type"]?.Value<string>()?.ToLower() ?? "string";
            switch (type)
            {
                case "int": EditorPrefs.SetInt(key, Validate.Required<int>(p, "value")); break;
                case "float": EditorPrefs.SetFloat(key, Validate.Required<float>(p, "value")); break;
                case "bool": EditorPrefs.SetBool(key, Validate.Required<bool>(p, "value")); break;
                default: EditorPrefs.SetString(key, Validate.Required<string>(p, "value")); break;
            }
            return new { set = true, key, type };
        }

        private static object DrawGizmo(JToken p)
        {
            // Note: Gizmos only render in Scene view during OnDrawGizmos
            // This logs the requested gizmo — for actual runtime, use Debug.DrawLine/DrawRay
            var type = Validate.Required<string>(p, "type").ToLower();
            var from = new Vector3(
                p["from"]?["x"]?.Value<float>() ?? 0,
                p["from"]?["y"]?.Value<float>() ?? 0,
                p["from"]?["z"]?.Value<float>() ?? 0
            );
            var to = new Vector3(
                p["to"]?["x"]?.Value<float>() ?? from.x + 1,
                p["to"]?["y"]?.Value<float>() ?? from.y,
                p["to"]?["z"]?.Value<float>() ?? from.z
            );
            var duration = p["duration"]?.Value<float>() ?? 5f;

            switch (type)
            {
                case "line":
                    Debug.DrawLine(from, to, Color.green, duration);
                    break;
                case "ray":
                    Debug.DrawRay(from, to - from, Color.red, duration);
                    break;
                default:
                    Debug.DrawLine(from, to, Color.yellow, duration);
                    break;
            }
            return new { drawn = true, type, duration };
        }

        private static object GetSystemInfo(JToken p)
        {
            return new
            {
                deviceName = SystemInfo.deviceName,
                operatingSystem = SystemInfo.operatingSystem,
                processorType = SystemInfo.processorType,
                processorCount = SystemInfo.processorCount,
                systemMemorySize = SystemInfo.systemMemorySize,
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                graphicsDeviceType = SystemInfo.graphicsDeviceType.ToString(),
                graphicsMemorySize = SystemInfo.graphicsMemorySize,
                graphicsShaderLevel = SystemInfo.graphicsShaderLevel,
                maxTextureSize = SystemInfo.maxTextureSize,
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
            };
        }

        // ── Log Capture ──
        private static bool _isCapturing;
        private static readonly List<CapturedLog> _capturedLogs = new();

        private struct CapturedLog
        {
            public string message;
            public string stackTrace;
            public string type;
            public string timestamp;
        }

        private static void OnLogReceived(string message, string stackTrace, LogType type)
        {
            _capturedLogs.Add(new CapturedLog
            {
                message = message,
                stackTrace = stackTrace,
                type = type.ToString(),
                timestamp = DateTime.Now.ToString("HH:mm:ss.fff"),
            });
        }

        private static object StartCapture(JToken p)
        {
            if (_isCapturing) return new { success = false, error = "Capture already in progress" };
            _capturedLogs.Clear();
            Application.logMessageReceived += OnLogReceived;
            _isCapturing = true;
            return new { success = true, message = "Log capture started" };
        }

        private static object StopCapture(JToken p)
        {
            if (!_isCapturing) return new { success = false, error = "No capture in progress" };
            Application.logMessageReceived -= OnLogReceived;
            _isCapturing = false;
            return new { success = true, logCount = _capturedLogs.Count, message = "Log capture stopped. Use debug.getCapturedLogs to retrieve." };
        }

        private static object GetCapturedLogs(JToken p)
        {
            string typeFilter = (string)p?["type"];
            bool clear = (bool?)p?["clear"] ?? false;
            int maxCount = (int?)p?["count"] ?? 200;

            IEnumerable<CapturedLog> logs = _capturedLogs;
            if (!string.IsNullOrEmpty(typeFilter))
                logs = logs.Where(l => l.type.Equals(typeFilter, StringComparison.OrdinalIgnoreCase));

            var result = logs.TakeLast(maxCount).Select(l => new
            {
                l.message,
                l.type,
                l.timestamp,
                stackTrace = (bool?)p?["includeStackTrace"] == true ? l.stackTrace : null,
            }).ToArray();

            if (clear) _capturedLogs.Clear();

            return new { isCapturing = _isCapturing, totalCaptured = _capturedLogs.Count, returned = result.Length, logs = result };
        }

        // ── Scripting Defines ──
        private static object GetDefines(JToken p)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedTarget, out string[] defines);
            return new { platform = buildTargetGroup.ToString(), defines };
        }

        private static object SetDefines(JToken p)
        {
            var action = Validate.Required<string>(p, "action");
            var symbol = Validate.Required<string>(p, "symbol");
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedTarget, out string[] currentDefines);
            var defineList = currentDefines.ToList();

            switch (action)
            {
                case "add":
                    if (!defineList.Contains(symbol)) defineList.Add(symbol);
                    break;
                case "remove":
                    defineList.Remove(symbol);
                    break;
                default:
                    throw new McpException(-32602, $"Unknown action: {action}. Valid: add, remove");
            }

            PlayerSettings.SetScriptingDefineSymbols(namedTarget, defineList.ToArray());
            return new { success = true, action, symbol, defines = defineList.ToArray() };
        }

        private static object ForceRecompile(JToken p)
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            return new { success = true, message = "Script recompilation requested" };
        }
    }
}
