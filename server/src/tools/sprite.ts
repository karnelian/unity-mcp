import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });

export function registerSpriteTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_sprite_create", "Create SpriteRenderer", {
    name: z.string().optional(),
    position: vec3.optional(),
    spritePath: z.string().optional(),
    color: color.optional(),
    sortingOrder: z.number().optional(),
    sortingLayerName: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("sprite.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sprite_setProperties", "Set SpriteRenderer properties", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    spritePath: z.string().optional(),
    color: color.optional(),
    flipX: z.boolean().optional(), flipY: z.boolean().optional(),
    drawMode: z.enum(["Simple", "Sliced", "Tiled"]).optional(),
    sortingOrder: z.number().optional(),
    sortingLayerName: z.string().optional(),
    materialPath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("sprite.setProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sprite_find", "Find SpriteRenderers", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("sprite.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sprite_setSortingOrder", "Set sorting order", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    sortingOrder: z.number().optional(),
    sortingLayerName: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("sprite.setSortingOrder", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
