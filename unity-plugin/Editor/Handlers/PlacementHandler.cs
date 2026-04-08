using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class PlacementHandler
    {
        public static void Register()
        {
            CommandRouter.Register("placement.align", Align);
            CommandRouter.Register("placement.distribute", Distribute);
            CommandRouter.Register("placement.snap", Snap);
            CommandRouter.Register("placement.randomize", Randomize);
            CommandRouter.Register("placement.circle", ArrangeCircle);
            CommandRouter.Register("placement.grid", ArrangeGrid);
            CommandRouter.Register("placement.stack", Stack);
            CommandRouter.Register("placement.groundSnap", GroundSnap);
            CommandRouter.Register("placement.mirror", Mirror);
            CommandRouter.Register("placement.scatter", Scatter);
        }

        private static List<GameObject> ResolveTargets(JToken p)
        {
            var targets = new List<GameObject>();
            var names = p["names"]?.ToObject<string[]>();
            var paths = p["paths"]?.ToObject<string[]>();

            if (paths != null)
                foreach (var path in paths)
                    targets.Add(GameObjectFinder.FindOrThrow(path: path));
            else if (names != null)
                foreach (var n in names)
                    targets.Add(GameObjectFinder.FindOrThrow(name: n));

            if (targets.Count < 1) throw new McpException(-32602, "At least one target required (names or paths)");
            return targets;
        }

        private static object Align(JToken p)
        {
            var targets = ResolveTargets(p);
            var axis = Validate.Required<string>(p, "axis").ToLower(); // x, y, z
            var mode = p["mode"]?.Value<string>()?.ToLower() ?? "first"; // first, center, min, max

            foreach (var go in targets) WorkflowManager.SnapshotObject(go);

            float value;
            switch (mode)
            {
                case "first": value = GetAxis(targets[0].transform.position, axis); break;
                case "min": value = targets.Min(g => GetAxis(g.transform.position, axis)); break;
                case "max": value = targets.Max(g => GetAxis(g.transform.position, axis)); break;
                default: value = targets.Average(g => GetAxis(g.transform.position, axis)); break;
            }

            foreach (var go in targets)
            {
                Undo.RecordObject(go.transform, "MCP: Align");
                var pos = go.transform.position;
                go.transform.position = SetAxis(pos, axis, value);
            }

            return new { aligned = targets.Count, axis, mode, value };
        }

        private static object Distribute(JToken p)
        {
            var targets = ResolveTargets(p);
            if (targets.Count < 3) throw new McpException(-32602, "Distribute requires at least 3 objects");
            var axis = Validate.Required<string>(p, "axis").ToLower();

            foreach (var go in targets) WorkflowManager.SnapshotObject(go);

            var sorted = targets.OrderBy(g => GetAxis(g.transform.position, axis)).ToList();
            float min = GetAxis(sorted.First().transform.position, axis);
            float max = GetAxis(sorted.Last().transform.position, axis);
            float step = (max - min) / (sorted.Count - 1);

            for (int i = 1; i < sorted.Count - 1; i++)
            {
                Undo.RecordObject(sorted[i].transform, "MCP: Distribute");
                var pos = sorted[i].transform.position;
                sorted[i].transform.position = SetAxis(pos, axis, min + step * i);
            }

            return new { distributed = targets.Count, axis };
        }

        private static object Snap(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var gridSize = p["gridSize"]?.Value<float>() ?? 1f;
            WorkflowManager.SnapshotObject(go);

            Undo.RecordObject(go.transform, "MCP: Snap");
            var pos = go.transform.position;
            go.transform.position = new Vector3(
                Mathf.Round(pos.x / gridSize) * gridSize,
                Mathf.Round(pos.y / gridSize) * gridSize,
                Mathf.Round(pos.z / gridSize) * gridSize
            );

            return new { name = go.name, position = new { x = go.transform.position.x, y = go.transform.position.y, z = go.transform.position.z }, gridSize };
        }

        private static object Randomize(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(go.transform, "MCP: Randomize");

            if (p["positionRange"] != null)
            {
                var range = p["positionRange"].Value<float>();
                go.transform.position += new Vector3(
                    UnityEngine.Random.Range(-range, range),
                    UnityEngine.Random.Range(-range, range),
                    UnityEngine.Random.Range(-range, range)
                );
            }
            if (p["rotationRange"] != null)
            {
                var range = p["rotationRange"].Value<float>();
                go.transform.rotation *= Quaternion.Euler(
                    UnityEngine.Random.Range(-range, range),
                    UnityEngine.Random.Range(-range, range),
                    UnityEngine.Random.Range(-range, range)
                );
            }
            if (p["scaleRange"] != null)
            {
                var min = p["scaleRange"]["min"]?.Value<float>() ?? 0.8f;
                var max = p["scaleRange"]["max"]?.Value<float>() ?? 1.2f;
                var s = UnityEngine.Random.Range(min, max);
                go.transform.localScale = new Vector3(s, s, s);
            }

            var t = go.transform;
            return new
            {
                name = go.name,
                position = new { x = t.position.x, y = t.position.y, z = t.position.z },
                rotation = new { x = t.eulerAngles.x, y = t.eulerAngles.y, z = t.eulerAngles.z },
                scale = new { x = t.localScale.x, y = t.localScale.y, z = t.localScale.z },
            };
        }

        private static object ArrangeCircle(JToken p)
        {
            var targets = ResolveTargets(p);
            var radius = p["radius"]?.Value<float>() ?? 5f;
            var center = new Vector3(
                p["center"]?["x"]?.Value<float>() ?? 0,
                p["center"]?["y"]?.Value<float>() ?? 0,
                p["center"]?["z"]?.Value<float>() ?? 0
            );

            foreach (var go in targets) WorkflowManager.SnapshotObject(go);

            for (int i = 0; i < targets.Count; i++)
            {
                float angle = (360f / targets.Count) * i * Mathf.Deg2Rad;
                Undo.RecordObject(targets[i].transform, "MCP: Circle");
                targets[i].transform.position = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            }

            return new { arranged = targets.Count, radius, center = new { x = center.x, y = center.y, z = center.z } };
        }

        private static object ArrangeGrid(JToken p)
        {
            var targets = ResolveTargets(p);
            var columns = p["columns"]?.Value<int>() ?? Mathf.CeilToInt(Mathf.Sqrt(targets.Count));
            var spacing = p["spacing"]?.Value<float>() ?? 2f;
            var origin = new Vector3(
                p["origin"]?["x"]?.Value<float>() ?? 0,
                p["origin"]?["y"]?.Value<float>() ?? 0,
                p["origin"]?["z"]?.Value<float>() ?? 0
            );

            foreach (var go in targets) WorkflowManager.SnapshotObject(go);

            for (int i = 0; i < targets.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;
                Undo.RecordObject(targets[i].transform, "MCP: Grid");
                targets[i].transform.position = origin + new Vector3(col * spacing, 0, row * spacing);
            }

            return new { arranged = targets.Count, columns, spacing };
        }

        private static object Stack(JToken p)
        {
            var targets = ResolveTargets(p);
            var axis = p["axis"]?.Value<string>()?.ToLower() ?? "y";
            var gap = p["gap"]?.Value<float>() ?? 0f;
            var startPos = targets[0].transform.position;

            foreach (var go in targets) WorkflowManager.SnapshotObject(go);

            float offset = 0;
            foreach (var go in targets)
            {
                Undo.RecordObject(go.transform, "MCP: Stack");
                go.transform.position = SetAxis(startPos, axis, GetAxis(startPos, axis) + offset);
                var bounds = GetBounds(go);
                offset += GetAxis(bounds.size, axis) + gap;
            }

            return new { stacked = targets.Count, axis, gap };
        }

        private static object GroundSnap(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var layerMask = p["layerMask"]?.Value<int>() ?? ~0;
            WorkflowManager.SnapshotObject(go);

            Undo.RecordObject(go.transform, "MCP: GroundSnap");
            var pos = go.transform.position;
            if (Physics.Raycast(pos + Vector3.up * 100, Vector3.down, out RaycastHit hit, 200, layerMask))
            {
                var bounds = GetBounds(go);
                float yOffset = pos.y - bounds.min.y;
                go.transform.position = new Vector3(pos.x, hit.point.y + yOffset, pos.z);
                return new { name = go.name, snapped = true, y = go.transform.position.y, surface = hit.collider.name };
            }

            return new { name = go.name, snapped = false, reason = "No surface found below" };
        }

        private static object Mirror(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var axis = p["axis"]?.Value<string>()?.ToLower() ?? "x";
            var pivotValue = p["pivot"]?.Value<float>() ?? 0;
            WorkflowManager.SnapshotObject(go);

            Undo.RecordObject(go.transform, "MCP: Mirror");
            var pos = go.transform.position;
            float current = GetAxis(pos, axis);
            float mirrored = 2 * pivotValue - current;
            go.transform.position = SetAxis(pos, axis, mirrored);

            var scale = go.transform.localScale;
            go.transform.localScale = SetAxis(scale, axis, -GetAxis(scale, axis));

            return new { name = go.name, axis, pivot = pivotValue, newPosition = GetAxis(go.transform.position, axis) };
        }

        private static object Scatter(JToken p)
        {
            var prefabPath = Validate.Required<string>(p, "prefabPath");
            prefabPath = Validate.SafeAssetPath(prefabPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) throw new McpException(-32003, $"Prefab not found: {prefabPath}");

            var count = p["count"]?.Value<int>() ?? 10;
            var area = p["area"]?.Value<float>() ?? 20f;
            var center = new Vector3(
                p["center"]?["x"]?.Value<float>() ?? 0,
                p["center"]?["y"]?.Value<float>() ?? 0,
                p["center"]?["z"]?.Value<float>() ?? 0
            );
            var useGroundSnap = p["groundSnap"]?.Value<bool>() ?? false;
            var randomRotation = p["randomRotation"]?.Value<bool>() ?? false;
            var scaleMin = p["scaleMin"]?.Value<float>() ?? 1f;
            var scaleMax = p["scaleMax"]?.Value<float>() ?? 1f;
            var placed = new List<object>();

            for (int i = 0; i < count; i++)
            {
                var pos = center + new Vector3(
                    UnityEngine.Random.Range(-area / 2, area / 2),
                    0,
                    UnityEngine.Random.Range(-area / 2, area / 2)
                );

                if (useGroundSnap && Physics.Raycast(pos + Vector3.up * 100, Vector3.down, out RaycastHit hit, 200))
                    pos.y = hit.point.y;

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = pos;
                if (randomRotation) instance.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
                if (scaleMin != 1f || scaleMax != 1f)
                {
                    var s = UnityEngine.Random.Range(scaleMin, scaleMax);
                    instance.transform.localScale = Vector3.one * s;
                }
                Undo.RegisterCreatedObjectUndo(instance, "MCP: Scatter");
                placed.Add(new { name = instance.name, path = GameObjectFinder.GetPath(instance) });
            }

            return new { placed = placed.Count, prefab = prefabPath, area, objects = placed };
        }

        // Helpers
        private static float GetAxis(Vector3 v, string axis)
        {
            switch (axis) { case "x": return v.x; case "z": return v.z; default: return v.y; }
        }

        private static Vector3 SetAxis(Vector3 v, string axis, float value)
        {
            switch (axis) { case "x": v.x = value; break; case "z": v.z = value; break; default: v.y = value; break; }
            return v;
        }

        private static Bounds GetBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }
}
