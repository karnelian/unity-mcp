using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

namespace KarnelLabs.MCP
{
    public static class ConstraintHandler
    {
        public static void Register()
        {
            CommandRouter.Register("constraint.addAim", AddAim);
            CommandRouter.Register("constraint.addParent", AddParent);
            CommandRouter.Register("constraint.addPosition", AddPosition);
            CommandRouter.Register("constraint.addRotation", AddRotation);
            CommandRouter.Register("constraint.addScale", AddScale);
            CommandRouter.Register("constraint.addLookAt", AddLookAt);
            CommandRouter.Register("constraint.getInfo", GetInfo);
            CommandRouter.Register("constraint.find", Find);
        }

        private static void AddSources(IConstraint constraint, JToken sourcesToken)
        {
            if (sourcesToken is not JArray sources) return;
            foreach (var s in sources)
            {
                string srcName = (string)s["sourceTransform"];
                float weight = s["weight"]?.Value<float>() ?? 1f;
                var srcGo = GameObjectFinder.FindByName(srcName);
                var cs = new ConstraintSource { sourceTransform = srcGo.transform, weight = weight };
                constraint.AddSource(cs);
            }
        }

        private static object AddAim(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add AimConstraint");
            var c = Undo.AddComponent<AimConstraint>(go);

            AddSources(c, p["sources"]);
            if (p["constraintActive"] != null) c.constraintActive = (bool)p["constraintActive"];
            if (p["locked"] != null) c.locked = (bool)p["locked"];

            return new { success = true, gameObject = go.name, component = "AimConstraint" };
        }

        private static object AddParent(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add ParentConstraint");
            var c = Undo.AddComponent<ParentConstraint>(go);

            AddSources(c, p["sources"]);
            if (p["constraintActive"] != null) c.constraintActive = (bool)p["constraintActive"];
            if (p["locked"] != null) c.locked = (bool)p["locked"];
            if (p["translationAtRest"] != null) c.translationAtRest = JsonHelper.ToVector3(p["translationAtRest"]);
            if (p["rotationAtRest"] != null) c.rotationAtRest = JsonHelper.ToVector3(p["rotationAtRest"]);

            return new { success = true, gameObject = go.name, component = "ParentConstraint" };
        }

        private static object AddPosition(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add PositionConstraint");
            var c = Undo.AddComponent<PositionConstraint>(go);

            AddSources(c, p["sources"]);
            if (p["constraintActive"] != null) c.constraintActive = (bool)p["constraintActive"];
            if (p["locked"] != null) c.locked = (bool)p["locked"];
            if (p["translationAtRest"] != null) c.translationAtRest = JsonHelper.ToVector3(p["translationAtRest"]);
            if (p["translationOffset"] != null) c.translationOffset = JsonHelper.ToVector3(p["translationOffset"]);

            return new { success = true, gameObject = go.name, component = "PositionConstraint" };
        }

        private static object AddRotation(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add RotationConstraint");
            var c = Undo.AddComponent<RotationConstraint>(go);

            AddSources(c, p["sources"]);
            if (p["constraintActive"] != null) c.constraintActive = (bool)p["constraintActive"];
            if (p["locked"] != null) c.locked = (bool)p["locked"];
            if (p["rotationAtRest"] != null) c.rotationAtRest = JsonHelper.ToVector3(p["rotationAtRest"]);
            if (p["rotationOffset"] != null) c.rotationOffset = JsonHelper.ToVector3(p["rotationOffset"]);

            return new { success = true, gameObject = go.name, component = "RotationConstraint" };
        }

        private static object AddScale(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add ScaleConstraint");
            var c = Undo.AddComponent<ScaleConstraint>(go);

            AddSources(c, p["sources"]);
            if (p["constraintActive"] != null) c.constraintActive = (bool)p["constraintActive"];
            if (p["locked"] != null) c.locked = (bool)p["locked"];
            if (p["scaleAtRest"] != null) c.scaleAtRest = JsonHelper.ToVector3(p["scaleAtRest"]);
            if (p["scaleOffset"] != null) c.scaleOffset = JsonHelper.ToVector3(p["scaleOffset"]);

            return new { success = true, gameObject = go.name, component = "ScaleConstraint" };
        }

        private static object AddLookAt(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add LookAtConstraint");
            var c = Undo.AddComponent<LookAtConstraint>(go);

            AddSources(c, p["sources"]);
            if (p["constraintActive"] != null) c.constraintActive = (bool)p["constraintActive"];
            if (p["locked"] != null) c.locked = (bool)p["locked"];
            if (p["roll"] != null) c.roll = (float)p["roll"];
            if (p["useUpObject"] != null) c.useUpObject = (bool)p["useUpObject"];
            if (p["worldUpObject"] != null)
            {
                var upGo = GameObjectFinder.FindByName((string)p["worldUpObject"]);
                c.worldUpObject = upGo.transform;
            }

            return new { success = true, gameObject = go.name, component = "LookAtConstraint" };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            string typeFilter = (string)p?["constraintType"];

            var constraints = go.GetComponents<IConstraint>()
                .Where(c => typeFilter == null || c.GetType().Name.Contains(typeFilter))
                .Select(c =>
                {
                    var sources = new System.Collections.Generic.List<object>();
                    for (int i = 0; i < c.sourceCount; i++)
                    {
                        var src = c.GetSource(i);
                        sources.Add(new
                        {
                            sourceTransform = src.sourceTransform != null ? src.sourceTransform.name : null,
                            weight = src.weight
                        });
                    }

                    return new
                    {
                        type = c.GetType().Name,
                        constraintActive = c.constraintActive,
                        locked = c.locked,
                        weight = c.weight,
                        sourceCount = c.sourceCount,
                        sources
                    };
                }).ToArray();

            return new { gameObject = go.name, constraintCount = constraints.Length, constraints };
        }

        private static object Find(JToken p)
        {
            string typeFilter = (string)p?["constraintType"];

            var allGOs = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(mb => mb is IConstraint)
                .Select(mb => mb.gameObject)
                .Distinct();

            var result = allGOs.SelectMany(go =>
            {
                return go.GetComponents<IConstraint>()
                    .Where(c => typeFilter == null || typeFilter == "All" || c.GetType().Name.Contains(typeFilter))
                    .Select(c => new
                    {
                        gameObject = go.name,
                        path = GameObjectFinder.GetPath(go),
                        constraintType = c.GetType().Name,
                        constraintActive = c.constraintActive,
                    });
            }).ToArray();

            return new { count = result.Length, constraints = result };
        }
    }
}
