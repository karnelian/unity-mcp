using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class CanvasGroupHandler
    {
        public static void Register()
        {
            CommandRouter.Register("canvasGroup.add", Add);
            CommandRouter.Register("canvasGroup.set", Set);
            CommandRouter.Register("canvasGroup.find", Find);
        }

        private static object Add(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add CanvasGroup");
            var cg = Undo.AddComponent<CanvasGroup>(go);

            if (p["alpha"] != null) cg.alpha = (float)p["alpha"];
            if (p["interactable"] != null) cg.interactable = (bool)p["interactable"];
            if (p["blocksRaycasts"] != null) cg.blocksRaycasts = (bool)p["blocksRaycasts"];
            if (p["ignoreParentGroups"] != null) cg.ignoreParentGroups = (bool)p["ignoreParentGroups"];

            return new { success = true, gameObject = go.name, component = "CanvasGroup" };
        }

        private static object Set(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) throw new McpException(-32000, $"No CanvasGroup on '{go.name}'");

            Undo.RecordObject(cg, "Set CanvasGroup");
            if (p["alpha"] != null) cg.alpha = (float)p["alpha"];
            if (p["interactable"] != null) cg.interactable = (bool)p["interactable"];
            if (p["blocksRaycasts"] != null) cg.blocksRaycasts = (bool)p["blocksRaycasts"];
            if (p["ignoreParentGroups"] != null) cg.ignoreParentGroups = (bool)p["ignoreParentGroups"];

            EditorUtility.SetDirty(cg);
            return new { success = true, gameObject = go.name };
        }

        private static object Find(JToken p)
        {
            var groups = Object.FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
            var result = groups.Select(cg => new
            {
                gameObject = cg.gameObject.name,
                path = GameObjectFinder.GetPath(cg.gameObject),
                alpha = cg.alpha,
                interactable = cg.interactable,
                blocksRaycasts = cg.blocksRaycasts,
            }).ToArray();

            return new { count = result.Length, canvasGroups = result };
        }
    }
}
