import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const quat = z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number() });

export function registerSplineTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_spline_create", "Create Spline", {
    name: z.string().optional(),
    position: vec3.optional(),
    closed: z.boolean().optional().describe("닫힌 스플라인 (기본: false)"),
    knots: z.array(vec3).optional(),
  }, async (p) => {
    const r = await bridge.request("spline.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_getInfo", "Get Spline info", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_addKnot", "Add spline knot", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number(), y: z.number(), z: z.number(),
    index: z.number().optional(),
    tangentIn: vec3.optional(), tangentOut: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("spline.addKnot", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_removeKnot", "Remove spline knot", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    index: z.number(),
  }, async (p) => {
    const r = await bridge.request("spline.removeKnot", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_setKnot", "Set spline knot", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    index: z.number(),
    position: vec3.optional(), tangentIn: vec3.optional(), tangentOut: vec3.optional(),
    rotation: quat.optional(),
  }, async (p) => {
    const r = await bridge.request("spline.setKnot", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_getKnots", "Get spline knots", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.getKnots", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_setTangentMode", "Set tangent mode", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    index: z.number(),
    mode: z.enum(["AutoSmooth", "Linear", "Broken"]).describe("탄젠트 모드"),
  }, async (p) => {
    const r = await bridge.request("spline.setTangentMode", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_find", "Find Splines", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_addExtrude", "Add Spline Extrude", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    radius: z.number().optional(), segments: z.number().optional(),
    sides: z.number().optional(), capped: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.addExtrude", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_addAnimate", "Add Spline Animate", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    duration: z.number().optional(),
    loop: z.enum(["Once", "Loop", "PingPong"]).optional(),
    easingMode: z.enum(["None", "EaseIn", "EaseOut", "EaseInOut"]).optional(),
  }, async (p) => {
    const r = await bridge.request("spline.addAnimate", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_addInstantiate", "Add Spline Instantiate", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    prefabPath: z.string().optional(),
    spacing: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.addInstantiate", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
