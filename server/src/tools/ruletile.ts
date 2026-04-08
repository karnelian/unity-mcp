import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerRuleTileTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_ruleTile_create", "Create a RuleTile asset.", {
    path: z.string().describe("Asset path (e.g., Assets/Tiles/MyRuleTile.asset)"),
    defaultSprite: z.string().optional().describe("Default sprite asset path"),
    defaultColliderType: z.enum(["None", "Sprite", "Grid"]).optional(),
  }, async (p) => {
    const r = await bridge.request("ruleTile.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ruleTile_addRule", "Add a tiling rule to a RuleTile.", {
    path: z.string().describe("RuleTile asset path"),
    sprite: z.string().describe("Sprite asset path for this rule"),
    neighbors: z.array(z.number()).optional().describe("Neighbor rule array (0=DontCare, 1=This, 2=NotThis)"),
    output: z.enum(["Single", "Random", "Animation"]).optional(),
    colliderType: z.enum(["None", "Sprite", "Grid"]).optional(),
  }, async (p) => {
    const r = await bridge.request("ruleTile.addRule", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ruleTile_getInfo", "Get RuleTile information.", {
    path: z.string().describe("RuleTile asset path"),
  }, async (p) => {
    const r = await bridge.request("ruleTile.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ruleTile_find", "Find all RuleTile assets in the project.", {
    nameFilter: z.string().optional(),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ruleTile.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
