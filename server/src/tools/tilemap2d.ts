import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });

export function registerTilemap2DTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_tilemap_create", "Tilemap(Grid + Tilemap)을 생성합니다.", {
    name: z.string().optional().describe("이름 (기본: Tilemap)"),
    cellSize: z.number().optional().describe("셀 크기 (기본: 1)"),
    position: vec3.optional(),
    sortingLayerName: z.string().optional(),
    sortingOrder: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_setTile", "타일맵에 타일을 배치합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number().describe("그리드 X"), y: z.number().describe("그리드 Y"),
    tilePath: z.string().optional().describe("타일 에셋 경로 (미지정 시 타일 제거)"),
  }, async (p) => {
    const r = await bridge.request("tilemap.setTile", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_setTilesBatch", "여러 타일을 한번에 배치합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    tiles: z.array(z.object({
      x: z.number(), y: z.number(), tilePath: z.string().optional(),
    })).describe("타일 배열"),
  }, async (p) => {
    const r = await bridge.request("tilemap.setTilesBatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_clearTiles", "타일을 지웁니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    bounds: z.object({
      xMin: z.number(), yMin: z.number(), xMax: z.number(), yMax: z.number(),
    }).optional().describe("영역 (미지정 시 전체 클리어)"),
  }, async (p) => {
    const r = await bridge.request("tilemap.clearTiles", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_getTile", "특정 위치의 타일 정보를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number().describe("그리드 X"), y: z.number().describe("그리드 Y"),
  }, async (p) => {
    const r = await bridge.request("tilemap.getTile", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_getInfo", "타일맵 정보를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_setRenderer", "타일맵 렌더러를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    sortingLayerName: z.string().optional(),
    sortingOrder: z.number().optional(),
    mode: z.enum(["Chunk", "Individual"]).optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.setRenderer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tilemap_find", "씬의 타일맵을 검색합니다.", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("tilemap.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

}
