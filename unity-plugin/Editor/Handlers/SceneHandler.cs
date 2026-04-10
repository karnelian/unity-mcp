using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarnelLabs.MCP
{
    public static class SceneHandler
    {
        public static void Register()
        {
            CommandRouter.Register("scene.hierarchy", GetHierarchy);
            CommandRouter.Register("scene.create", CreateGameObject);
            CommandRouter.Register("scene.setTransform", SetTransform);
            CommandRouter.Register("scene.addComponent", AddComponent);
            CommandRouter.Register("scene.setComponent", SetComponent);
            CommandRouter.Register("scene.delete", DeleteGameObject);
            CommandRouter.Register("scene.duplicate", DuplicateGameObject);
            CommandRouter.Register("scene.manage", ManageScene);
            CommandRouter.Register("scene.find", FindObjects);
            CommandRouter.Register("scene.select", SelectObject);
            // ── 배치 도구 ──
            CommandRouter.Register("scene.createBatch", CreateBatch);
            CommandRouter.Register("scene.setTransformBatch", SetTransformBatch);
            CommandRouter.Register("scene.deleteBatch", DeleteBatch);
            // ── 씬 확장 ──
            CommandRouter.Register("scene.listLoaded", ListLoadedScenes);
            CommandRouter.Register("scene.setActiveScene", SetActiveScene);
            CommandRouter.Register("scene.moveToScene", MoveToScene);
            CommandRouter.Register("scene.openAdditive", OpenAdditive);
            CommandRouter.Register("scene.close", CloseScene);
            CommandRouter.Register("scene.saveAs", SaveSceneAs);
            // ── 에디터 확장 ──
            CommandRouter.Register("scene.undo", PerformUndo);
            CommandRouter.Register("scene.redo", PerformRedo);
            CommandRouter.Register("scene.setSelection", SetSelection);
            CommandRouter.Register("scene.getSelection", GetSelection);
            CommandRouter.Register("scene.setParent", SetParent);
            CommandRouter.Register("scene.getContext", GetEditorContext);
        }

        // ── 다중 식별자 헬퍼 ──────────────────────────────────────

        private static GameObject FindGo(JToken p)
        {
            return GameObjectFinder.FindOrThrow(
                instanceId: (int?)p?["instanceId"],
                path: (string)p?["path"],
                name: (string)p?["name"],
                tag: (string)p?["tag"],
                componentType: (string)p?["componentType"]
            );
        }

        // ── Hierarchy 직렬화 ──────────────────────────────────────

        private const int HardDepthCap = 50;

        private static object SerializeTransform(Transform t, int depth, int maxDepth, bool includeComponents, string nameFilter)
        {
            if (depth > HardDepthCap) return null;
            if (maxDepth >= 0 && depth > maxDepth) return null;
            bool nameMatch = string.IsNullOrEmpty(nameFilter) || t.name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            var children = new List<object>();
            for (int i = 0; i < t.childCount; i++)
            {
                var child = SerializeTransform(t.GetChild(i), depth + 1, maxDepth, includeComponents, nameFilter);
                if (child != null) children.Add(child);
            }
            if (!nameMatch && children.Count == 0) return null;
            return new
            {
                name = t.name,
                instanceId = t.gameObject.GetInstanceID(),
                path = GameObjectFinder.GetFullPath(t),
                active = t.gameObject.activeSelf,
                tag = t.tag,
                layer = LayerMask.LayerToName(t.gameObject.layer),
                position = new { x = t.position.x, y = t.position.y, z = t.position.z },
                rotation = new { x = t.eulerAngles.x, y = t.eulerAngles.y, z = t.eulerAngles.z },
                scale = new { x = t.localScale.x, y = t.localScale.y, z = t.localScale.z },
                childCount = t.childCount,
                components = includeComponents ? t.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name).ToArray() : null,
                children = children.Count > 0 ? children : null,
            };
        }

        // ── 핸들러 구현 ──────────────────────────────────────────

        private static object GetHierarchy(JToken p)
        {
            string path = (string)p?["path"];
            int depth = (int?)p?["depth"] ?? -1;
            bool includeComponents = (bool?)p?["includeComponents"] ?? false;
            string nameFilter = (string)p?["nameFilter"];
            var scene = SceneManager.GetActiveScene();
            Transform[] roots;
            if (!string.IsNullOrEmpty(path))
            {
                var go = GameObjectFinder.FindOrThrow(path: path);
                roots = new[] { go.transform };
            }
            else
                roots = scene.GetRootGameObjects().Select(g => g.transform).ToArray();
            return new
            {
                sceneName = scene.name, scenePath = scene.path,
                rootCount = scene.GetRootGameObjects().Length,
                objects = roots.Select(r => SerializeTransform(r, 0, depth, includeComponents, nameFilter)).Where(o => o != null).ToArray(),
            };
        }

        private static object CreateGameObject(JToken p)
        {
            string goName = Validate.Required((string)p?["name"], "name");
            string type = (string)p?["type"] ?? "empty";
            string parent = (string)p?["parent"];
            GameObject go;
            switch (type)
            {
                case "primitive":
                    var primitiveType = Validate.ParseEnum<PrimitiveType>((string)p["primitiveType"], "primitiveType");
                    go = GameObject.CreatePrimitive(primitiveType); go.name = goName; break;
                case "prefab":
                    string prefabPath = Validate.SafeAssetPath((string)p["prefabPath"], "prefabPath");
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab == null) throw new McpException(-32003, $"Prefab not found: {prefabPath}");
                    go = (GameObject)PrefabUtility.InstantiatePrefab(prefab); go.name = goName; break;
                default: go = new GameObject(goName); break;
            }
            Undo.RegisterCreatedObjectUndo(go, $"Create {goName}");
            WorkflowManager.SnapshotObject(go, $"scene.create({goName})");

            if (!string.IsNullOrEmpty(parent))
            {
                var parentGo = GameObjectFinder.FindOrThrow(path: parent, name: parent);
                go.transform.SetParent(parentGo.transform, false);
            }
            var pos = p?["position"];
            if (pos != null) go.transform.position = new Vector3((float)pos["x"], (float)pos["y"], (float)pos["z"]);
            var rot = p?["rotation"];
            if (rot != null) go.transform.eulerAngles = new Vector3((float)rot["x"], (float)rot["y"], (float)rot["z"]);
            var scl = p?["scale"];
            if (scl != null) go.transform.localScale = new Vector3((float)scl["x"], (float)scl["y"], (float)scl["z"]);

            return GameObjectFinder.ToRichInfo(go);
        }

        private static object SetTransform(JToken p)
        {
            var go = FindGo(p);
            string space = (string)p?["space"] ?? "world";
            Undo.RecordObject(go.transform, "Set Transform");
            WorkflowManager.SnapshotObject(go, "scene.setTransform");

            var pos = p?["position"];
            if (pos != null) { var v = new Vector3((float)pos["x"], (float)pos["y"], (float)pos["z"]); if (space == "local") go.transform.localPosition = v; else go.transform.position = v; }
            var rot = p?["rotation"];
            if (rot != null) { var v = new Vector3((float)rot["x"], (float)rot["y"], (float)rot["z"]); if (space == "local") go.transform.localEulerAngles = v; else go.transform.eulerAngles = v; }
            var scl = p?["scale"];
            if (scl != null) go.transform.localScale = new Vector3((float)scl["x"], (float)scl["y"], (float)scl["z"]);
            EditorUtility.SetDirty(go);
            return GameObjectFinder.ToRichInfo(go);
        }

        private static object AddComponent(JToken p)
        {
            var go = FindGo(p);
            string componentType = Validate.Required((string)p?["componentType"], "componentType");
            var type = TypeCache.GetTypesDerivedFrom<Component>().FirstOrDefault(t => t.Name == componentType || t.FullName == componentType);
            if (type == null) throw new McpException(-32602, $"Component type not found: {componentType}");
            WorkflowManager.SnapshotObject(go, $"scene.addComponent({componentType})");
            Undo.AddComponent(go, type);
            return GameObjectFinder.ToRichInfo(go);
        }

        private static object SetComponent(JToken p)
        {
            var go = FindGo(p);
            string componentType = Validate.Required((string)p?["componentType"], "componentType");
            var comp = go.GetComponents<Component>().FirstOrDefault(c => c != null && (c.GetType().Name == componentType || c.GetType().FullName == componentType));
            if (comp == null) throw new McpException(-32602, $"Component not found: {componentType} on {go.name}");
            var so = new SerializedObject(comp);
            var props = p["properties"] as JObject;
            var applied = new List<string>();
            var skipped = new List<string>();
            if (props != null)
            {
                WorkflowManager.SnapshotObject(go, $"scene.setComponent({componentType})");
                foreach (var prop in props.Properties())
                {
                    var sp = so.FindProperty(prop.Name);
                    if (sp == null) { skipped.Add(prop.Name); continue; }
                    switch (sp.propertyType)
                    {
                        case SerializedPropertyType.Float: sp.floatValue = (float)prop.Value; applied.Add(prop.Name); break;
                        case SerializedPropertyType.Integer: sp.intValue = (int)prop.Value; applied.Add(prop.Name); break;
                        case SerializedPropertyType.Boolean: sp.boolValue = (bool)prop.Value; applied.Add(prop.Name); break;
                        case SerializedPropertyType.String: sp.stringValue = (string)prop.Value; applied.Add(prop.Name); break;
                        case SerializedPropertyType.Vector3:
                            sp.vector3Value = new Vector3((float)prop.Value["x"], (float)prop.Value["y"], (float)prop.Value["z"]);
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.Color:
                            sp.colorValue = new Color((float)prop.Value["r"], (float)prop.Value["g"], (float)prop.Value["b"], (float?)prop.Value["a"] ?? 1f);
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.Enum:
                            if (prop.Value.Type == JTokenType.Integer)
                                sp.enumValueIndex = (int)prop.Value;
                            else if (prop.Value.Type == JTokenType.String)
                            {
                                var names = sp.enumDisplayNames;
                                var idx = System.Array.IndexOf(names, (string)prop.Value);
                                if (idx >= 0) sp.enumValueIndex = idx;
                                else sp.intValue = 0;
                            }
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.Vector2:
                            sp.vector2Value = new Vector2((float)prop.Value["x"], (float)prop.Value["y"]);
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.Vector4:
                            sp.vector4Value = new Vector4((float)prop.Value["x"], (float)prop.Value["y"], (float)prop.Value["z"], (float)prop.Value["w"]);
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.Quaternion:
                            sp.quaternionValue = Quaternion.Euler((float)prop.Value["x"], (float)prop.Value["y"], (float)prop.Value["z"]);
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.Rect:
                            sp.rectValue = new Rect((float)prop.Value["x"], (float)prop.Value["y"], (float)prop.Value["width"], (float)prop.Value["height"]);
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.Vector2Int:
                            sp.vector2IntValue = new Vector2Int((int)prop.Value["x"], (int)prop.Value["y"]);
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.Vector3Int:
                            sp.vector3IntValue = new Vector3Int((int)prop.Value["x"], (int)prop.Value["y"], (int)prop.Value["z"]);
                            applied.Add(prop.Name); break;
                        case SerializedPropertyType.ObjectReference:
                            if (prop.Value.Type == JTokenType.String)
                            {
                                var refPath = (string)prop.Value;
                                UnityEngine.Object resolved = null;

                                // Parse "path:ComponentType" format (e.g. "toggle_BGM:Toggle", "btn_Close:Button")
                                string objPath = refPath;
                                string compType = null;
                                var colonIdx = refPath.LastIndexOf(':');
                                if (colonIdx > 0 && !refPath.StartsWith("Assets/"))
                                {
                                    objPath = refPath.Substring(0, colonIdx);
                                    compType = refPath.Substring(colonIdx + 1);
                                }

                                // 1) Asset path (starts with "Assets/")
                                if (refPath.StartsWith("Assets/"))
                                    resolved = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(refPath);

                                // 2) Child of target GO (e.g. "toggle_BGM" or "toggle_BGM:Toggle")
                                if (resolved == null && go != null)
                                {
                                    var child = go.transform.Find(objPath);
                                    if (child != null)
                                        resolved = ResolveComponent(child.gameObject, compType);
                                }

                                // 3) Scene object path (e.g. "Canvas/Panel_MainMenu")
                                if (resolved == null)
                                {
                                    var sceneGo = GameObject.Find(objPath);
                                    if (sceneGo != null)
                                        resolved = ResolveComponent(sceneGo, compType);
                                }

                                if (resolved != null) { sp.objectReferenceValue = resolved; applied.Add(prop.Name); }
                                else skipped.Add($"{prop.Name}(not found:{refPath})");
                            }
                            else if (prop.Value.Type == JTokenType.Null)
                            {
                                sp.objectReferenceValue = null; applied.Add(prop.Name);
                            }
                            else skipped.Add($"{prop.Name}(ObjectReference expects string path or null)");
                            break;
                        default: skipped.Add($"{prop.Name}(unsupported:{sp.propertyType})"); break;
                    }
                }
                so.ApplyModifiedProperties();
            }
            var info = GameObjectFinder.ToRichInfo(go);
            return new { info, applied = applied.Count > 0 ? applied : null, skipped = skipped.Count > 0 ? skipped : null };
        }

        private static UnityEngine.Object ResolveComponent(GameObject go, string componentTypeName)
        {
            if (string.IsNullOrEmpty(componentTypeName))
                return go;
            foreach (var c in go.GetComponents<Component>())
            {
                if (c != null && (c.GetType().Name == componentTypeName || c.GetType().FullName == componentTypeName))
                    return c;
            }
            // If component not found, return the GameObject itself as fallback
            return go;
        }

        private static object DeleteGameObject(JToken p)
        {
            var go = FindGo(p);
            var deletedPath = GameObjectFinder.GetFullPath(go.transform);
            var deletedId = go.GetInstanceID();
            WorkflowManager.SnapshotObject(go, $"scene.delete({deletedPath})");
            Undo.DestroyObjectImmediate(go);
            return new { success = true, deleted = deletedPath, instanceId = deletedId };
        }

        private static object DuplicateGameObject(JToken p)
        {
            var go = FindGo(p);
            int count = (int?)p?["count"] ?? 1;
            Validate.InRange(count, 1, 100, "count");
            Vector3 offset = Vector3.zero;
            var off = p?["offset"];
            if (off != null) offset = new Vector3((float)off["x"], (float)off["y"], (float)off["z"]);
            var created = new List<object>();
            for (int i = 0; i < count; i++)
            {
                var clone = UnityEngine.Object.Instantiate(go, go.transform.parent);
                clone.name = $"{go.name} ({i + 1})";
                clone.transform.position = go.transform.position + offset * (i + 1);
                Undo.RegisterCreatedObjectUndo(clone, $"Duplicate {go.name}");
                WorkflowManager.SnapshotObject(clone, "scene.duplicate");
                created.Add(GameObjectFinder.ToRichInfo(clone));
            }
            return new { success = true, source = GameObjectFinder.GetFullPath(go.transform), count, created };
        }

        private static object ManageScene(JToken p)
        {
            string action = Validate.Required((string)p?["action"], "action");
            switch (action)
            {
                case "save": EditorSceneManager.SaveOpenScenes(); return new { success = true, action = "save" };
                case "open":
                    string op = Validate.Required((string)p?["scenePath"], "scenePath");
                    EditorSceneManager.OpenScene(op);
                    return new { success = true, action = "open", scenePath = op };
                case "create":
                    string cp = Validate.Required((string)p?["scenePath"], "scenePath");
                    var ns = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(ns, cp);
                    return new { success = true, action = "create", scenePath = cp };
                case "info":
                    var s = SceneManager.GetActiveScene();
                    return new { name = s.name, path = s.path, isDirty = s.isDirty, rootCount = s.rootCount, isLoaded = s.isLoaded };
                default:
                    throw new McpException(-32602, $"Unknown scene action: {action}. Valid: save, open, create, info");
            }
        }

        private static object FindObjects(JToken p)
        {
            string name = (string)p?["name"]; string tag = (string)p?["tag"]; string layer = (string)p?["layer"]; string componentType = (string)p?["componentType"];
            var all = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            IEnumerable<GameObject> results = all;
            if (!string.IsNullOrEmpty(name)) results = results.Where(g => g.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(tag)) results = results.Where(g => g.CompareTag(tag));
            if (!string.IsNullOrEmpty(layer)) { int li = LayerMask.NameToLayer(layer); results = results.Where(g => g.layer == li); }
            if (!string.IsNullOrEmpty(componentType))
            {
                var t = TypeCache.GetTypesDerivedFrom<Component>().FirstOrDefault(t => t.Name == componentType);
                if (t != null) results = results.Where(g => g.GetComponent(t) != null);
            }
            var list = results.Take(100).Select(g => GameObjectFinder.ToRichInfo(g)).ToArray();
            return new { count = list.Length, objects = list };
        }

        private static object SelectObject(JToken p)
        {
            var go = FindGo(p);
            bool ping = (bool?)p?["ping"] ?? true;
            bool frame = (bool?)p?["frame"] ?? true;
            Selection.activeGameObject = go;
            if (ping) EditorGUIUtility.PingObject(go);
            if (frame && SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();
            return GameObjectFinder.ToRichInfo(go);
        }

        // ── 배치 도구 ──────────────────────────────────────────────

        private static object CreateBatch(JToken p)
        {
            var items = p?["items"] as JArray;
            return BatchExecutor.Execute(items, (item, idx) => CreateGameObject(item));
        }

        private static object SetTransformBatch(JToken p)
        {
            var items = p?["items"] as JArray;
            return BatchExecutor.Execute(items, (item, idx) => SetTransform(item));
        }

        private static object DeleteBatch(JToken p)
        {
            var items = p?["items"] as JArray;
            return BatchExecutor.Execute(items, (item, idx) => DeleteGameObject(item));
        }

        // ── 씬 확장 ──────────────────────────────────────────────────

        private static object ListLoadedScenes(JToken p)
        {
            var scenes = new List<object>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                scenes.Add(new
                {
                    name = scene.name,
                    path = scene.path,
                    buildIndex = scene.buildIndex,
                    isLoaded = scene.isLoaded,
                    isDirty = scene.isDirty,
                    rootCount = scene.rootCount,
                    isActive = scene == SceneManager.GetActiveScene(),
                });
            }
            return new { sceneCount = scenes.Count, scenes };
        }

        private static object SetActiveScene(JToken p)
        {
            var sceneName = p["sceneName"]?.Value<string>();
            var scenePath = p["scenePath"]?.Value<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if ((!string.IsNullOrEmpty(sceneName) && scene.name == sceneName) ||
                    (!string.IsNullOrEmpty(scenePath) && scene.path == scenePath))
                {
                    SceneManager.SetActiveScene(scene);
                    return new { success = true, activeScene = scene.name };
                }
            }
            throw new McpException(-32003, $"Scene not found: {sceneName ?? scenePath}");
        }

        private static object MoveToScene(JToken p)
        {
            var go = FindGo(p);
            if (go.transform.parent != null)
                throw new McpException(-32602, $"'{go.name}' must be a root object to move between scenes");
            var sceneName = p["targetScene"]?.Value<string>();
            var scenePath = p["targetScenePath"]?.Value<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if ((!string.IsNullOrEmpty(sceneName) && scene.name == sceneName) ||
                    (!string.IsNullOrEmpty(scenePath) && scene.path == scenePath))
                {
                    Undo.RecordObject(go.transform, "Move to Scene");
                    SceneManager.MoveGameObjectToScene(go, scene);
                    return new { success = true, gameObject = go.name, scene = scene.name };
                }
            }
            throw new McpException(-32003, $"Target scene not found: {sceneName ?? scenePath}");
        }

        private static object OpenAdditive(JToken p)
        {
            var path = Validate.Required<string>(p, "scenePath");
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            return new { success = true, name = scene.name, path = scene.path };
        }

        private static object CloseScene(JToken p)
        {
            var sceneName = p["sceneName"]?.Value<string>();
            var scenePath = p["scenePath"]?.Value<string>();
            var removeScene = p["removeScene"]?.Value<bool>() ?? true;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if ((!string.IsNullOrEmpty(sceneName) && scene.name == sceneName) ||
                    (!string.IsNullOrEmpty(scenePath) && scene.path == scenePath))
                {
                    EditorSceneManager.CloseScene(scene, removeScene);
                    return new { success = true, closed = sceneName ?? scenePath };
                }
            }
            throw new McpException(-32003, $"Scene not found: {sceneName ?? scenePath}");
        }

        private static object SaveSceneAs(JToken p)
        {
            var newPath = Validate.SafeAssetPath(Validate.Required<string>(p, "newPath"), "newPath");
            var scene = SceneManager.GetActiveScene();
            var sceneName = p["sceneName"]?.Value<string>();
            if (!string.IsNullOrEmpty(sceneName))
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.name == sceneName) { scene = s; break; }
                }
            }
            EditorSceneManager.SaveScene(scene, newPath);
            return new { success = true, savedTo = newPath, scene = scene.name };
        }

        // ── 에디터 확장 메서드 ──

        private static object PerformUndo(JToken p)
        {
            var steps = p["steps"]?.Value<int>() ?? 1;
            for (int i = 0; i < steps; i++) Undo.PerformUndo();
            return new { success = true, undoSteps = steps };
        }

        private static object PerformRedo(JToken p)
        {
            var steps = p["steps"]?.Value<int>() ?? 1;
            for (int i = 0; i < steps; i++) Undo.PerformRedo();
            return new { success = true, redoSteps = steps };
        }

        private static object SetSelection(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            return new { success = true, selected = go.name, path = GameObjectFinder.GetPath(go) };
        }

        private static object GetSelection(JToken p)
        {
            var active = Selection.activeGameObject;
            var objects = Selection.gameObjects;
            return new
            {
                active = active != null ? new { name = active.name, path = GameObjectFinder.GetPath(active), instanceId = active.GetInstanceID() } : null,
                count = objects.Length,
                selected = objects.Select(o => new
                {
                    name = o.name,
                    path = GameObjectFinder.GetPath(o),
                    instanceId = o.GetInstanceID(),
                }).ToArray(),
            };
        }

        private static object SetParent(JToken p)
        {
            // Build child JToken for FindOrThrow
            var childToken = new JObject();
            if (p["childInstanceId"] != null) childToken["instanceId"] = p["childInstanceId"];
            else if (p["childPath"] != null) childToken["path"] = p["childPath"];
            else if (p["childName"] != null) childToken["name"] = p["childName"];
            else throw new McpException(-32602, "Provide childPath, childName, or childInstanceId");

            var child = GameObjectFinder.FindOrThrow(childToken);

            // Build parent JToken (null = unparent)
            GameObject parent = null;
            if (p["parentInstanceId"] != null || p["parentPath"] != null || p["parentName"] != null)
            {
                var parentToken = new JObject();
                if (p["parentInstanceId"] != null) parentToken["instanceId"] = p["parentInstanceId"];
                else if (p["parentPath"] != null) parentToken["path"] = p["parentPath"];
                else if (p["parentName"] != null) parentToken["name"] = p["parentName"];
                parent = GameObjectFinder.FindOrThrow(parentToken);
            }

            var worldPositionStays = p["worldPositionStays"]?.Value<bool>() ?? true;
            Undo.SetTransformParent(child.transform, parent?.transform, worldPositionStays, "MCP: Set Parent");

            return new
            {
                success = true,
                child = child.name,
                parent = parent?.name,
                childPath = GameObjectFinder.GetPath(child),
            };
        }

        private static object GetEditorContext(JToken p)
        {
            var scene = SceneManager.GetActiveScene();
            return new
            {
                activeScene = scene.name,
                scenePath = scene.path,
                isPlaying = EditorApplication.isPlaying,
                isPaused = EditorApplication.isPaused,
                isCompiling = EditorApplication.isCompiling,
                platform = EditorUserBuildSettings.activeBuildTarget.ToString(),
                unityVersion = Application.unityVersion,
                selection = Selection.activeGameObject?.name,
            };
        }
    }
}
