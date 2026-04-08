using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class AnimatorHandler
    {
        public static void Register()
        {
            CommandRouter.Register("animator.createController", CreateController);
            CommandRouter.Register("animator.assignController", AssignController);
            CommandRouter.Register("animator.addParameter", AddParameter);
            CommandRouter.Register("animator.removeParameter", RemoveParameter);
            CommandRouter.Register("animator.getParameters", GetParameters);
            CommandRouter.Register("animator.setParameter", SetParameter);
            CommandRouter.Register("animator.addState", AddState);
            CommandRouter.Register("animator.addTransition", AddTransition);
            CommandRouter.Register("animator.getStates", GetStates);
            CommandRouter.Register("animator.getInfo", GetInfo);
            CommandRouter.Register("animator.addLayer", AddLayer);
            CommandRouter.Register("animator.removeLayer", RemoveLayer);
            CommandRouter.Register("animator.setLayerWeight", SetLayerWeight);
            CommandRouter.Register("animator.getLayers", GetLayers);
            CommandRouter.Register("animator.createBlendTree", CreateBlendTree);
            CommandRouter.Register("animator.removeState", RemoveState);
            CommandRouter.Register("animator.removeTransition", RemoveTransition);
        }

        private static AnimatorController LoadController(JToken p)
        {
            var path = Validate.Required<string>(p, "controllerPath");
            path = Validate.SafeAssetPath(path);
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (ctrl == null) throw new McpException(-32003, $"AnimatorController not found: {path}");
            return ctrl;
        }

        private static object ControllerInfo(AnimatorController ctrl)
        {
            var path = AssetDatabase.GetAssetPath(ctrl);
            return new
            {
                name = ctrl.name,
                path,
                layers = ctrl.layers.Select(l => new { name = l.name, stateCount = l.stateMachine.states.Length }).ToArray(),
                parameters = ctrl.parameters.Select(p => new { name = p.name, type = p.type.ToString(), defaultFloat = p.defaultFloat, defaultInt = p.defaultInt, defaultBool = p.defaultBool }).ToArray(),
            };
        }

        private static object CreateController(JToken p)
        {
            var ctrlName = Validate.Required<string>(p, "name");
            var savePath = p["savePath"]?.Value<string>() ?? $"Assets/Animations/{ctrlName}.controller";
            savePath = Validate.SafeAssetPath(savePath);

            // Auto-append .controller extension if savePath is a directory or missing extension
            if (!savePath.EndsWith(".controller", StringComparison.OrdinalIgnoreCase))
            {
                if (!Path.HasExtension(savePath))
                    savePath = Path.Combine(savePath, $"{ctrlName}.controller").Replace("\\", "/");
                else
                    savePath = Path.ChangeExtension(savePath, ".controller");
            }

            var dir = Path.GetDirectoryName(Path.Combine(Application.dataPath, "..", savePath));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(savePath);
            ctrl.name = ctrlName;

            // Add default parameters
            if (p["parameters"] is JArray parameters)
            {
                foreach (var param in parameters)
                {
                    var paramName = param["name"]?.Value<string>();
                    var paramType = param["type"]?.Value<string>() ?? "Float";
                    if (string.IsNullOrEmpty(paramName)) continue;
                    var type = Validate.ParseEnum<AnimatorControllerParameterType>(paramType, "type");
                    ctrl.AddParameter(paramName, type);
                }
            }

            AssetDatabase.SaveAssets();
            return ControllerInfo(ctrl);
        }

        private static object AssignController(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var animator = go.GetComponent<Animator>();
            if (animator == null)
            {
                WorkflowManager.SnapshotObject(go);
                animator = Undo.AddComponent<Animator>(go);
            }

            var ctrlPath = Validate.Required<string>(p, "controllerPath");
            ctrlPath = Validate.SafeAssetPath(ctrlPath);
            var ctrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
            if (ctrl == null) throw new McpException(-32003, $"Controller not found: {ctrlPath}");

            Undo.RecordObject(animator, "MCP: Assign Controller");
            animator.runtimeAnimatorController = ctrl;
            return new { gameObject = go.name, controller = ctrl.name };
        }

        private static object AddParameter(JToken p)
        {
            var ctrl = LoadController(p);
            var paramName = Validate.Required<string>(p, "parameterName");
            var paramType = p["parameterType"]?.Value<string>() ?? "Float";
            var type = Validate.ParseEnum<AnimatorControllerParameterType>(paramType, "parameterType");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            ctrl.AddParameter(paramName, type);
            AssetDatabase.SaveAssets();
            return new { added = true, name = paramName, type = type.ToString() };
        }

        private static object RemoveParameter(JToken p)
        {
            var ctrl = LoadController(p);
            var paramName = Validate.Required<string>(p, "parameterName");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            var param = ctrl.parameters.FirstOrDefault(x => x.name == paramName);
            if (param == null) throw new McpException(-32602, $"Parameter '{paramName}' not found");
            ctrl.RemoveParameter(param);
            AssetDatabase.SaveAssets();
            return new { removed = true, name = paramName };
        }

        private static object GetParameters(JToken p)
        {
            var ctrl = LoadController(p);
            return new
            {
                parameters = ctrl.parameters.Select(x => new
                {
                    name = x.name,
                    type = x.type.ToString(),
                    defaultFloat = x.defaultFloat,
                    defaultInt = x.defaultInt,
                    defaultBool = x.defaultBool,
                }).ToArray(),
            };
        }

        private static object SetParameter(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var animator = go.GetComponent<Animator>();
            if (animator == null) throw new McpException(-32602, $"No Animator on '{go.name}'");

            var paramName = Validate.Required<string>(p, "parameterName");
            var paramType = p["parameterType"]?.Value<string>();

            if (paramType == null)
            {
                // Auto-detect
                var ctrl = animator.runtimeAnimatorController as AnimatorController;
                if (ctrl != null)
                {
                    var param = ctrl.parameters.FirstOrDefault(x => x.name == paramName);
                    if (param != null) paramType = param.type.ToString();
                }
            }

            switch (paramType?.ToLower())
            {
                case "float": animator.SetFloat(paramName, p["value"].Value<float>()); break;
                case "int": case "integer": animator.SetInteger(paramName, p["value"].Value<int>()); break;
                case "bool": case "boolean": animator.SetBool(paramName, p["value"].Value<bool>()); break;
                case "trigger": animator.SetTrigger(paramName); break;
                default: throw new McpException(-32602, $"Unknown parameter type: {paramType}");
            }

            return new { gameObject = go.name, parameter = paramName, type = paramType };
        }

        private static object AddState(JToken p)
        {
            var ctrl = LoadController(p);
            var stateName = Validate.Required<string>(p, "stateName");
            var layerIndex = p["layerIndex"]?.Value<int>() ?? 0;
            var clipPath = p["clipPath"]?.Value<string>();

            if (layerIndex >= ctrl.layers.Length)
                throw new McpException(-32602, $"Layer index {layerIndex} out of range");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            var sm = ctrl.layers[layerIndex].stateMachine;
            var state = sm.AddState(stateName);

            if (!string.IsNullOrEmpty(clipPath))
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Validate.SafeAssetPath(clipPath));
                if (clip != null) state.motion = clip;
            }

            if (p["isDefault"]?.Value<bool>() == true) sm.defaultState = state;
            if (p["speed"] != null) state.speed = p["speed"].Value<float>();

            AssetDatabase.SaveAssets();
            return new { added = true, state = stateName, layer = ctrl.layers[layerIndex].name };
        }

        private static object AddTransition(JToken p)
        {
            var ctrl = LoadController(p);
            var fromState = Validate.Required<string>(p, "fromState");
            var toState = Validate.Required<string>(p, "toState");
            var layerIndex = p["layerIndex"]?.Value<int>() ?? 0;

            if (layerIndex >= ctrl.layers.Length)
                throw new McpException(-32602, $"Layer index {layerIndex} out of range");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            var sm = ctrl.layers[layerIndex].stateMachine;
            var from = sm.states.FirstOrDefault(s => s.state.name == fromState).state;
            var to = sm.states.FirstOrDefault(s => s.state.name == toState).state;

            if (from == null) throw new McpException(-32602, $"State '{fromState}' not found");
            if (to == null) throw new McpException(-32602, $"State '{toState}' not found");

            var transition = from.AddTransition(to);
            if (p["hasExitTime"] != null) transition.hasExitTime = p["hasExitTime"].Value<bool>();
            if (p["duration"] != null) transition.duration = p["duration"].Value<float>();
            if (p["exitTime"] != null) transition.exitTime = p["exitTime"].Value<float>();

            // Add conditions
            if (p["conditions"] is JArray conditions)
            {
                foreach (var cond in conditions)
                {
                    var paramName = cond["parameter"]?.Value<string>();
                    var mode = cond["mode"]?.Value<string>() ?? "If";
                    var threshold = cond["threshold"]?.Value<float>() ?? 0;
                    transition.AddCondition(Validate.ParseEnum<AnimatorConditionMode>(mode, "mode"), threshold, paramName);
                }
            }

            AssetDatabase.SaveAssets();
            return new { added = true, from = fromState, to = toState };
        }

        private static object GetStates(JToken p)
        {
            var ctrl = LoadController(p);
            var layerIndex = p["layerIndex"]?.Value<int>() ?? 0;

            if (layerIndex >= ctrl.layers.Length)
                throw new McpException(-32602, $"Layer index {layerIndex} out of range");

            var sm = ctrl.layers[layerIndex].stateMachine;
            var states = sm.states.Select(s => new
            {
                name = s.state.name,
                motion = s.state.motion?.name,
                speed = s.state.speed,
                isDefault = sm.defaultState == s.state,
                transitions = s.state.transitions.Select(t => new
                {
                    destination = t.destinationState?.name,
                    hasExitTime = t.hasExitTime,
                    duration = t.duration,
                    conditionCount = t.conditions.Length,
                }).ToArray(),
            }).ToArray();

            return new { layer = ctrl.layers[layerIndex].name, states };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var animator = go.GetComponent<Animator>();
            if (animator == null) throw new McpException(-32602, $"No Animator on '{go.name}'");

            var ctrl = animator.runtimeAnimatorController as AnimatorController;
            if (ctrl == null) return new { gameObject = go.name, hasController = false };

            return new { gameObject = go.name, hasController = true, controller = ControllerInfo(ctrl) };
        }

        private static object AddLayer(JToken p)
        {
            var ctrl = LoadController(p);
            var layerName = Validate.Required<string>(p, "layerName");
            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            ctrl.AddLayer(layerName);

            var layers = ctrl.layers;
            var newLayer = layers[layers.Length - 1];
            if (p["defaultWeight"] != null)
            {
                newLayer.defaultWeight = p["defaultWeight"].Value<float>();
                ctrl.layers = layers;
            }
            if (p["blendingMode"] != null)
            {
                if (Enum.TryParse<AnimatorLayerBlendingMode>(p["blendingMode"].Value<string>(), true, out var bm))
                {
                    newLayer.blendingMode = bm;
                    ctrl.layers = layers;
                }
            }
            AssetDatabase.SaveAssets();
            return new { added = true, layerName, index = layers.Length - 1 };
        }

        private static object RemoveLayer(JToken p)
        {
            var ctrl = LoadController(p);
            var layerIndex = Validate.Required<int>(p, "layerIndex");
            if (layerIndex <= 0 || layerIndex >= ctrl.layers.Length)
                throw new McpException(-32602, $"Cannot remove layer {layerIndex} (base layer or out of range)");
            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            ctrl.RemoveLayer(layerIndex);
            AssetDatabase.SaveAssets();
            return new { removed = true, layerIndex };
        }

        private static object SetLayerWeight(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var animator = go.GetComponent<Animator>();
            if (animator == null) throw new McpException(-32602, $"No Animator on '{go.name}'");
            var layerIndex = Validate.Required<int>(p, "layerIndex");
            var weight = Validate.Required<float>(p, "weight");
            animator.SetLayerWeight(layerIndex, weight);
            return new { success = true, layerIndex, weight };
        }

        private static object GetLayers(JToken p)
        {
            var ctrl = LoadController(p);
            return new
            {
                layerCount = ctrl.layers.Length,
                layers = ctrl.layers.Select((l, i) => new
                {
                    index = i,
                    name = l.name,
                    defaultWeight = l.defaultWeight,
                    blendingMode = l.blendingMode.ToString(),
                    stateCount = l.stateMachine.states.Length,
                }).ToArray(),
            };
        }

        private static object CreateBlendTree(JToken p)
        {
            var ctrl = LoadController(p);
            var treeName = p["treeName"]?.Value<string>() ?? "BlendTree";
            var layerIndex = p["layerIndex"]?.Value<int>() ?? 0;
            var parameterName = Validate.Required<string>(p, "parameterName");
            var blendType = p["blendType"]?.Value<string>() ?? "Simple1D";

            if (layerIndex >= ctrl.layers.Length)
                throw new McpException(-32602, $"Layer index {layerIndex} out of range");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            var sm = ctrl.layers[layerIndex].stateMachine;
            BlendTree tree;
            var state = ctrl.CreateBlendTreeInController(treeName, out tree, layerIndex);
            tree.blendParameter = parameterName;

            if (Enum.TryParse<BlendTreeType>(blendType, true, out var bt))
                tree.blendType = bt;

            var motions = p["motions"] as JArray;
            if (motions != null)
            {
                foreach (var m in motions)
                {
                    var clipPath = m["clipPath"]?.Value<string>();
                    var threshold = m["threshold"]?.Value<float>() ?? 0;
                    AnimationClip clip = null;
                    if (!string.IsNullOrEmpty(clipPath))
                        clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    tree.AddChild(clip, threshold);
                }
            }

            AssetDatabase.SaveAssets();
            return new { success = true, treeName, parameterName, blendType, layer = ctrl.layers[layerIndex].name };
        }

        private static object RemoveState(JToken p)
        {
            var ctrl = LoadController(p);
            var stateName = Validate.Required<string>(p, "stateName");
            var layerIndex = p["layerIndex"]?.Value<int>() ?? 0;
            if (layerIndex >= ctrl.layers.Length)
                throw new McpException(-32602, $"Layer index {layerIndex} out of range");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            var sm = ctrl.layers[layerIndex].stateMachine;
            var stateEntry = sm.states.FirstOrDefault(s => s.state.name == stateName);
            if (stateEntry.state == null) throw new McpException(-32602, $"State '{stateName}' not found");
            sm.RemoveState(stateEntry.state);
            AssetDatabase.SaveAssets();
            return new { removed = true, stateName, layer = ctrl.layers[layerIndex].name };
        }

        private static object RemoveTransition(JToken p)
        {
            var ctrl = LoadController(p);
            var fromState = Validate.Required<string>(p, "fromState");
            var toState = Validate.Required<string>(p, "toState");
            var layerIndex = p["layerIndex"]?.Value<int>() ?? 0;
            if (layerIndex >= ctrl.layers.Length)
                throw new McpException(-32602, $"Layer index {layerIndex} out of range");

            WorkflowManager.SnapshotAsset(AssetDatabase.GetAssetPath(ctrl));
            var sm = ctrl.layers[layerIndex].stateMachine;
            var from = sm.states.FirstOrDefault(s => s.state.name == fromState).state;
            if (from == null) throw new McpException(-32602, $"State '{fromState}' not found");

            var transition = from.transitions.FirstOrDefault(t => t.destinationState?.name == toState);
            if (transition == null) throw new McpException(-32602, $"No transition from '{fromState}' to '{toState}'");
            from.RemoveTransition(transition);
            AssetDatabase.SaveAssets();
            return new { removed = true, from = fromState, to = toState };
        }
    }
}
