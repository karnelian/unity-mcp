import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerUnitySearchTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_search_assets", "Search assets (Unity Search)", {
    query: z.string(),
    maxResults: z.number().optional(),
    provider: z.enum(["asset", "scene", "project", "all"]).optional(),
  }, async (p) => {
    const r = await bridge.request("unitySearch.assets", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_search_scene", "Search scene (Unity Search)", {
    query: z.string(),
    maxResults: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("unitySearch.scene", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_search_menu", "Search menu items", {
    query: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("unitySearch.menu", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
