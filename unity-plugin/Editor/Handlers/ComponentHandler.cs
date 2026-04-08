using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class ComponentHandler
    {
        public static void Register()
        {
            CommandRouter.Register("component.list", ListComponents);
            CommandRouter.Register("component.get", GetComponent);
            CommandRouter.Register("component.remove", RemoveComponent);
            CommandRouter.Register("component.enable", EnableComponent);
            CommandRouter.Register("component.copy", CopyComponent);
            CommandRouter.Register("component.paste", PasteComponent);
            CommandRouter.Register("component.getAll", GetAllProperties);
            CommandRouter.Register("component.move", MoveComponent);
            CommandRouter.Register("component.enableBatch", EnableBatch);
            CommandRouter.Register("component.removeBatch", RemoveBatch);
        }

        private static Component FindComponent(GameObject go, string typeName, int index = 0)
        {
            var components = go.GetComponents<Component>().Where(c => c != null && c.GetType().Name == typeName).ToArray();
            if (components.Length == 0) throw new McpException(-32602, $"No '{typeName}' on '{go.name}'");
            if (index >= components.Length) throw new McpException(-32602, $"Component index {index} out of range (0-{components.Length - 1})");
            return components[index];
        }

        private static object ComponentInfo(Component comp)
        {
            if (comp == null) return null;
            var behaviour = comp as Behaviour;
            return new
            {
                type = comp.GetType().Name,
                fullType = comp.GetType().FullName,
                enabled = behaviour?.enabled,
                gameObject = comp.gameObject.name,
                instanceId = comp.GetInstanceID(),
            };
        }

        private static object ListComponents(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var includeInherited = p["includeInherited"]?.Value<bool>() ?? false;

            var components = go.GetComponents<Component>()
                .Where(c => c != null)
                .Select(c =>
                {
                    var b = c as Behaviour;
                    return new
                    {
                        type = c.GetType().Name,
                        fullType = c.GetType().FullName,
                        enabled = b?.enabled,
                        baseTypes = includeInherited ? GetBaseTypes(c.GetType()) : null,
                    };
                }).ToArray();

            return new { gameObject = go.name, count = components.Length, components };
        }

        private static string[] GetBaseTypes(Type t)
        {
            var types = new List<string>();
            var current = t.BaseType;
            while (current != null && current != typeof(object))
            {
                types.Add(current.Name);
                current = current.BaseType;
            }
            return types.ToArray();
        }

        private static object GetComponent(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var typeName = Validate.Required<string>(p, "componentType");
            var index = p["index"]?.Value<int>() ?? 0;
            var comp = FindComponent(go, typeName, index);
            return ComponentInfo(comp);
        }

        private static object RemoveComponent(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var typeName = Validate.Required<string>(p, "componentType");
            var index = p["index"]?.Value<int>() ?? 0;
            var comp = FindComponent(go, typeName, index);

            WorkflowManager.SnapshotObject(go);
            Undo.DestroyObjectImmediate(comp);
            return new { removed = true, gameObject = go.name, componentType = typeName };
        }

        private static object EnableComponent(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var typeName = Validate.Required<string>(p, "componentType");
            var enabled = p["enabled"]?.Value<bool>() ?? true;
            var index = p["index"]?.Value<int>() ?? 0;
            var comp = FindComponent(go, typeName, index);

            if (comp is Behaviour behaviour)
            {
                WorkflowManager.SnapshotObject(go);
                Undo.RecordObject(behaviour, "MCP: Enable Component");
                behaviour.enabled = enabled;
                return new { gameObject = go.name, componentType = typeName, enabled };
            }
            throw new McpException(-32602, $"'{typeName}' is not a Behaviour and cannot be enabled/disabled");
        }

        private static object CopyComponent(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var typeName = Validate.Required<string>(p, "componentType");
            var index = p["index"]?.Value<int>() ?? 0;
            var comp = FindComponent(go, typeName, index);

            UnityEditorInternal.ComponentUtility.CopyComponent(comp);
            return new { copied = true, gameObject = go.name, componentType = typeName };
        }

        private static object PasteComponent(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var asNew = p["asNew"]?.Value<bool>() ?? false;

            WorkflowManager.SnapshotObject(go);
            bool success;
            if (asNew)
                success = UnityEditorInternal.ComponentUtility.PasteComponentAsNew(go);
            else
                success = UnityEditorInternal.ComponentUtility.PasteComponentValues(go.GetComponents<Component>().Last());

            return new { pasted = success, gameObject = go.name, asNew };
        }

        private static object GetAllProperties(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var typeName = Validate.Required<string>(p, "componentType");
            var index = p["index"]?.Value<int>() ?? 0;
            var comp = FindComponent(go, typeName, index);

            var so = new SerializedObject(comp);
            var props = new List<object>();
            var iter = so.GetIterator();
            iter.Next(true);
            do
            {
                props.Add(new
                {
                    name = iter.name,
                    displayName = iter.displayName,
                    type = iter.propertyType.ToString(),
                    depth = iter.depth,
                    editable = iter.editable,
                });
            } while (iter.Next(iter.depth < 2));

            return new { gameObject = go.name, componentType = typeName, properties = props };
        }

        private static object MoveComponent(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var typeName = Validate.Required<string>(p, "componentType");
            var direction = p["direction"]?.Value<string>() ?? "up";
            var index = p["index"]?.Value<int>() ?? 0;
            var comp = FindComponent(go, typeName, index);

            WorkflowManager.SnapshotObject(go);
            bool success;
            if (direction.ToLower() == "up")
                success = UnityEditorInternal.ComponentUtility.MoveComponentUp(comp);
            else
                success = UnityEditorInternal.ComponentUtility.MoveComponentDown(comp);

            return new { moved = success, gameObject = go.name, componentType = typeName, direction };
        }

        private static object EnableBatch(JToken p)
        {
            var items = Validate.Required<JArray>(p, "items");
            return BatchExecutor.Execute(items, item => EnableComponent(item));
        }

        private static object RemoveBatch(JToken p)
        {
            var items = Validate.Required<JArray>(p, "items");
            return BatchExecutor.Execute(items, item => RemoveComponent(item));
        }
    }
}
