using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarnelLabs.MCP
{
    /// <summary>
    /// 다중 전략 GameObject 검색 + 요청 단위 씬 캐시.
    /// Unity-Skills의 GameObjectFinder 패턴을 MCP에 맞게 구현.
    /// 메모리 안전: 캐시는 명시적으로 무효화, Destroyed 객체 체크.
    /// </summary>
    public static class GameObjectFinder
    {
        // ── 씬 캐시 ──────────────────────────────────────────────────
        private static Dictionary<int, GameObject> _idCache;
        private static Dictionary<string, GameObject> _pathCache;
        private static Dictionary<string, List<GameObject>> _nameCache;
        private static bool _cacheValid;

        /// <summary>요청 처리 후 반드시 호출. 다음 요청 시 캐시 재빌드.</summary>
        public static void InvalidateCache()
        {
            _cacheValid = false;
            _idCache = null;
            _pathCache = null;
            _nameCache = null;
        }

        private static void EnsureCache()
        {
            if (_cacheValid) return;

            _idCache = new Dictionary<int, GameObject>();
            _pathCache = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            _nameCache = new Dictionary<string, List<GameObject>>(StringComparer.OrdinalIgnoreCase);

            // 스택 기반 순회 (재귀 스택 오버플로 방지)
            var stack = new Stack<Transform>();
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    stack.Push(root.transform);
            }

            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (t == null) continue;

                var go = t.gameObject;
                var path = GetFullPath(t);

                _idCache[go.GetInstanceID()] = go;
                _pathCache[path] = go;

                if (!_nameCache.TryGetValue(go.name, out var list))
                {
                    list = new List<GameObject>();
                    _nameCache[go.name] = list;
                }
                list.Add(go);

                for (int i = 0; i < t.childCount; i++)
                    stack.Push(t.GetChild(i));
            }

            _cacheValid = true;
        }

        // ── 공개 검색 API ──────────────────────────────────────────

        /// <summary>JToken 파라미터에서 식별자 추출 후 검색</summary>
        public static GameObject FindOrThrow(Newtonsoft.Json.Linq.JToken p)
        {
            return FindOrThrow(
                instanceId: (int?)p?["instanceId"],
                path: (string)p?["path"],
                name: (string)p?["name"],
                tag: (string)p?["tag"],
                componentType: (string)p?["componentType"]
            );
        }

        /// <summary>
        /// 다중 전략 검색. 우선순위: instanceId > path > name.
        /// 실패 시 McpException + 유사 이름 제안.
        /// </summary>
        public static GameObject FindOrThrow(int? instanceId = null, string path = null, string name = null, string tag = null, string componentType = null)
        {
            var (go, error) = FindOrError(instanceId, path, name, tag, componentType);
            if (go == null) throw new McpException(-32003, error);
            return go;
        }

        /// <summary>
        /// 다중 전략 검색. 에러 시 (null, errorMessage) 반환.
        /// </summary>
        public static (GameObject go, string error) FindOrError(int? instanceId = null, string path = null, string name = null, string tag = null, string componentType = null)
        {
            EnsureCache();

            // 1. InstanceId (가장 정확)
            if (instanceId.HasValue)
            {
                if (_idCache.TryGetValue(instanceId.Value, out var go) && go != null)
                    return (go, null);
                return (null, $"InstanceId {instanceId.Value}에 해당하는 GameObject를 찾을 수 없습니다.");
            }

            // 2. Path (계층 경로)
            if (!string.IsNullOrEmpty(path))
            {
                if (_pathCache.TryGetValue(path, out var go) && go != null)
                    return (go, null);

                // 부분 경로 매칭 시도
                var partial = _pathCache.Keys
                    .Where(k => k.EndsWith("/" + path, StringComparison.OrdinalIgnoreCase) || k.Equals(path, StringComparison.OrdinalIgnoreCase))
                    .Take(5).ToList();

                if (partial.Count == 1 && _pathCache.TryGetValue(partial[0], out go) && go != null)
                    return (go, null);

                var suggestion = partial.Count > 0 ? $" 유사 경로: {string.Join(", ", partial)}" : "";
                return (null, $"경로 '{path}'에 해당하는 GameObject를 찾을 수 없습니다.{suggestion}");
            }

            // 3. Name
            if (!string.IsNullOrEmpty(name))
            {
                // 3a. 정확한 이름 매칭
                if (_nameCache.TryGetValue(name, out var list))
                {
                    var valid = list.Where(g => g != null).ToList();
                    if (valid.Count == 1)
                        return (valid[0], null);
                    if (valid.Count > 1)
                        return (valid[0], null); // 첫 번째 반환 (Unity-Skills와 달리 로그로 경고)
                }

                // 3b. 부분 매칭 (contains)
                var partialMatches = _nameCache
                    .Where(kv => kv.Key.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    .SelectMany(kv => kv.Value)
                    .Where(g => g != null)
                    .Take(5).ToList();

                if (partialMatches.Count == 1)
                    return (partialMatches[0], null);

                // 유사 이름 제안
                var suggestions = _nameCache.Keys
                    .Where(k => k.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                (name.Length >= 3 && k.StartsWith(name.Substring(0, Math.Min(3, name.Length)), StringComparison.OrdinalIgnoreCase)))
                    .Take(5).ToList();

                var suggestStr = suggestions.Count > 0 ? $" 유사한 이름: {string.Join(", ", suggestions)}" : "";
                return (null, $"이름 '{name}'에 해당하는 GameObject를 찾을 수 없습니다.{suggestStr}");
            }

            // 4. Tag
            if (!string.IsNullOrEmpty(tag))
            {
                try
                {
                    var tagged = GameObject.FindGameObjectsWithTag(tag);
                    if (tagged.Length > 0) return (tagged[0], null);
                }
                catch (UnityException)
                {
                    return (null, $"유효하지 않은 태그: '{tag}'");
                }
                return (null, $"태그 '{tag}'를 가진 GameObject를 찾을 수 없습니다.");
            }

            // 5. Component type
            if (!string.IsNullOrEmpty(componentType))
            {
                var type = TypeCache.GetTypesDerivedFrom<Component>()
                    .FirstOrDefault(t => t.Name == componentType || t.FullName == componentType);
                if (type == null)
                    return (null, $"컴포넌트 타입을 찾을 수 없습니다: '{componentType}'");

                var obj = UnityEngine.Object.FindAnyObjectByType(type) as Component;
                if (obj != null) return (obj.gameObject, null);
                return (null, $"'{componentType}' 컴포넌트를 가진 GameObject를 찾을 수 없습니다.");
            }

            return (null, "검색 조건이 지정되지 않았습니다. instanceId, path, name, tag, componentType 중 하나를 제공하세요.");
        }

        /// <summary>GameObject의 전체 계층 경로</summary>
        public static string GetPath(GameObject go) => GetFullPath(go.transform);

        /// <summary>Transform의 전체 계층 경로</summary>
        public static string GetFullPath(Transform t)
        {
            var parts = new List<string>();
            var current = t;
            while (current != null)
            {
                parts.Insert(0, current.name);
                current = current.parent;
            }
            return string.Join("/", parts);
        }

        /// <summary>GameObject 정보를 풍부한 응답 객체로 직렬화</summary>
        public static object ToRichInfo(GameObject go)
        {
            var t = go.transform;
            return new
            {
                name = go.name,
                instanceId = go.GetInstanceID(),
                path = GetFullPath(t),
                active = go.activeSelf,
                activeInHierarchy = go.activeInHierarchy,
                tag = go.tag,
                layer = LayerMask.LayerToName(go.layer),
                isStatic = go.isStatic,
                position = new { x = t.position.x, y = t.position.y, z = t.position.z },
                rotation = new { x = t.eulerAngles.x, y = t.eulerAngles.y, z = t.eulerAngles.z },
                scale = new { x = t.localScale.x, y = t.localScale.y, z = t.localScale.z },
                childCount = t.childCount,
                components = go.GetComponents<Component>()
                    .Where(c => c != null)
                    .Select(c => c.GetType().Name)
                    .ToArray(),
            };
        }
    }
}
