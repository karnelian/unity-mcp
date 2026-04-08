import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerUnitySearchTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_search_assets", "Search assets using Unity's Search API with advanced queries.", {
    query: z.string().describe("Search query (supports Unity Search syntax: t:, ref:, dep:, glob:, etc.)"),
    maxResults: z.number().optional().describe("Max results (default: 100)"),
    provider: z.enum(["asset", "scene", "project", "all"]).optional(),
  }, async (p) => {
    const r = await bridge.request("unitySearch.assets", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_search_scene", "Search scene objects using Unity's Search API.", {
    query: z.string().describe("Search query"),
    maxResults: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("unitySearch.scene", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_search_menu", "Search and list available menu items.", {
    query: z.string().optional().describe("Menu item filter"),
  }, async (p) => {
    const r = await bridge.request("unitySearch.menu", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
