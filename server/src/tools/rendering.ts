import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });

export function registerRenderingTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_rendering_createVolume", "Create Volume", {
    name: z.string().optional(),
    isGlobal: z.boolean().optional().describe("전역 볼륨 (기본: true)"),
    priority: z.number().optional(),
    weight: z.number().optional(),
    profilePath: z.string().optional(),
    position: vec3.optional(),
    size: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.createVolume", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_getVolumeInfo", "Get Volume info", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.getVolumeInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_findVolumes", "Find Volumes", {}, async () => {
    const r = await bridge.request("rendering.findVolumes", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_addOverride", "Add volume override", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    overrideType: z.string().describe("오버라이드 타입 (예: Bloom, ColorAdjustments, Vignette, Tonemapping 등)"),
  }, async (p) => {
    const r = await bridge.request("rendering.addOverride", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_removeOverride", "Remove volume override", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    overrideType: z.string(),
  }, async (p) => {
    const r = await bridge.request("rendering.removeOverride", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_setOverrideProperty", "Set override property", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    overrideType: z.string().describe("오버라이드 타입 (예: Bloom)"),
    property: z.string().describe("속성 이름 (예: intensity, threshold, scatter)"),
    value: z.any(),
  }, async (p) => {
    const r = await bridge.request("rendering.setOverrideProperty", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_getOverrides", "Get volume overrides", {}, async () => {
    const r = await bridge.request("rendering.getOverrides", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_getPipelineInfo", "Get pipeline info", {}, async () => {
    const r = await bridge.request("rendering.getPipelineInfo", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_setFog", "Set fog settings", {
    enabled: z.boolean().optional(),
    color: color.optional(),
    mode: z.enum(["Linear", "Exponential", "ExponentialSquared"]).optional(),
    density: z.number().optional(),
    startDistance: z.number().optional(),
    endDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.setFog", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_getFog", "Get fog settings", {}, async () => {
    const r = await bridge.request("rendering.getFog", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_setSkybox", "Set skybox", {
    materialPath: z.string().optional(),
    ambientIntensity: z.number().optional(),
    reflectionIntensity: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.setSkybox", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_setGlobalShaderProperty", "Set global shader property", {
    property: z.string(),
    floatValue: z.number().optional(),
    intValue: z.number().optional(),
    colorValue: color.optional(),
    vectorValue: z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number().optional() }).optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.setGlobalShaderProperty", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
