import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });

export function registerTilemap2DTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_tilemap_create", "Create Tilemap", {
    name: z.string().optional(),
    cellSize: z.number().optional(),
    position: vec3.optional(),
    sortingLayerName: z.string().optional(),
    sortingOrder: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_setTile", "Set tile", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number(), y: z.number(),
    tilePath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.setTile", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_setTilesBatch", "Batch set tiles", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    tiles: z.array(z.object({
      x: z.number(), y: z.number(), tilePath: z.string().optional(),
    })),
  }, async (p) => {
    const r = await bridge.request("tilemap.setTilesBatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_clearTiles", "Clear tiles", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    bounds: z.object({
      xMin: z.number(), yMin: z.number(), xMax: z.number(), yMax: z.number(),
    }).optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.clearTiles", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_getTile", "Get tile info", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number(), y: z.number(),
  }, async (p) => {
    const r = await bridge.request("tilemap.getTile", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_getInfo", "Get Tilemap info", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_setRenderer", "Set Tilemap renderer", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    sortingLayerName: z.string().optional(),
    sortingOrder: z.number().optional(),
    mode: z.enum(["Chunk", "Individual"]).optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.setRenderer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_find", "Find Tilemaps", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

}
