using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarnelLabs.MCP
{
    public static class ResourceHandler
    {
        public static void Register()
        {
            CommandRouter.Register("resource.projectInfo", GetProjectInfo);
            CommandRouter.Register("resource.currentScene", GetCurrentScene);
            CommandRouter.Register("resource.recentConsole", GetRecentConsole);
            CommandRouter.Register("resource.compileStatus", GetCompileStatus);
            CommandRouter.Register("resource.installedPackages", GetInstalledPackages);
        }

        private static object GetProjectInfo(JToken p)
        {
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            return new
            {
                unityVersion = Application.unityVersion, productName = Application.productName,
                companyName = Application.companyName,
                platform = EditorUserBuildSettings.activeBuildTarget.ToString(),
                renderPipeline = rp != null ? rp.GetType().Name : "Built-in",
                colorSpace = PlayerSettings.colorSpace.ToString(),
                scriptingBackend = PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)).ToString(),
                apiCompatibility = PlayerSettings.GetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)).ToString(),
            };
        }

        private static object GetCurrentScene(JToken p)
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            var hierarchy = roots.Select(r => SerializeLight(r.transform, 0, 2)).ToArray();
            return new { name = scene.name, path = scene.path, isDirty = scene.isDirty, rootCount = roots.Length, hierarchy };
        }

        private static object SerializeLight(Transform t, int depth, int maxDepth)
        {
            var children = new List<object>();
            if (depth < maxDepth)
                for (int i = 0; i < t.childCount; i++) children.Add(SerializeLight(t.GetChild(i), depth + 1, maxDepth));
            return new
            {
                name = t.name, active = t.gameObject.activeSelf, childCount = t.childCount,
                components = t.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToArray(),
                children = children.Count > 0 ? children : null,
            };
        }

        private static object GetRecentConsole(JToken p)
        {
            try
            {
                var logEntriesType = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
                var countMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
                var startMethod = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public);
                var endMethod = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public);
                var getEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
                int totalCount = (int)countMethod.Invoke(null, null);
                startMethod.Invoke(null, null);
                var logEntryType = typeof(Editor).Assembly.GetType("UnityEditor.LogEntry");
                var logEntry = Activator.CreateInstance(logEntryType);
                var messageField = logEntryType.GetField("message", BindingFlags.Public | BindingFlags.Instance);
                var modeField = logEntryType.GetField("mode", BindingFlags.Public | BindingFlags.Instance);
                var entries = new List<object>();
                int start = Math.Max(0, totalCount - 50);
                for (int i = start; i < totalCount; i++)
                {
                    getEntry.Invoke(null, new object[] { i, logEntry });
                    string message = (string)messageField.GetValue(logEntry);
                    int modeValue = (int)modeField.GetValue(logEntry);
                    string logType = (modeValue & 1) != 0 ? "error" : (modeValue & 2) != 0 ? "warning" : "log";
                    entries.Add(new { message = message.Length > 300 ? message.Substring(0, 300) + "..." : message, type = logType });
                }
                endMethod.Invoke(null, null);
                return new { count = entries.Count, entries };
            }
            catch (Exception ex)
            {
                return new { count = 0, error = $"Failed to read console: {ex.Message}", entries = Array.Empty<object>() };
            }
        }

        private static object GetCompileStatus(JToken p)
        {
            bool hasErrors = EditorUtility.scriptCompilationFailed;
            bool isCompiling = EditorApplication.isCompiling;
            var assemblies = CompilationPipeline.GetAssemblies();
            return new
            {
                status = hasErrors ? "error" : (isCompiling ? "compiling" : "ok"),
                hasErrors, isCompiling, assemblyCount = assemblies.Length,
            };
        }

        private static object _cachedPackages;
        private static double _packagesCacheTime;
        private static UnityEditor.PackageManager.Requests.ListRequest _pendingListRequest;

        private static object GetInstalledPackages(JToken p)
        {
            if (_cachedPackages != null && EditorApplication.timeSinceStartup - _packagesCacheTime < 60)
                return _cachedPackages;

            if (_pendingListRequest != null)
            {
                if (!_pendingListRequest.IsCompleted)
                    return new { status = "loading", message = "Package list is being fetched. Try again shortly.", packages = Array.Empty<object>() };

                if (_pendingListRequest.Status == StatusCode.Success)
                {
                    var packages = _pendingListRequest.Result.Select(pkg => new
                    {
                        name = pkg.name, version = pkg.version, displayName = pkg.displayName, source = pkg.source.ToString(),
                    }).ToArray();
                    _cachedPackages = new { count = packages.Length, packages };
                    _packagesCacheTime = EditorApplication.timeSinceStartup;
                }
                else
                {
                    _cachedPackages = new { error = _pendingListRequest.Error?.message ?? "Failed to list packages", count = 0, packages = Array.Empty<object>() };
                }
                _pendingListRequest = null;
                return _cachedPackages;
            }

            _pendingListRequest = Client.List(true);
            if (_cachedPackages != null) return _cachedPackages;
            return new { status = "loading", message = "Package list is being fetched for the first time. Try again shortly.", count = 0, packages = Array.Empty<object>() };
        }
    }
}
