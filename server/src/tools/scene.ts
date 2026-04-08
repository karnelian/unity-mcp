import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerSceneTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_scene_hierarchy",
    "Get scene hierarchy tree. Filter by name or path. Use this first to understand the scene structure before making changes.",
    {
      path: z.string().optional().describe("특정 경로 하위만 조회 (예: 'Environment/Trees')"),
      depth: z.number().optional().describe("조회 깊이 (-1=전체, 기본: -1)"),
      includeComponents: z.boolean().optional().describe("컴포넌트 목록 포함 여부"),
      nameFilter: z.string().optional().describe("이름 필터 (대소문자 무시 부분매칭)"),
    },
    async (params) => {
      const result = await bridge.request("scene.hierarchy", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_create",
    "Create a new GameObject — empty, primitive (Cube/Sphere/Capsule/Cylinder/Plane/Quad), or prefab instance. Set position, rotation, scale, and parent.",
    {
      name: z.string().describe("오브젝트 이름"),
      type: z.enum(["empty", "primitive", "prefab"]).optional().describe("생성 타입 (기본: empty)"),
      primitiveType: z.enum(["Cube", "Sphere", "Capsule", "Cylinder", "Plane", "Quad"]).optional().describe("프리미티브 타입 (type=primitive일 때)"),
      prefabPath: z.string().optional().describe("프리팹 에셋 경로 (type=prefab일 때)"),
      parent: z.string().optional().describe("부모 오브젝트 경로"),
      position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional().describe("초기 위치"),
    },
    async (params) => {
      const result = await bridge.request("scene.create", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_setTransform",
    "GameObject의 위치/회전/스케일을 수정합니다. path, name, instanceId 중 하나로 대상 지정.",
    {
      path: z.string().optional().describe("오브젝트 경로 (예: 'Player/Camera')"),
      name: z.string().optional().describe("오브젝트 이름으로 검색"),
      instanceId: z.number().optional().describe("인스턴스 ID로 검색 (가장 정확)"),
      position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
      rotation: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional().describe("오일러 각도"),
      scale: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
      space: z.enum(["world", "local"]).optional().describe("좌표계 (기본: world)"),
    },
    async (params) => {
      const result = await bridge.request("scene.setTransform", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_addComponent",
    "GameObject에 컴포넌트를 추가합니다. path, name, instanceId 중 하나로 대상 지정.",
    {
      path: z.string().optional().describe("오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      componentType: z.string().describe("컴포넌트 타입 (예: 'Rigidbody', 'BoxCollider', 커스텀 스크립트명)"),
    },
    async (params) => {
      const result = await bridge.request("scene.addComponent", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_setComponent",
    "컴포넌트의 프로퍼티를 수정합니다. path, name, instanceId 중 하나로 대상 지정.",
    {
      path: z.string().optional().describe("오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      componentType: z.string().describe("컴포넌트 타입"),
      properties: z.record(z.string(), z.any()).describe("설정할 프로퍼티 (key-value)"),
    },
    async (params) => {
      const result = await bridge.request("scene.setComponent", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_delete",
    "GameObject를 삭제합니다. Undo 지원. path, name, instanceId 중 하나로 대상 지정.",
    {
      path: z.string().optional().describe("삭제할 오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
    },
    async (params) => {
      const result = await bridge.request("scene.delete", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_duplicate",
    "GameObject를 복제합니다. 배열 배치 가능. path, name, instanceId 중 하나로 대상 지정.",
    {
      path: z.string().optional().describe("복제할 오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      count: z.number().optional().describe("복제 수 (기본: 1)"),
      offset: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional().describe("복제본 간 간격"),
    },
    async (params) => {
      const result = await bridge.request("scene.duplicate", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_manage",
    "씬을 저장/열기/생성하거나 현재 씬 정보를 조회합니다.",
    {
      action: z.enum(["save", "open", "create", "info"]).describe("수행할 작업"),
      scenePath: z.string().optional().describe("씬 파일 경로 (save/open/create 시)"),
    },
    async (params) => {
      const result = await bridge.request("scene.manage", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_find",
    "조건으로 씬의 오브젝트를 검색합니다.",
    {
      name: z.string().optional().describe("이름 검색 (부분매칭)"),
      tag: z.string().optional().describe("태그 필터"),
      layer: z.string().optional().describe("레이어 필터"),
      componentType: z.string().optional().describe("컴포넌트 타입 필터"),
    },
    async (params) => {
      const result = await bridge.request("scene.find", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_select",
    "에디터에서 오브젝트를 선택하고 Scene View에서 포커스합니다. path, name, instanceId 중 하나로 대상 지정.",
    {
      path: z.string().optional().describe("선택할 오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      ping: z.boolean().optional().describe("Hierarchy에서 하이라이트 (기본: true)"),
      frame: z.boolean().optional().describe("Scene View에서 포커스 (기본: true)"),
    },
    async (params) => {
      const result = await bridge.request("scene.select", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool("unity_scene_listLoaded", "현재 로드된 모든 씬을 조회합니다.", {}, async () => {
    const r = await bridge.request("scene.listLoaded", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_setActive", "활성 씬을 변경합니다.", {
    sceneName: z.string().optional().describe("씬 이름"),
    scenePath: z.string().optional().describe("씬 경로"),
  }, async (p) => {
    const r = await bridge.request("scene.setActiveScene", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_moveToScene", "루트 오브젝트를 다른 씬으로 이동합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    targetScene: z.string().optional().describe("대상 씬 이름"),
    targetScenePath: z.string().optional().describe("대상 씬 경로"),
  }, async (p) => {
    const r = await bridge.request("scene.moveToScene", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_openAdditive", "씬을 추가로 엽니다 (멀티씬 편집).", {
    scenePath: z.string().describe("씬 파일 경로"),
  }, async (p) => {
    const r = await bridge.request("scene.openAdditive", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_close", "로드된 씬을 닫습니다.", {
    sceneName: z.string().optional().describe("씬 이름"),
    scenePath: z.string().optional().describe("씬 경로"),
    removeScene: z.boolean().optional().describe("씬 완전 제거 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("scene.close", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_saveAs", "씬을 다른 경로로 저장합니다.", {
    newPath: z.string().describe("새 저장 경로"),
    sceneName: z.string().optional().describe("대상 씬 이름 (기본: 활성 씬)"),
  }, async (p) => {
    const r = await bridge.request("scene.saveAs", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  // ── 에디터 확장 ──

  server.tool("unity_scene_undo", "Undo를 수행합니다.", {
    steps: z.number().optional().describe("Undo 횟수 (기본: 1)"),
  }, async (p) => {
    const r = await bridge.request("scene.undo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_redo", "Redo를 수행합니다.", {
    steps: z.number().optional().describe("Redo 횟수 (기본: 1)"),
  }, async (p) => {
    const r = await bridge.request("scene.redo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_setSelection", "에디터에서 오브젝트를 선택합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.setSelection", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_getSelection", "에디터에서 현재 선택된 오브젝트를 조회합니다.", {}, async () => {
    const r = await bridge.request("scene.getSelection", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_setParent", "오브젝트의 부모를 설정합니다.", {
    childPath: z.string().optional(), childName: z.string().optional(), childInstanceId: z.number().optional(),
    parentPath: z.string().optional(), parentName: z.string().optional(), parentInstanceId: z.number().optional(),
    worldPositionStays: z.boolean().optional().describe("월드 위치 유지 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("scene.setParent", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_getContext", "에디터 상태 정보를 조회합니다 (활성 씬, 플레이 모드, 빌드 타겟 등).", {}, async () => {
    const r = await bridge.request("scene.getContext", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
