import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

export function registerSortingLayerTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_sortingLayer_create", "Create sorting layer", {
    name: z.string(),
  }, async (p) => {
    const r = await bridge.request("sortingLayer.create", p);
    return textResult(r);
  });

  server.tool("unity_sortingLayer_delete", "Delete sorting layer", {
    name: z.string(),
  }, async (p) => {
    const r = await bridge.request("sortingLayer.delete", p);
    return textResult(r);
  });

  server.tool("unity_sortingLayer_reorder", "Reorder sorting layers", {
    layers: z.array(z.string()),
  }, async (p) => {
    const r = await bridge.request("sortingLayer.reorder", p);
    return textResult(r);
  });

  server.tool("unity_sortingLayer_list", "List sorting layers", {}, async (p) => {
    const r = await bridge.request("sortingLayer.list", p);
    return textResult(r);
  });
}
