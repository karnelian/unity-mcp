import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerRenderFeatureTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_renderFeature_list", "List all Renderer Features on the active URP renderer.", {}, async (p) => {
    const r = await bridge.request("renderFeature.list", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderFeature_add", "Add a Renderer Feature to the active URP renderer.", {
    featureType: z.string().describe("Feature type name (e.g., ScreenSpaceAmbientOcclusion, DecalRendererFeature)"),
    name: z.string().optional().describe("Display name"),
  }, async (p) => {
    const r = await bridge.request("renderFeature.add", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderFeature_remove", "Remove a Renderer Feature by name or index.", {
    name: z.string().optional(),
    index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("renderFeature.remove", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderFeature_setActive", "Enable or disable a Renderer Feature.", {
    name: z.string().optional(),
    index: z.number().optional(),
    active: z.boolean().describe("Enable or disable"),
  }, async (p) => {
    const r = await bridge.request("renderFeature.setActive", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
