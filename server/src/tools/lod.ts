import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerLODTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_lod_add", "Add LODGroup", {
    ...goRef,
    levels: z.array(z.object({
      screenRelativeHeight: z.number().describe("Screen height ratio (0-1) for LOD transition"),
      renderers: z.array(z.string()).optional(),
    })).optional(),
    fadeMode: z.enum(["None", "CrossFade", "SpeedTree"]).optional(),
    animateCrossFading: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("lod.add", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_getInfo", "Get LODGroup info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("lod.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_setLevels", "Set LOD levels", {
    ...goRef,
    levels: z.array(z.object({
      screenRelativeHeight: z.number(),
      renderers: z.array(z.string()).optional(),
    })),
  }, async (p) => {
    const r = await bridge.request("lod.setLevels", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_setTransition", "Set LOD transitions", {
    ...goRef,
    fadeMode: z.enum(["None", "CrossFade", "SpeedTree"]).optional(),
    animateCrossFading: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("lod.setTransition", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_find", "Find LODGroups", {}, async (p) => {
    const r = await bridge.request("lod.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lod_remove", "Remove LODGroup", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("lod.remove", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
