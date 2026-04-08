using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace KarnelLabs.MCP
{
    public static class EventHandler
    {
        public static void Register()
        {
            CommandRouter.Register("event.listEvents", ListEvents);
            CommandRouter.Register("event.getListeners", GetListeners);
            CommandRouter.Register("event.addListener", AddListener);
            CommandRouter.Register("event.removeListener", RemoveListener);
            CommandRouter.Register("event.setListenerState", SetListenerState);
        }

        private static (Component component, UnityEventBase unityEvent, string eventName) FindEvent(GameObject go, string componentType, string eventName)
        {
            var components = go.GetComponents<Component>().Where(c => c != null);
            if (!string.IsNullOrEmpty(componentType))
                components = components.Where(c => c.GetType().Name == componentType);

            foreach (var comp in components)
            {
                var type = comp.GetType();
                // Check fields
                var field = type.GetField(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null && typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                    return (comp, (UnityEventBase)field.GetValue(comp), eventName);

                // Check properties
                var prop = type.GetProperty(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null && typeof(UnityEventBase).IsAssignableFrom(prop.PropertyType))
                    return (comp, (UnityEventBase)prop.GetValue(comp), eventName);
            }

            throw new McpException(-32003, $"UnityEvent '{eventName}' not found on '{go.name}'" +
                (string.IsNullOrEmpty(componentType) ? "" : $" (component: {componentType})"));
        }

        private static List<object> CollectEvents(Component comp)
        {
            var events = new List<object>();
            var type = comp.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var field in type.GetFields(flags))
            {
                if (typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                {
                    var evt = (UnityEventBase)field.GetValue(comp);
                    int listenerCount = evt != null ? evt.GetPersistentEventCount() : 0;
                    events.Add(new
                    {
                        name = field.Name,
                        type = field.FieldType.Name,
                        listenerCount,
                        component = comp.GetType().Name,
                    });
                }
            }

            foreach (var prop in type.GetProperties(flags))
            {
                if (typeof(UnityEventBase).IsAssignableFrom(prop.PropertyType) && prop.CanRead)
                {
                    try
                    {
                        var evt = (UnityEventBase)prop.GetValue(comp);
                        int listenerCount = evt != null ? evt.GetPersistentEventCount() : 0;
                        events.Add(new
                        {
                            name = prop.Name,
                            type = prop.PropertyType.Name,
                            listenerCount,
                            component = comp.GetType().Name,
                        });
                    }
                    catch { }
                }
            }

            return events;
        }

        private static object ListEvents(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            string componentFilter = (string)p?["componentType"];

            var allEvents = new List<object>();
            var components = go.GetComponents<Component>().Where(c => c != null);
            if (!string.IsNullOrEmpty(componentFilter))
                components = components.Where(c => c.GetType().Name == componentFilter);

            foreach (var comp in components)
            {
                allEvents.AddRange(CollectEvents(comp));
            }

            return new { gameObject = go.name, count = allEvents.Count, events = allEvents.ToArray() };
        }

        private static object GetListeners(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            string eventName = Validate.Required<string>(p, "eventName");
            string componentType = (string)p?["componentType"];

            var (comp, evt, _) = FindEvent(go, componentType, eventName);
            int count = evt.GetPersistentEventCount();
            var listeners = new List<object>();

            for (int i = 0; i < count; i++)
            {
                var target = evt.GetPersistentTarget(i);
                listeners.Add(new
                {
                    index = i,
                    target = target != null ? target.name : "null",
                    targetType = target != null ? target.GetType().Name : "null",
                    methodName = evt.GetPersistentMethodName(i),
                    callState = evt.GetPersistentListenerState(i).ToString(),
                });
            }

            return new
            {
                gameObject = go.name,
                component = comp.GetType().Name,
                eventName,
                listenerCount = count,
                listeners = listeners.ToArray(),
            };
        }

        private static object AddListener(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            string eventName = Validate.Required<string>(p, "eventName");
            string componentType = (string)p?["componentType"];
            string targetPath = Validate.Required<string>(p, "targetPath");
            string methodName = Validate.Required<string>(p, "methodName");
            string argType = (string)p?["argumentType"] ?? "void";

            WorkflowManager.SnapshotObject(go);

            var (comp, evt, _) = FindEvent(go, componentType, eventName);

            // Find target object
            GameObject targetGo;
            if (targetPath == "self" || targetPath == go.name)
                targetGo = go;
            else
                targetGo = GameObject.Find(targetPath) ?? throw new McpException(-32003, $"Target not found: {targetPath}");

            // Find target component with the method
            UnityEngine.Object targetObj = null;
            foreach (var targetComp in targetGo.GetComponents<Component>())
            {
                if (targetComp == null) continue;
                var method = targetComp.GetType().GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (method != null)
                {
                    targetObj = targetComp;
                    break;
                }
            }

            if (targetObj == null)
                throw new McpException(-32003, $"Method '{methodName}' not found on any component of '{targetGo.name}'");

            var so = new SerializedObject(comp);
            var eventProp = so.FindProperty(eventName);
            if (eventProp == null)
                throw new McpException(-32003, $"Cannot serialize event '{eventName}' for editing");

            var callsProp = eventProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
            callsProp.arraySize++;
            var newEntry = callsProp.GetArrayElementAtIndex(callsProp.arraySize - 1);
            newEntry.FindPropertyRelative("m_Target").objectReferenceValue = targetObj;
            newEntry.FindPropertyRelative("m_MethodName").stringValue = methodName;
            newEntry.FindPropertyRelative("m_CallState").intValue = 2; // RuntimeOnly
            newEntry.FindPropertyRelative("m_Mode").intValue = argType switch
            {
                "void" => 1,    // EventDefined
                "int" => 4,     // Int
                "float" => 2,   // Float
                "string" => 5,  // String
                "bool" => 6,    // Bool
                "object" => 3,  // Object
                _ => 1,
            };

            // Set argument value if provided
            if (p?["argumentValue"] != null)
            {
                var argsProp = newEntry.FindPropertyRelative("m_Arguments");
                switch (argType)
                {
                    case "int": argsProp.FindPropertyRelative("m_IntArgument").intValue = p["argumentValue"].Value<int>(); break;
                    case "float": argsProp.FindPropertyRelative("m_FloatArgument").floatValue = p["argumentValue"].Value<float>(); break;
                    case "string": argsProp.FindPropertyRelative("m_StringArgument").stringValue = p["argumentValue"].Value<string>(); break;
                    case "bool": argsProp.FindPropertyRelative("m_BoolArgument").boolValue = p["argumentValue"].Value<bool>(); break;
                }
            }

            so.ApplyModifiedProperties();

            return new
            {
                success = true,
                gameObject = go.name,
                eventName,
                target = targetGo.name,
                methodName,
                listenerIndex = callsProp.arraySize - 1,
            };
        }

        private static object RemoveListener(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            string eventName = Validate.Required<string>(p, "eventName");
            string componentType = (string)p?["componentType"];
            int index = Validate.Required<int>(p, "index");

            WorkflowManager.SnapshotObject(go);

            var (comp, evt, _) = FindEvent(go, componentType, eventName);

            if (index < 0 || index >= evt.GetPersistentEventCount())
                throw new McpException(-32602, $"Listener index {index} out of range (0-{evt.GetPersistentEventCount() - 1})");

            var so = new SerializedObject(comp);
            var eventProp = so.FindProperty(eventName);
            var callsProp = eventProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
            callsProp.DeleteArrayElementAtIndex(index);
            so.ApplyModifiedProperties();

            return new { success = true, gameObject = go.name, eventName, removedIndex = index };
        }

        private static object SetListenerState(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            string eventName = Validate.Required<string>(p, "eventName");
            string componentType = (string)p?["componentType"];
            int index = Validate.Required<int>(p, "index");
            string state = Validate.Required<string>(p, "state");

            WorkflowManager.SnapshotObject(go);

            var (comp, evt, _) = FindEvent(go, componentType, eventName);

            if (index < 0 || index >= evt.GetPersistentEventCount())
                throw new McpException(-32602, $"Listener index {index} out of range (0-{evt.GetPersistentEventCount() - 1})");

            var callState = state switch
            {
                "Off" => UnityEventCallState.Off,
                "EditorAndRuntime" => UnityEventCallState.EditorAndRuntime,
                "RuntimeOnly" => UnityEventCallState.RuntimeOnly,
                _ => throw new McpException(-32602, $"Invalid state: {state}. Valid: Off, RuntimeOnly, EditorAndRuntime"),
            };

            var so = new SerializedObject(comp);
            var eventProp = so.FindProperty(eventName);
            var callsProp = eventProp.FindPropertyRelative("m_PersistentCalls.m_Calls");
            var entry = callsProp.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("m_CallState").intValue = (int)callState;
            so.ApplyModifiedProperties();

            return new { success = true, gameObject = go.name, eventName, index, state };
        }
    }
}
