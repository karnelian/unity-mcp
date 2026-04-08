import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const vec2 = z.object({ x: z.number(), y: z.number() });

export function registerTerrainTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_terrain_create", "Terrain을 생성합니다.", {
    name: z.string().optional(), width: z.number().optional(), height: z.number().optional(),
    length: z.number().optional(), heightmapResolution: z.number().optional(),
    position: vec3.optional(), savePath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_get_info", "Terrain 정보를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_set_height", "특정 영역의 높이를 설정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    x: z.number().describe("하이트맵 X 좌표"), y: z.number().describe("하이트맵 Y 좌표"),
    height: z.number().describe("높이값 (0~1 정규화)"), radius: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.setHeight", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_get_height", "월드 좌표에서 높이를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    worldX: z.number(), worldZ: z.number(),
  }, async (p) => {
    const r = await bridge.request("terrain.getHeight", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_flatten", "Terrain을 평탄화합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    height: z.number().optional().describe("높이값 (기본: 0)"),
  }, async (p) => {
    const r = await bridge.request("terrain.flatten", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_perlin_noise", "Perlin Noise로 지형을 생성합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    scale: z.number().optional().describe("노이즈 스케일 (기본: 20)"),
    amplitude: z.number().optional().describe("진폭 (기본: 0.1)"),
    offsetX: z.number().optional(), offsetY: z.number().optional(),
    additive: z.boolean().optional().describe("기존 높이에 더하기"),
  }, async (p) => {
    const r = await bridge.request("terrain.perlinNoise", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_smooth", "Terrain을 부드럽게 합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    iterations: z.number().optional().describe("반복 횟수 (기본: 1)"),
  }, async (p) => {
    const r = await bridge.request("terrain.smooth", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_add_layer", "Terrain Layer(텍스처)를 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    texturePath: z.string().describe("텍스처 경로"),
    normalMapPath: z.string().optional(), tileSize: vec2.optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.addLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_paint_texture", "Terrain에 텍스처를 페인팅합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    layerIndex: z.number().describe("레이어 인덱스"), x: z.number(), y: z.number(),
    radius: z.number().optional(), opacity: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.paintTexture", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_set_size", "Terrain 크기를 변경합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    width: z.number().optional(), height: z.number().optional(), length: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.setSize", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_addTree", "터레인에 나무를 추가합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    prefabPath: z.string().describe("트리 프리팹 에셋 경로"),
    x: z.number().optional().describe("정규화 X (0~1)"),
    y: z.number().optional().describe("높이 (0~1)"),
    z: z.number().optional().describe("정규화 Z (0~1)"),
    widthScale: z.number().optional(), heightScale: z.number().optional(),
    positions: z.array(z.object({
      x: z.number(), y: z.number().optional(), z: z.number(),
      widthScale: z.number().optional(), heightScale: z.number().optional(),
    })).optional().describe("여러 위치에 나무 추가"),
  }, async (p) => {
    const r = await bridge.request("terrain.addTree", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_removeTree", "터레인의 나무를 제거합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    prototypeIndex: z.number().optional().describe("특정 프로토타입 인덱스만 제거 (미지정 시 전체 제거)"),
  }, async (p) => {
    const r = await bridge.request("terrain.removeTree", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_getTreePrototypes", "터레인 트리 프로토타입을 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.getTreePrototypes", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_setTreePrototypes", "터레인 트리 프로토타입을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    prototypes: z.array(z.object({
      prefabPath: z.string().describe("프리팹 경로"),
    })).describe("트리 프로토타입 목록"),
  }, async (p) => {
    const r = await bridge.request("terrain.setTreePrototypes", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_setDetailLayer", "터레인 디테일(풀/꽃) 밀도를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    layerIndex: z.number().optional().describe("디테일 레이어 인덱스 (기본: 0)"),
    x: z.number().describe("X 좌표"), y: z.number().describe("Y 좌표"),
    radius: z.number().optional().describe("반경 (기본: 5)"),
    density: z.number().optional().describe("밀도 (기본: 1)"),
  }, async (p) => {
    const r = await bridge.request("terrain.setDetailLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_getDetailPrototypes", "터레인 디테일 프로토타입을 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("terrain.getDetailPrototypes", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_setHoles", "터레인에 구멍을 만들거나 제거합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number().describe("X 좌표"), y: z.number().describe("Y 좌표"),
    radius: z.number().optional().describe("반경 (기본: 1)"),
    isHole: z.boolean().optional().describe("구멍 여부 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("terrain.setHoles", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_terrain_getSteepness", "터레인의 경사도와 법선을 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number().describe("정규화 X (0~1)"),
    y: z.number().describe("정규화 Y (0~1)"),
  }, async (p) => {
    const r = await bridge.request("terrain.getSteepness", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
