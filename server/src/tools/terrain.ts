import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const vec2 = z.object({ x: z.number(), y: z.number() });

export function registerTerrainTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_terrain_create", "Create Terrain", {
    name: z.string().optional(), width: z.number().optional(), height: z.number().optional(),
    length: z.number().optional(), heightmapResolution: z.number().optional(),
    position: vec3.optional(), savePath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_get_info", "Get Terrain info", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_set_height", "Set terrain height", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    x: z.number(), y: z.number(),
    height: z.number(), radius: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.setHeight", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_get_height", "Get terrain height", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    worldX: z.number(), worldZ: z.number(),
  }, async (p) => {
    const r = await bridge.request("terrain.getHeight", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_flatten", "Flatten terrain", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    height: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.flatten", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_perlin_noise", "Apply perlin noise", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    scale: z.number().optional(),
    amplitude: z.number().optional(),
    offsetX: z.number().optional(), offsetY: z.number().optional(),
    additive: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.perlinNoise", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_smooth", "Smooth terrain", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    iterations: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.smooth", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_add_layer", "Add terrain layer", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    texturePath: z.string(),
    normalMapPath: z.string().optional(), tileSize: vec2.optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.addLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_paint_texture", "Paint terrain texture", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    layerIndex: z.number(), x: z.number(), y: z.number(),
    radius: z.number().optional(), opacity: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.paintTexture", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_set_size", "Set terrain size", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    width: z.number().optional(), height: z.number().optional(), length: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.setSize", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_addTree", "Add tree", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    prefabPath: z.string(),
    x: z.number().optional(),
    y: z.number().optional(),
    z: z.number().optional(),
    widthScale: z.number().optional(), heightScale: z.number().optional(),
    positions: z.array(z.object({
      x: z.number(), y: z.number().optional(), z: z.number(),
      widthScale: z.number().optional(), heightScale: z.number().optional(),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.addTree", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_removeTree", "Remove trees", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    prototypeIndex: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.removeTree", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_getTreePrototypes", "Get tree prototypes", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.getTreePrototypes", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_setTreePrototypes", "Set tree prototypes", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    prototypes: z.array(z.object({
      prefabPath: z.string(),
    })),
  }, async (p) => {
    const r = await bridge.request("terrain.setTreePrototypes", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_setDetailLayer", "Set detail layer", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    layerIndex: z.number().optional(),
    x: z.number(), y: z.number(),
    radius: z.number().optional(),
    density: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.setDetailLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_getDetailPrototypes", "Get detail prototypes", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.getDetailPrototypes", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_setHoles", "Set terrain holes", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number(), y: z.number(),
    radius: z.number().optional(),
    isHole: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.setHoles", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_getSteepness", "Get terrain steepness", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number(),
    y: z.number(),
  }, async (p) => {
    const r = await bridge.request("terrain.getSteepness", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
