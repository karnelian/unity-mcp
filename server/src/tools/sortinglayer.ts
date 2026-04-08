import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerSortingLayerTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_sortingLayer_create", "Create a new sorting layer.", {
    name: z.string().describe("Sorting layer name"),
  }, async (p) => {
    const r = await bridge.request("sortingLayer.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sortingLayer_delete", "Delete a sorting layer.", {
    name: z.string().describe("Sorting layer name to delete"),
  }, async (p) => {
    const r = await bridge.request("sortingLayer.delete", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sortingLayer_reorder", "Reorder sorting layers.", {
    layers: z.array(z.string()).describe("Ordered array of sorting layer names"),
  }, async (p) => {
    const r = await bridge.request("sortingLayer.reorder", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sortingLayer_list", "List all sorting layers.", {}, async (p) => {
    const r = await bridge.request("sortingLayer.list", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
