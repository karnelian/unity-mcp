using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class UnitySearchHandler
    {
        public static void Register()
        {
            CommandRouter.Register("unitySearch.assets", SearchAssets);
            CommandRouter.Register("unitySearch.scene", SearchScene);
            CommandRouter.Register("unitySearch.menu", SearchMenu);
        }

        private static object SearchAssets(JToken p)
        {
            string query = (string)p["query"];
            int maxResults = p["maxResults"]?.Value<int>() ?? 100;

            var guids = AssetDatabase.FindAssets(query);
            var results = guids.Take(maxResults).Select(g =>
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                return new
                {
                    path,
                    name = System.IO.Path.GetFileName(path),
                    type = type?.Name ?? "Unknown",
                    guid = g,
                };
            }).ToArray();

            return new { count = results.Length, totalFound = guids.Length, results };
        }

        private static object SearchScene(JToken p)
        {
            string query = (string)p["query"];
            int maxResults = p["maxResults"]?.Value<int>() ?? 100;

            // Search GameObjects in scene by name
            var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var filtered = allGOs.Where(go =>
            {
                if (go.name.Contains(query, System.StringComparison.OrdinalIgnoreCase)) return true;
                // Also check component types
                var components = go.GetComponents<Component>();
                return components.Any(c => c != null && c.GetType().Name.Contains(query, System.StringComparison.OrdinalIgnoreCase));
            }).Take(maxResults);

            var results = filtered.Select(go => new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
                instanceId = go.GetInstanceID(),
                components = go.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToArray(),
                active = go.activeInHierarchy,
            }).ToArray();

            return new { count = results.Length, results };
        }

        private static object SearchMenu(JToken p)
        {
            string query = (string)p?["query"];
            // Note: Unity doesn't have a direct API to enumerate all menu items.
            // We return commonly used ones and match against the query.
            var commonMenus = new[]
            {
                "File/New Scene", "File/Open Scene", "File/Save", "File/Build Settings...",
                "Edit/Undo", "Edit/Redo", "Edit/Play", "Edit/Pause", "Edit/Step",
                "Edit/Project Settings...", "Edit/Preferences...",
                "GameObject/Create Empty", "GameObject/3D Object/Cube", "GameObject/3D Object/Sphere",
                "GameObject/Light/Directional Light", "GameObject/Camera",
                "Component/Physics/Rigidbody", "Component/Physics/Box Collider",
                "Window/General/Inspector", "Window/General/Hierarchy", "Window/General/Project",
                "Assets/Create/Material", "Assets/Create/Shader/Standard Surface Shader",
                "Assets/Create/C# Script", "Assets/Create/Folder",
            };

            var filtered = string.IsNullOrEmpty(query)
                ? commonMenus
                : commonMenus.Where(m => m.Contains(query, System.StringComparison.OrdinalIgnoreCase)).ToArray();

            return new { count = filtered.Length, menuItems = filtered };
        }
    }
}
