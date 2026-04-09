import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

const sourceEntry = z.object({
  sourceTransform: z.string(),
  weight: z.number().optional(),
});

export function registerConstraintTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_constraint_addAim", "Add AimConstraint", {
    ...goRef,
    aimAxis: z.enum(["X", "Y", "Z", "NegX", "NegY", "NegZ"]).optional(),
    upAxis: z.enum(["X", "Y", "Z", "NegX", "NegY", "NegZ"]).optional(),
    worldUpType: z.enum(["SceneUp", "ObjectUp", "ObjectRotationUp", "Vector", "None"]).optional(),
    sources: z.array(sourceEntry).optional(),
    constraintActive: z.boolean().optional(),
    locked: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("constraint.addAim", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_constraint_addParent", "Add ParentConstraint", {
    ...goRef,
    sources: z.array(sourceEntry).optional(),
    constraintActive: z.boolean().optional(),
    locked: z.boolean().optional(),
    translationAtRest: vec3.optional(),
    rotationAtRest: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("constraint.addParent", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_constraint_addPosition", "Add PositionConstraint", {
    ...goRef,
    sources: z.array(sourceEntry).optional(),
    constraintActive: z.boolean().optional(),
    locked: z.boolean().optional(),
    translationAtRest: vec3.optional(),
    translationOffset: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("constraint.addPosition", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_constraint_addRotation", "Add RotationConstraint", {
    ...goRef,
    sources: z.array(sourceEntry).optional(),
    constraintActive: z.boolean().optional(),
    locked: z.boolean().optional(),
    rotationAtRest: vec3.optional(),
    rotationOffset: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("constraint.addRotation", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_constraint_addScale", "Add ScaleConstraint", {
    ...goRef,
    sources: z.array(sourceEntry).optional(),
    constraintActive: z.boolean().optional(),
    locked: z.boolean().optional(),
    scaleAtRest: vec3.optional(),
    scaleOffset: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("constraint.addScale", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_constraint_addLookAt", "Add LookAtConstraint", {
    ...goRef,
    sources: z.array(sourceEntry).optional(),
    constraintActive: z.boolean().optional(),
    locked: z.boolean().optional(),
    roll: z.number().optional(),
    useUpObject: z.boolean().optional(),
    worldUpObject: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("constraint.addLookAt", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_constraint_getInfo", "Get constraint info", {
    ...goRef,
    constraintType: z.enum(["Aim", "Parent", "Position", "Rotation", "Scale", "LookAt"]).optional(),
  }, async (p) => {
    const r = await bridge.request("constraint.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_constraint_find", "Find constraints", {
    constraintType: z.enum(["Aim", "Parent", "Position", "Rotation", "Scale", "LookAt", "All"]).optional(),
  }, async (p) => {
    const r = await bridge.request("constraint.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
