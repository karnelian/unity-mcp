using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class BuildHandler
    {
        public static void Register()
        {
            CommandRouter.Register("editor.playMode", PlayMode);
            CommandRouter.Register("editor.build", Build);
            CommandRouter.Register("editor.buildSettings", BuildSettings);
            CommandRouter.Register("editor.executeMenu", ExecuteMenu);
            CommandRouter.Register("editor.runTests", RunTests);
            CommandRouter.Register("editor.testList", TestList);
            CommandRouter.Register("editor.testResults", TestResults);
            CommandRouter.Register("editor.status", ConnectionStatus);
            CommandRouter.Register("editor.console", GetConsole);
            CommandRouter.Register("editor.projectInfo", ProjectInfo);
            CommandRouter.Register("editor.getSelection", GetSelection);
            CommandRouter.Register("editor.getContext", GetContext);
        }

        private static object PlayMode(JToken p)
        {
            string action = Validate.Required((string)p?["action"], "action");
            switch (action)
            {
                case "play": EditorApplication.isPlaying = true; return new { success = true, action, isPlaying = true };
                case "stop": EditorApplication.isPlaying = false; return new { success = true, action, isPlaying = false };
                case "pause": EditorApplication.isPaused = !EditorApplication.isPaused; return new { success = true, action, isPaused = EditorApplication.isPaused };
                case "step": EditorApplication.Step(); return new { success = true, action };
                case "status": return new { isPlaying = EditorApplication.isPlaying, isPaused = EditorApplication.isPaused, isCompiling = EditorApplication.isCompiling };
                default: throw new McpException(-32602, $"Unknown play mode action: {action}. Valid: play, stop, pause, step, status");
            }
        }

        private static object Build(JToken p)
        {
            string target = Validate.Required((string)p?["target"], "target");
            var buildTarget = target switch
            {
                "Windows" => BuildTarget.StandaloneWindows64, "Android" => BuildTarget.Android,
                "iOS" => BuildTarget.iOS, "WebGL" => BuildTarget.WebGL,
                "macOS" => BuildTarget.StandaloneOSX, "Linux" => BuildTarget.StandaloneLinux64,
                _ => throw new McpException(-32602, $"Unknown build target: {target}. Valid: Windows, Android, iOS, WebGL, macOS, Linux")
            };

            string[] scenes;
            var scenesToken = p?["scenes"] as JArray;
            if (scenesToken != null)
                scenes = scenesToken.Select(s => (string)s).ToArray();
            else
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

            string outputPath = (string)p?["outputPath"] ?? $"Builds/{target}/{Application.productName}";

            var options = BuildOptions.None;
            var optionsToken = p?["options"] as JArray;
            if (optionsToken != null)
            {
                foreach (var opt in optionsToken)
                {
                    string optStr = (string)opt;
                    if (string.Equals(optStr, "development", StringComparison.OrdinalIgnoreCase)) options |= BuildOptions.Development;
                    else if (string.Equals(optStr, "autoRun", StringComparison.OrdinalIgnoreCase)) options |= BuildOptions.AutoRunPlayer;
                    else if (string.Equals(optStr, "deepProfiling", StringComparison.OrdinalIgnoreCase)) options |= BuildOptions.EnableDeepProfilingSupport;
                    else if (string.Equals(optStr, "allowDebugging", StringComparison.OrdinalIgnoreCase)) options |= BuildOptions.AllowDebugging;
                }
            }

            var report = BuildPipeline.BuildPlayer(scenes, outputPath, buildTarget, options);
            return new
            {
                success = report.summary.result == BuildResult.Succeeded,
                result = report.summary.result.ToString(), totalTime = report.summary.totalTime.TotalSeconds,
                totalSize = report.summary.totalSize, errors = report.summary.totalErrors,
                warnings = report.summary.totalWarnings, outputPath = report.summary.outputPath,
            };
        }

        private static object BuildSettings(JToken p)
        {
            string action = (string)p?["action"] ?? "get";
            if (action == "get")
            {
                var scenes = EditorBuildSettings.scenes.Select(s => new { path = s.path, enabled = s.enabled, guid = s.guid.ToString() }).ToArray();
                return new
                {
                    activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(), scenes,
                    productName = PlayerSettings.productName, companyName = PlayerSettings.companyName,
                    bundleVersion = PlayerSettings.bundleVersion,
                    scriptingBackend = PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)).ToString(),
                };
            }
            if (action == "set")
            {
                var s = p?["settings"];
                if (s?["productName"] != null) PlayerSettings.productName = (string)s["productName"];
                if (s?["companyName"] != null) PlayerSettings.companyName = (string)s["companyName"];
                if (s?["bundleVersion"] != null) PlayerSettings.bundleVersion = (string)s["bundleVersion"];
                return new { success = true, action = "set" };
            }
            throw new McpException(-32602, $"Unknown action: {action}. Use 'get' or 'set'.");
        }

        private static object ExecuteMenu(JToken p)
        {
            string menuPath = Validate.Required((string)p?["menuPath"], "menuPath");
            bool result = EditorApplication.ExecuteMenuItem(menuPath);
            if (!result) throw new McpException(-32003, $"Menu item not found or failed: {menuPath}");
            return new { success = true, menuPath };
        }

        private static object RunTests(JToken p)
        {
            string mode = (string)p?["mode"] ?? "EditMode";
            string filter = (string)p?["filter"];
            var testRunnerApi = Type.GetType("UnityEditor.TestTools.TestRunner.Api.TestRunnerApi, UnityEditor.TestRunner");
            if (testRunnerApi == null)
                return new { success = false, error = "Test Runner package not installed. Install 'com.unity.test-framework' via Package Manager." };
            try
            {
                var api = ScriptableObject.CreateInstance(testRunnerApi);
                var executeMethod = testRunnerApi.GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance);
                var executionSettingsType = Type.GetType("UnityEditor.TestTools.TestRunner.Api.ExecutionSettings, UnityEditor.TestRunner");
                var settings = Activator.CreateInstance(executionSettingsType);
                var filterType = Type.GetType("UnityEditor.TestTools.TestRunner.Api.Filter, UnityEditor.TestRunner");
                var filterObj = Activator.CreateInstance(filterType);
                filterType.GetField("testMode").SetValue(filterObj, mode == "PlayMode" ? 2 : 1);
                if (!string.IsNullOrEmpty(filter)) filterType.GetField("testNames").SetValue(filterObj, new[] { filter });
                var filtersArray = Array.CreateInstance(filterType, 1);
                filtersArray.SetValue(filterObj, 0);
                executionSettingsType.GetField("filters").SetValue(settings, filtersArray);
                executeMethod.Invoke(api, new[] { settings });
                return new { success = true, mode, filter = filter ?? "(all)", message = "Test execution started." };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to run tests: {ex.Message}" };
            }
        }

        private static object ConnectionStatus(JToken p)
        {
            return McpBridge.GetDiagnostics();
        }

        private static object GetConsole(JToken p)
        {
            string typeFilter = (string)p?["type"];
            if (typeFilter == "all") typeFilter = null;
            int maxCount = (int?)p?["count"] ?? 50;
            Validate.InRange(maxCount, 1, 500, "count");
            bool clear = (bool?)p?["clear"] ?? false;

            if (clear)
            {
                var logEntriesType = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
                logEntriesType?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
                return new { success = true, action = "clear" };
            }

            try
            {
                var logEntriesType = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
                var startMethod = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public);
                var endMethod = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public);
                var getEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
                var countMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
                int totalCount = (int)countMethod.Invoke(null, null);
                startMethod.Invoke(null, null);
                var logEntryType = typeof(Editor).Assembly.GetType("UnityEditor.LogEntry");
                var logEntry = Activator.CreateInstance(logEntryType);
                var messageField = logEntryType.GetField("message", BindingFlags.Public | BindingFlags.Instance);
                var modeField = logEntryType.GetField("mode", BindingFlags.Public | BindingFlags.Instance);
                var entries = new List<object>();
                int start = Math.Max(0, totalCount - maxCount);
                for (int i = start; i < totalCount; i++)
                {
                    getEntry.Invoke(null, new object[] { i, logEntry });
                    string message = (string)messageField.GetValue(logEntry);
                    int modeValue = (int)modeField.GetValue(logEntry);
                    string logType = (modeValue & 1) != 0 ? "error" : (modeValue & 2) != 0 ? "warning" : "log";
                    if (typeFilter != null && logType != typeFilter) continue;
                    entries.Add(new { message = message.Length > 500 ? message.Substring(0, 500) + "..." : message, type = logType });
                }
                endMethod.Invoke(null, null);
                return new { totalCount, filtered = entries.Count, entries };
            }
            catch (Exception ex)
            {
                return new { error = $"Failed to access console logs: {ex.Message}", totalCount = 0, entries = Array.Empty<object>() };
            }
        }

        private static object ProjectInfo(JToken p)
        {
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            return new
            {
                unityVersion = Application.unityVersion, productName = Application.productName,
                companyName = Application.companyName, dataPath = Application.dataPath,
                platform = Application.platform.ToString(), systemLanguage = Application.systemLanguage.ToString(),
                isPlaying = Application.isPlaying,
                renderPipeline = rp != null ? rp.GetType().Name : "Built-in",
                scriptingBackend = PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)).ToString(),
                apiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)).ToString(),
                colorSpace = PlayerSettings.colorSpace.ToString(),
            };
        }

        // ── Test expansion ──
        private static object TestList(JToken p)
        {
            string mode = (string)p?["mode"] ?? "EditMode";
            try
            {
                var testRunnerApiType = Type.GetType("UnityEditor.TestTools.TestRunner.Api.TestRunnerApi, UnityEditor.TestRunner");
                if (testRunnerApiType == null)
                    return new { success = false, error = "Test Runner package not installed." };

                // Use reflection to retrieve test tree
                var api = ScriptableObject.CreateInstance(testRunnerApiType);
                var retrieveMethod = testRunnerApiType.GetMethod("RetrieveTestList", BindingFlags.Public | BindingFlags.Instance);
                if (retrieveMethod == null)
                    return new { success = false, error = "RetrieveTestList not available in this Unity version." };

                int testMode = mode == "PlayMode" ? 2 : 1;
                var callbackType = Type.GetType("UnityEditor.TestTools.TestRunner.Api.ITestAdaptor, UnityEditor.TestRunner");

                // Fallback: use CompilationPipeline to find test assemblies
                var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies()
                    .Where(a => a.flags.HasFlag(UnityEditor.Compilation.AssemblyFlags.EditorAssembly)
                        && (a.name.Contains("Test") || a.name.Contains("test")))
                    .Select(a => new { name = a.name, sourceFiles = a.sourceFiles.Length })
                    .ToArray();

                return new { mode, testAssemblies = assemblies, message = "Use editor.runTests to execute tests." };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to list tests: {ex.Message}" };
            }
        }

        private static string _lastTestResults;
        private static object TestResults(JToken p)
        {
            // Check for test result XML files
            string resultPath = System.IO.Path.Combine(Application.dataPath, "..", "TestResults");
            if (!System.IO.Directory.Exists(resultPath))
                return new { success = false, error = "No test results found. Run tests first with editor.runTests." };

            var files = System.IO.Directory.GetFiles(resultPath, "*.xml")
                .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                .Take(5)
                .Select(f => new
                {
                    file = System.IO.Path.GetFileName(f),
                    lastModified = System.IO.File.GetLastWriteTime(f).ToString("yyyy-MM-dd HH:mm:ss"),
                    size = new System.IO.FileInfo(f).Length,
                })
                .ToArray();

            if (files.Length == 0)
                return new { success = false, error = "No test result XML files found." };

            // Read latest result summary
            string latestFile = System.IO.Directory.GetFiles(resultPath, "*.xml")
                .OrderByDescending(f => System.IO.File.GetLastWriteTime(f)).First();
            string content = System.IO.File.ReadAllText(latestFile);

            // Quick parse for summary attributes
            string passed = ExtractXmlAttr(content, "passed");
            string failed = ExtractXmlAttr(content, "failed");
            string total = ExtractXmlAttr(content, "total");
            string duration = ExtractXmlAttr(content, "duration");

            return new
            {
                success = true,
                latestResult = System.IO.Path.GetFileName(latestFile),
                summary = new { total, passed, failed, duration },
                resultFiles = files,
            };
        }

        private static string ExtractXmlAttr(string xml, string attr)
        {
            string search = $"{attr}=\"";
            int idx = xml.IndexOf(search);
            if (idx < 0) return "?";
            idx += search.Length;
            int end = xml.IndexOf('"', idx);
            return end > idx ? xml.Substring(idx, end - idx) : "?";
        }

        // ── Editor context ──
        private static object GetSelection(JToken p)
        {
            var gameObjects = Selection.gameObjects.Select(go => new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
                components = go.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToArray(),
            }).ToArray();

            var assets = Selection.assetGUIDs.Select(guid =>
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return new { path, type = AssetDatabase.GetMainAssetTypeAtPath(path)?.Name ?? "Unknown" };
            }).ToArray();

            return new
            {
                gameObjectCount = gameObjects.Length,
                assetCount = assets.Length,
                gameObjects,
                assets,
                activeGameObject = Selection.activeGameObject != null ? Selection.activeGameObject.name : null,
            };
        }

        private static object GetContext(JToken p)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var sceneView = SceneView.lastActiveSceneView;

            object sceneViewInfo = null;
            if (sceneView != null)
            {
                sceneViewInfo = new
                {
                    cameraPosition = new { x = sceneView.camera.transform.position.x, y = sceneView.camera.transform.position.y, z = sceneView.camera.transform.position.z },
                    pivot = new { x = sceneView.pivot.x, y = sceneView.pivot.y, z = sceneView.pivot.z },
                    size = sceneView.size,
                    is2D = sceneView.in2DMode,
                    orthographic = sceneView.orthographic,
                };
            }

            var selectedGOs = Selection.gameObjects.Select(go => new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
            }).ToArray();

            var selectedAssets = Selection.assetGUIDs.Select(guid =>
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                return new { path = assetPath, type = AssetDatabase.GetMainAssetTypeAtPath(assetPath)?.Name ?? "Unknown" };
            }).ToArray();

            var focusedWindow = EditorWindow.focusedWindow;

            return new
            {
                scene = new { name = scene.name, path = scene.path, isDirty = scene.isDirty },
                isPlaying = EditorApplication.isPlaying,
                isPaused = EditorApplication.isPaused,
                isCompiling = EditorApplication.isCompiling,
                focusedWindow = focusedWindow?.GetType().Name,
                selection = new { gameObjects = selectedGOs, assets = selectedAssets },
                sceneView = sceneViewInfo,
            };
        }
    }
}
