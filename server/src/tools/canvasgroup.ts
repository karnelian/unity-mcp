import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerCanvasGroupTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_canvasGroup_add", "Add CanvasGroup", {
    ...goRef,
    alpha: z.number().optional(),
    interactable: z.boolean().optional(),
    blocksRaycasts: z.boolean().optional(),
    ignoreParentGroups: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("canvasGroup.add", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_canvasGroup_set", "Set CanvasGroup", {
    ...goRef,
    alpha: z.number().optional(),
    interactable: z.boolean().optional(),
    blocksRaycasts: z.boolean().optional(),
    ignoreParentGroups: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("canvasGroup.set", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_canvasGroup_find", "Find CanvasGroups", {}, async (p) => {
    const r = await bridge.request("canvasGroup.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
