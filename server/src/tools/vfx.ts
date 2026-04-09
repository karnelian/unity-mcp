import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec4 = z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number().optional() });
const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerVFXTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_vfx_create", "Create VFX", {
    name: z.string().optional(),
    position: vec3.optional(),
    assetPath: z.string().optional(),
    playOnAwake: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("vfx.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_getInfo", "Get VFX info", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("vfx.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_play", "Play VFX", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    eventName: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("vfx.play", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_stop", "Stop VFX", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    eventName: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("vfx.stop", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_setFloat", "Set VFX float", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    property: z.string(),
    value: z.number(),
  }, async (p) => {
    const r = await bridge.request("vfx.setFloat", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_setInt", "Set VFX int", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    property: z.string(),
    value: z.number(),
  }, async (p) => {
    const r = await bridge.request("vfx.setInt", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_setBool", "Set VFX bool", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    property: z.string(),
    value: z.boolean(),
  }, async (p) => {
    const r = await bridge.request("vfx.setBool", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_setVector", "Set VFX vector", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    property: z.string(),
    value: vec4,
  }, async (p) => {
    const r = await bridge.request("vfx.setVector", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_find", "Find VFX objects", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("vfx.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
