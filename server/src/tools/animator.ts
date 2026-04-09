import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAnimatorTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_animator_create_controller", "Create AnimatorController", {
    name: z.string(), savePath: z.string().optional(),
    parameters: z.array(z.object({ name: z.string(), type: z.enum(["Float", "Int", "Bool", "Trigger"]) })).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.createController", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_assign_controller", "Assign AnimatorController", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    controllerPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("animator.assignController", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_add_parameter", "Add Animator parameter", {
    controllerPath: z.string(),
    parameterName: z.string(), parameterType: z.enum(["Float", "Int", "Bool", "Trigger"]).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.addParameter", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_remove_parameter", "Remove Animator parameter", {
    controllerPath: z.string(), parameterName: z.string(),
  }, async (p) => {
    const r = await bridge.request("animator.removeParameter", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_get_parameters", "Get Animator parameters", {
    controllerPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("animator.getParameters", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_set_parameter", "Set Animator parameter", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    parameterName: z.string(), parameterType: z.enum(["Float", "Int", "Bool", "Trigger"]).optional(),
    value: z.union([z.number(), z.boolean()]).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.setParameter", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_add_state", "Add Animator state", {
    controllerPath: z.string(), stateName: z.string(), layerIndex: z.number().optional(),
    clipPath: z.string().optional(), isDefault: z.boolean().optional(), speed: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("animator.addState", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_add_transition", "Add Animator transition", {
    controllerPath: z.string(), fromState: z.string(), toState: z.string(),
    layerIndex: z.number().optional(), hasExitTime: z.boolean().optional(),
    duration: z.number().optional(), exitTime: z.number().optional(),
    conditions: z.array(z.object({
      parameter: z.string(), mode: z.enum(["If", "IfNot", "Greater", "Less", "Equals", "NotEqual"]),
      threshold: z.number().optional(),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.addTransition", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_get_states", "Get Animator states", {
    controllerPath: z.string(), layerIndex: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("animator.getStates", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_get_info", "Get Animator info", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("animator.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_add_layer", "Add Animator layer", {
    controllerPath: z.string(),
    layerName: z.string(),
    defaultWeight: z.number().optional(),
    blendingMode: z.enum(["Override", "Additive"]).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.addLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_remove_layer", "Remove Animator layer", {
    controllerPath: z.string(),
    layerIndex: z.number(),
  }, async (p) => {
    const r = await bridge.request("animator.removeLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_set_layer_weight", "Set layer weight", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    layerIndex: z.number(),
    weight: z.number(),
  }, async (p) => {
    const r = await bridge.request("animator.setLayerWeight", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_get_layers", "Get Animator layers", {
    controllerPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("animator.getLayers", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_create_blend_tree", "Create BlendTree", {
    controllerPath: z.string(),
    treeName: z.string().optional(),
    parameterName: z.string(),
    layerIndex: z.number().optional(),
    blendType: z.enum(["Simple1D", "SimpleDirectional2D", "FreeformDirectional2D", "FreeformCartesian2D"]).optional(),
    motions: z.array(z.object({
      clipPath: z.string().optional(), threshold: z.number().optional(),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.createBlendTree", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_remove_state", "Remove Animator state", {
    controllerPath: z.string(),
    stateName: z.string(),
    layerIndex: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("animator.removeState", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_remove_transition", "Remove Animator transition", {
    controllerPath: z.string(),
    fromState: z.string(), toState: z.string(),
    layerIndex: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("animator.removeTransition", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
