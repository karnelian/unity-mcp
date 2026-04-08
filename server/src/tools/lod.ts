import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerLODTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_lod_add", "Add a LODGroup component to a GameObject.", {
    ...goRef,
    levels: z.array(z.object({
      screenRelativeHeight: z.number().describe("Screen height ratio (0-1) for LOD transition"),
      renderers: z.array(z.string()).optional().describe("Renderer GameObject names for this level"),
    })).optional().describe("LOD levels configuration"),
    fadeMode: z.enum(["None", "CrossFade", "SpeedTree"]).optional(),
    animateCrossFading: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("lod.add", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_getInfo", "Get LODGroup information from a GameObject.", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("lod.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_setLevels", "Set LOD levels configuration.", {
    ...goRef,
    levels: z.array(z.object({
      screenRelativeHeight: z.number(),
      renderers: z.array(z.string()).optional(),
    })).describe("LOD levels"),
  }, async (p) => {
    const r = await bridge.request("lod.setLevels", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_setTransition", "Set LOD transition percentages.", {
    ...goRef,
    fadeMode: z.enum(["None", "CrossFade", "SpeedTree"]).optional(),
    animateCrossFading: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("lod.setTransition", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_find", "Find all GameObjects with LODGroup in the scene.", {}, async (p) => {
    const r = await bridge.request("lod.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_remove", "Remove LODGroup from a GameObject.", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("lod.remove", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
