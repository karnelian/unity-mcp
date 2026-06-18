import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

export function registerRenderFeatureTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_renderFeature_list", "List RendererFeatures", {}, async (p) => {
    const r = await bridge.request("renderFeature.list", p);
    return textResult(r);
  });

  server.tool("unity_renderFeature_add", "Add RendererFeature", {
    featureType: z.string(),
    name: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("renderFeature.add", p);
    return textResult(r);
  });

  server.tool("unity_renderFeature_remove", "Remove RendererFeature", {
    name: z.string().optional(),
    index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("renderFeature.remove", p);
    return textResult(r);
  });

  server.tool("unity_renderFeature_setActive", "Set RendererFeature active", {
    name: z.string().optional(),
    index: z.number().optional(),
    active: z.boolean(),
  }, async (p) => {
    const r = await bridge.request("renderFeature.setActive", p);
    return textResult(r);
  });
}
