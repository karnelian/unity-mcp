#if UNITY_SPLINES
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace KarnelLabs.MCP
{
    public static class SplineHandler
    {
        public static void Register()
        {
            CommandRouter.Register("spline.create", Create);
            CommandRouter.Register("spline.getInfo", GetInfo);
            CommandRouter.Register("spline.addKnot", AddKnot);
            CommandRouter.Register("spline.removeKnot", RemoveKnot);
            CommandRouter.Register("spline.setKnot", SetKnot);
            CommandRouter.Register("spline.getKnots", GetKnots);
            CommandRouter.Register("spline.setTangentMode", SetTangentMode);
            CommandRouter.Register("spline.find", Find);
            CommandRouter.Register("spline.addExtrude", AddExtrude);
            CommandRouter.Register("spline.addAnimate", AddAnimate);
            CommandRouter.Register("spline.addInstantiate", AddInstantiate);
        }

        private static SplineContainer FindSpline(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var sc = go.GetComponent<SplineContainer>();
            if (sc == null) throw new McpException(-32010, $"No SplineContainer on '{go.name}'");
            return sc;
        }

        private static object Create(JToken p)
        {
            var name = p["name"]?.Value<string>() ?? "Spline";
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create Spline");

            if (p["position"] != null)
                go.transform.position = new Vector3(
                    p["position"]["x"]?.Value<float>() ?? 0,
                    p["position"]["y"]?.Value<float>() ?? 0,
                    p["position"]["z"]?.Value<float>() ?? 0);

            var container = go.AddComponent<SplineContainer>();
            var spline = container.Spline;

            var closed = p["closed"]?.Value<bool>() ?? false;
            spline.Closed = closed;

            // Add initial knots if provided
            var knots = p["knots"] as JArray;
            if (knots != null)
            {
                spline.Clear();
                foreach (var k in knots)
                {
                    var pos = new float3(
                        k["x"]?.Value<float>() ?? 0,
                        k["y"]?.Value<float>() ?? 0,
                        k["z"]?.Value<float>() ?? 0);
                    spline.Add(new BezierKnot(pos));
                }
            }

            return new
            {
                success = true, name = go.name, instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                knotCount = spline.Count, closed = spline.Closed,
            };
        }

        private static object GetInfo(JToken p)
        {
            var sc = FindSpline(p);
            var spline = sc.Spline;
            return new
            {
                name = sc.gameObject.name,
                path = GameObjectFinder.GetPath(sc.gameObject),
                knotCount = spline.Count,
                closed = spline.Closed,
                length = spline.GetLength(),
            };
        }

        private static object AddKnot(JToken p)
        {
            var sc = FindSpline(p);
            var spline = sc.Spline;
            Undo.RecordObject(sc, "Add Spline Knot");

            var pos = new float3(
                p["x"]?.Value<float>() ?? 0,
                p["y"]?.Value<float>() ?? 0,
                p["z"]?.Value<float>() ?? 0);

            var knot = new BezierKnot(pos);

            // Optional tangent in/out
            if (p["tangentIn"] != null)
                knot.TangentIn = new float3(
                    p["tangentIn"]["x"]?.Value<float>() ?? 0,
                    p["tangentIn"]["y"]?.Value<float>() ?? 0,
                    p["tangentIn"]["z"]?.Value<float>() ?? 0);
            if (p["tangentOut"] != null)
                knot.TangentOut = new float3(
                    p["tangentOut"]["x"]?.Value<float>() ?? 0,
                    p["tangentOut"]["y"]?.Value<float>() ?? 0,
                    p["tangentOut"]["z"]?.Value<float>() ?? 0);

            var index = p["index"]?.Value<int>();
            if (index.HasValue && index.Value >= 0 && index.Value <= spline.Count)
                spline.Insert(index.Value, knot);
            else
                spline.Add(knot);

            EditorUtility.SetDirty(sc);
            return new { success = true, knotCount = spline.Count };
        }

        private static object RemoveKnot(JToken p)
        {
            var sc = FindSpline(p);
            var spline = sc.Spline;
            var index = Validate.Required<int>(p, "index");

            if (index < 0 || index >= spline.Count)
                throw new McpException(-32602, $"Knot index {index} out of range (0..{spline.Count - 1})");

            Undo.RecordObject(sc, "Remove Spline Knot");
            spline.RemoveAt(index);
            EditorUtility.SetDirty(sc);
            return new { success = true, knotCount = spline.Count };
        }

        private static object SetKnot(JToken p)
        {
            var sc = FindSpline(p);
            var spline = sc.Spline;
            var index = Validate.Required<int>(p, "index");

            if (index < 0 || index >= spline.Count)
                throw new McpException(-32602, $"Knot index {index} out of range (0..{spline.Count - 1})");

            Undo.RecordObject(sc, "Set Spline Knot");
            var knot = spline[index];

            if (p["position"] != null)
                knot.Position = new float3(
                    p["position"]["x"]?.Value<float>() ?? knot.Position.x,
                    p["position"]["y"]?.Value<float>() ?? knot.Position.y,
                    p["position"]["z"]?.Value<float>() ?? knot.Position.z);

            if (p["tangentIn"] != null)
                knot.TangentIn = new float3(
                    p["tangentIn"]["x"]?.Value<float>() ?? 0,
                    p["tangentIn"]["y"]?.Value<float>() ?? 0,
                    p["tangentIn"]["z"]?.Value<float>() ?? 0);

            if (p["tangentOut"] != null)
                knot.TangentOut = new float3(
                    p["tangentOut"]["x"]?.Value<float>() ?? 0,
                    p["tangentOut"]["y"]?.Value<float>() ?? 0,
                    p["tangentOut"]["z"]?.Value<float>() ?? 0);

            if (p["rotation"] != null)
                knot.Rotation = new quaternion(
                    p["rotation"]["x"]?.Value<float>() ?? 0,
                    p["rotation"]["y"]?.Value<float>() ?? 0,
                    p["rotation"]["z"]?.Value<float>() ?? 0,
                    p["rotation"]["w"]?.Value<float>() ?? 1);

            spline[index] = knot;
            EditorUtility.SetDirty(sc);
            return new { success = true, index };
        }

        private static object GetKnots(JToken p)
        {
            var sc = FindSpline(p);
            var spline = sc.Spline;
            var knots = new List<object>();
            for (int i = 0; i < spline.Count; i++)
            {
                var k = spline[i];
                knots.Add(new
                {
                    index = i,
                    position = new { x = k.Position.x, y = k.Position.y, z = k.Position.z },
                    tangentIn = new { x = k.TangentIn.x, y = k.TangentIn.y, z = k.TangentIn.z },
                    tangentOut = new { x = k.TangentOut.x, y = k.TangentOut.y, z = k.TangentOut.z },
                    rotation = new { x = k.Rotation.value.x, y = k.Rotation.value.y, z = k.Rotation.value.z, w = k.Rotation.value.w },
                });
            }
            return new { knotCount = spline.Count, closed = spline.Closed, knots = knots.ToArray() };
        }

        private static object SetTangentMode(JToken p)
        {
            var sc = FindSpline(p);
            var spline = sc.Spline;
            var index = Validate.Required<int>(p, "index");
            var modeStr = Validate.Required<string>(p, "mode");

            if (index < 0 || index >= spline.Count)
                throw new McpException(-32602, $"Knot index {index} out of range (0..{spline.Count - 1})");

            if (!Enum.TryParse<TangentMode>(modeStr, true, out var mode))
                throw new McpException(-32602, $"Invalid tangent mode: {modeStr}. Use: AutoSmooth, Linear, Broken");

            Undo.RecordObject(sc, "Set Tangent Mode");
            spline.SetTangentMode(index, mode);
            EditorUtility.SetDirty(sc);
            return new { success = true, index, mode = mode.ToString() };
        }

        private static object Find(JToken p)
        {
            var containers = UnityEngine.Object.FindObjectsByType<SplineContainer>(FindObjectsSortMode.None);
            var nameFilter = p["nameFilter"]?.Value<string>();
            var results = containers
                .Where(c => string.IsNullOrEmpty(nameFilter) || c.gameObject.name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                .Select(c => new
                {
                    name = c.gameObject.name,
                    path = GameObjectFinder.GetPath(c.gameObject),
                    instanceId = c.gameObject.GetInstanceID(),
                    knotCount = c.Spline.Count,
                    closed = c.Spline.Closed,
                }).ToArray();
            return new { count = results.Length, splines = results };
        }

        private static object AddExtrude(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (go.GetComponent<SplineContainer>() == null)
                throw new McpException(-32010, $"No SplineContainer on '{go.name}'");

            Undo.RecordObject(go, "Add SplineExtrude");
            var extrude = go.GetComponent<SplineExtrude>();
            if (extrude == null) extrude = go.AddComponent<SplineExtrude>();

            if (p["radius"] != null) extrude.Radius = p["radius"].Value<float>();
            if (p["sides"] != null) extrude.Sides = p["sides"].Value<int>();
            if (p["capped"] != null) extrude.Capped = p["capped"].Value<bool>();

            EditorUtility.SetDirty(go);
            return new { success = true, name = go.name, component = "SplineExtrude" };
        }

        private static object AddAnimate(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (go.GetComponent<SplineContainer>() == null)
                throw new McpException(-32010, $"No SplineContainer on '{go.name}'");

            Undo.RecordObject(go, "Add SplineAnimate");
            var animate = go.GetComponent<SplineAnimate>();
            if (animate == null) animate = go.AddComponent<SplineAnimate>();

            if (p["duration"] != null) animate.Duration = p["duration"].Value<float>();
            if (p["loop"] != null)
            {
                if (Enum.TryParse<SplineAnimate.LoopMode>(p["loop"].Value<string>(), true, out var loop))
                    animate.Loop = loop;
            }
            if (p["easingMode"] != null)
            {
                if (Enum.TryParse<SplineAnimate.EasingMode>(p["easingMode"].Value<string>(), true, out var easing))
                    animate.Easing = easing;
            }

            EditorUtility.SetDirty(go);
            return new { success = true, name = go.name, component = "SplineAnimate" };
        }

        private static object AddInstantiate(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (go.GetComponent<SplineContainer>() == null)
                throw new McpException(-32010, $"No SplineContainer on '{go.name}'");

            Undo.RecordObject(go, "Add SplineInstantiate");
            var inst = go.GetComponent<SplineInstantiate>();
            if (inst == null) inst = go.AddComponent<SplineInstantiate>();

            // SplineInstantiate configuration via SerializedObject
            var so = new SerializedObject(inst);
            if (p["spacing"] != null)
            {
                var spacingProp = so.FindProperty("m_Spacing");
                if (spacingProp != null && spacingProp.propertyType == SerializedPropertyType.Float)
                    spacingProp.floatValue = p["spacing"].Value<float>();
                else if (spacingProp != null && spacingProp.propertyType == SerializedPropertyType.Integer)
                    spacingProp.intValue = (int)p["spacing"].Value<float>();
            }
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(go);
            return new { success = true, name = go.name, component = "SplineInstantiate" };
        }
    }
}
#endif
