import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerOcclusionCullingTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_occlusionCulling_bake", "Bake occlusion", {
    smallestOccluder: z.number().optional(),
    smallestHole: z.number().optional(),
    backfaceThreshold: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("occlusionCulling.bake", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_occlusionCulling_clear", "Clear occlusion data", {}, async (p) => {
    const r = await bridge.request("occlusionCulling.clear", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_occlusionCulling_getSettings", "Get occlusion settings", {}, async (p) => {
    const r = await bridge.request("occlusionCulling.getSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_occlusionCulling_setArea", "Set OcclusionArea", {
    ...goRef,
    center: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    size: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    isViewVolume: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("occlusionCulling.setArea", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
