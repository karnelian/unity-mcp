import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerProBuilderTools(server: McpServer, bridge: UnityBridge) {

  const goId = {
    path: z.string().optional().describe("오브젝트 경로"),
    name: z.string().optional().describe("오브젝트 이름"),
    instanceId: z.number().optional().describe("인스턴스 ID"),
  };

  server.tool("unity_probuilder_createCube", "ProBuilder Cube를 생성합니다. (com.unity.probuilder 필요)", {
    name: z.string().optional(), size: vec3.optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createCube", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createCylinder", "ProBuilder Cylinder를 생성합니다.", {
    name: z.string().optional(), radius: z.number().optional(), height: z.number().optional(),
    axisDivisions: z.number().optional(), heightCuts: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createCylinder", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createSphere", "ProBuilder Sphere(Icosahedron)를 생성합니다.", {
    name: z.string().optional(), radius: z.number().optional(), subdivisions: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createSphere", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createPlane", "ProBuilder Plane을 생성합니다.", {
    name: z.string().optional(), width: z.number().optional(), height: z.number().optional(),
    widthCuts: z.number().optional(), heightCuts: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createPlane", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createStair", "ProBuilder 계단을 생성합니다.", {
    name: z.string().optional(), width: z.number().optional(), height: z.number().optional(),
    depth: z.number().optional(), steps: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createStair", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createArch", "ProBuilder 아치를 생성합니다.", {
    name: z.string().optional(), angle: z.number().optional(), radius: z.number().optional(),
    width: z.number().optional(), depth: z.number().optional(), radialCuts: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createArch", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createDoor", "ProBuilder 문을 생성합니다.", {
    name: z.string().optional(), totalWidth: z.number().optional(), totalHeight: z.number().optional(),
    depth: z.number().optional(), doorWidth: z.number().optional(), doorHeight: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createDoor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createPipe", "ProBuilder 파이프를 생성합니다.", {
    name: z.string().optional(), radius: z.number().optional(), height: z.number().optional(),
    thickness: z.number().optional(), subdivisions: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createPipe", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_getInfo", "ProBuilder 메시 정보를 조회합니다.", goId, async (params) => {
    const result = await bridge.request("probuilder.getInfo", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_extrude", "ProBuilder 메시의 면을 돌출합니다.", {
    ...goId, distance: z.number().optional().describe("돌출 거리 (기본: 0.5)"),
    faceIndices: z.array(z.number()).optional().describe("면 인덱스 (생략 시 전체)"),
  }, async (params) => {
    const result = await bridge.request("probuilder.extrude", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_bevel", "ProBuilder 메시의 엣지를 베벨합니다.", {
    ...goId, amount: z.number().optional().describe("베벨 크기 (기본: 0.1)"),
  }, async (params) => {
    const result = await bridge.request("probuilder.bevel", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_subdivide", "ProBuilder 메시를 세분화합니다.", goId, async (params) => {
    const result = await bridge.request("probuilder.subdivide", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_merge", "여러 ProBuilder 메시를 병합합니다.", {
    names: z.array(z.string()).optional(), paths: z.array(z.string()).optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.merge", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_flip", "ProBuilder 메시의 노멀을 뒤집습니다.", goId, async (params) => {
    const result = await bridge.request("probuilder.flip", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_setMaterial", "ProBuilder 면에 Material을 설정합니다.", {
    ...goId, materialPath: z.string().describe("머티리얼 경로"),
    faceIndices: z.array(z.number()).optional().describe("면 인덱스 (생략 시 전체)"),
  }, async (params) => {
    const result = await bridge.request("probuilder.setMaterial", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_findProBuilderObjects", "씬의 모든 ProBuilder 오브젝트를 찾습니다.", {}, async (params) => {
    const result = await bridge.request("probuilder.findProBuilderObjects", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_centerPivot", "ProBuilder 메시의 피봇을 중앙으로 이동합니다.", goId, async (params) => {
    const result = await bridge.request("probuilder.centerPivot", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_freezeTransform", "ProBuilder Transform을 프리즈합니다.", goId, async (params) => {
    const result = await bridge.request("probuilder.freezeTransform", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_triangulate", "ProBuilder 메시를 삼각형화합니다.", goId, async (params) => {
    const result = await bridge.request("probuilder.triangulate", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_export", "ProBuilder 메시를 에셋으로 내보냅니다.", {
    ...goId, format: z.string().optional().describe("포맷 (기본: obj)"),
    savePath: z.string().optional().describe("저장 경로"),
  }, async (params) => {
    const result = await bridge.request("probuilder.export", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_selectFaces", "ProBuilder 면을 선택합니다.", {
    ...goId, faceIndices: z.array(z.number()).describe("선택할 면 인덱스"),
  }, async (params) => {
    const result = await bridge.request("probuilder.selectFaces", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_deleteFaces", "ProBuilder 면을 삭제합니다.", {
    ...goId, faceIndices: z.array(z.number()).describe("삭제할 면 인덱스"),
  }, async (params) => {
    const result = await bridge.request("probuilder.deleteFaces", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });
}
