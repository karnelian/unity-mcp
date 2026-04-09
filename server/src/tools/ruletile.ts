import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerRuleTileTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_ruleTile_create", "Create RuleTile", {
    path: z.string(),
    defaultSprite: z.string().optional(),
    defaultColliderType: z.enum(["None", "Sprite", "Grid"]).optional(),
  }, async (p) => {
    const r = await bridge.request("ruleTile.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ruleTile_addRule", "Add RuleTile rule", {
    path: z.string(),
    sprite: z.string(),
    neighbors: z.array(z.number()).optional(),
    output: z.enum(["Single", "Random", "Animation"]).optional(),
    colliderType: z.enum(["None", "Sprite", "Grid"]).optional(),
  }, async (p) => {
    const r = await bridge.request("ruleTile.addRule", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ruleTile_getInfo", "Get RuleTile info", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("ruleTile.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ruleTile_find", "Find RuleTiles", {
    nameFilter: z.string().optional(),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ruleTile.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
