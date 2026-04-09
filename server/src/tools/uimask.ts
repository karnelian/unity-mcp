import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerUIMaskTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_uiMask_addMask", "Add Mask", {
    ...goRef,
    showMaskGraphic: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("uiMask.addMask", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uiMask_addRectMask2D", "Add RectMask2D", {
    ...goRef,
    softness: z.object({ x: z.number(), y: z.number() }).optional(),
    padding: z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number() }).optional(),
  }, async (p) => {
    const r = await bridge.request("uiMask.addRectMask2D", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uiMask_set", "Set mask properties", {
    ...goRef,
    showMaskGraphic: z.boolean().optional(),
    softness: z.object({ x: z.number(), y: z.number() }).optional(),
  }, async (p) => {
    const r = await bridge.request("uiMask.set", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
