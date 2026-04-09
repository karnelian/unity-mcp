import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerLightmappingTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_lightmapping_bake", "Start lightmap bake", {
    async: z.boolean().optional().describe("Use async baking (default: true)"),
  }, async (p) => {
    const r = await bridge.request("lightmapping.bake", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lightmapping_cancel", "Cancel lightmap bake", {}, async (p) => {
    const r = await bridge.request("lightmapping.cancel", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lightmapping_clear", "Clear lightmap data", {}, async (p) => {
    const r = await bridge.request("lightmapping.clear", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lightmapping_getSettings", "Get lightmap settings", {}, async (p) => {
    const r = await bridge.request("lightmapping.getSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lightmapping_setSettings", "Set lightmap settings", {
    lightmapper: z.enum(["Enlighten", "ProgressiveGPU", "ProgressiveCPU"]).optional(),
    directSampleCount: z.number().optional(),
    indirectSampleCount: z.number().optional(),
    environmentSampleCount: z.number().optional(),
    bounces: z.number().optional(),
    lightmapResolution: z.number().optional(),
    lightmapPadding: z.number().optional(),
    compressLightmaps: z.boolean().optional(),
    ambientOcclusion: z.boolean().optional(),
    aoMaxDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("lightmapping.setSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lightmapping_getProgress", "Get bake progress", {}, async (p) => {
    const r = await bridge.request("lightmapping.getProgress", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
