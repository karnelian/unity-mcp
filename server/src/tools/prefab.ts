import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerPrefabTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_prefab_create", "씬 오브젝트를 Prefab으로 저장합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    savePath: z.string().optional().describe("저장 경로 (기본: Assets/Prefabs/{name}.prefab)"),
  }, async (p) => {
    const r = await bridge.request("prefab.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_instantiate", "Prefab을 씬에 인스턴스화합니다.", {
    prefabPath: z.string().describe("프리팹 에셋 경로"),
    name: z.string().optional(), position: vec3.optional(), rotation: vec3.optional(),
    parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.instantiate", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_instantiate_batch", "여러 Prefab을 한 번에 인스턴스화합니다.", {
    items: z.array(z.object({
      prefabPath: z.string(), name: z.string().optional(),
      position: vec3.optional(), rotation: vec3.optional(), parent: z.string().optional(),
    })).describe("인스턴스 배열"),
  }, async (p) => {
    const r = await bridge.request("prefab.instantiateBatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_apply", "Prefab 인스턴스 변경사항을 원본에 적용합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.apply", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_revert", "Prefab 인스턴스를 원본 상태로 되돌립니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.revert", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_unpack", "Prefab 인스턴스를 일반 오브젝트로 변환합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    completely: z.boolean().optional().describe("완전 언팩 (중첩 프리팹도 해제)"),
  }, async (p) => {
    const r = await bridge.request("prefab.unpack", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_get_overrides", "Prefab 인스턴스의 오버라이드 목록을 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.getOverrides", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_create_variant", "Prefab Variant를 생성합니다.", {
    basePrefabPath: z.string().describe("기본 프리팹 경로"),
    name: z.string().optional(), savePath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.createVariant", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_find", "프로젝트의 Prefab을 검색합니다.", {
    folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
    nameFilter: z.string().optional().describe("이름 필터"),
  }, async (p) => {
    const r = await bridge.request("prefab.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_get_info", "Prefab 에셋 정보를 조회합니다.", {
    prefabPath: z.string().describe("프리팹 경로"),
  }, async (p) => {
    const r = await bridge.request("prefab.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_set_active", "Prefab 인스턴스를 활성화/비활성화합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    active: z.boolean().describe("활성화 여부"),
  }, async (p) => {
    const r = await bridge.request("prefab.setActive", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_getAssetType", "에셋의 Prefab 타입을 확인합니다.", {
    assetPath: z.string().describe("에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("prefab.getAssetType", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_getInstanceStatus", "씬 오브젝트의 Prefab 인스턴스 상태를 확인합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.getInstanceStatus", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_replace", "씬의 오브젝트를 다른 Prefab으로 교체합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    newPrefabPath: z.string().describe("새 프리팹 에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("prefab.replace", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
