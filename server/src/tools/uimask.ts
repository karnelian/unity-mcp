import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerUIMaskTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_uiMask_addMask", "Add a Mask component to a UI GameObject.", {
    ...goRef,
    showMaskGraphic: z.boolean().optional().describe("Show the mask graphic (default: true)"),
  }, async (p) => {
    const r = await bridge.request("uiMask.addMask", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uiMask_addRectMask2D", "Add a RectMask2D component to a UI GameObject.", {
    ...goRef,
    softness: z.object({ x: z.number(), y: z.number() }).optional().describe("Mask softness"),
    padding: z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number() }).optional(),
  }, async (p) => {
    const r = await bridge.request("uiMask.addRectMask2D", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uiMask_set", "Set Mask or RectMask2D properties.", {
    ...goRef,
    showMaskGraphic: z.boolean().optional(),
    softness: z.object({ x: z.number(), y: z.number() }).optional(),
  }, async (p) => {
    const r = await bridge.request("uiMask.set", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
