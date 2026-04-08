import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAssetTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_asset_search",
    "프로젝트에서 에셋을 검색합니다.",
    {
      query: z.string().optional().describe("검색어"),
      type: z.string().optional().describe("에셋 타입 (예: 'Material', 'Prefab', 'Texture2D', 'AudioClip')"),
      folder: z.string().optional().describe("검색 폴더 (예: 'Assets/Prefabs')"),
    },
    async (params) => {
      const result = await bridge.request("asset.search", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_info",
    "에셋의 상세 정보와 의존성을 조회합니다.",
    {
      path: z.string().describe("에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("asset.info", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_createMaterial",
    "새 머티리얼을 생성합니다.",
    {
      name: z.string().describe("머티리얼 이름"),
      shaderName: z.string().optional().describe("셰이더 이름 (기본: 'Standard'. URP 프로젝트에서는 'Universal Render Pipeline/Lit' 사용)"),
      path: z.string().optional().describe("저장 경로 (기본: 'Assets/Materials/')"),
      properties: z.record(z.string(), z.any()).optional().describe("셰이더 프로퍼티 (예: { _Color: { r:1, g:0, b:0, a:1 } })"),
    },
    async (params) => {
      const result = await bridge.request("asset.createMaterial", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_createPrefab",
    "씬의 오브젝트를 프리팹으로 저장합니다.",
    {
      objectPath: z.string().describe("씬 오브젝트 경로"),
      savePath: z.string().describe("프리팹 저장 경로 (예: 'Assets/Prefabs/Player.prefab')"),
    },
    async (params) => {
      const result = await bridge.request("asset.createPrefab", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_importSettings",
    "에셋의 임포트 설정을 조회하거나 수정합니다.",
    {
      path: z.string().describe("에셋 경로"),
      action: z.enum(["get", "set"]).describe("조회 또는 수정"),
      settings: z.record(z.string(), z.any()).optional().describe("수정할 설정 (action=set일 때)"),
    },
    async (params) => {
      const result = await bridge.request("asset.importSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_createFolder",
    "프로젝트에 폴더를 생성합니다. 중간 경로 자동 생성.",
    {
      path: z.string().describe("폴더 경로 (예: 'Assets/Art/Textures/Characters')"),
    },
    async (params) => {
      const result = await bridge.request("asset.createFolder", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool("unity_asset_delete", "에셋을 삭제합니다.", {
    path: z.string().describe("에셋 경로"),
    useTrash: z.boolean().optional().describe("휴지통 사용 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("asset.delete", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_move", "에셋을 이동/이름변경합니다.", {
    oldPath: z.string().describe("현재 경로"),
    newPath: z.string().describe("새 경로"),
  }, async (p) => {
    const r = await bridge.request("asset.move", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_copy", "에셋을 복사합니다.", {
    sourcePath: z.string().describe("원본 경로"),
    destPath: z.string().describe("대상 경로"),
  }, async (p) => {
    const r = await bridge.request("asset.copy", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_refresh", "AssetDatabase를 새로고침합니다.", {}, async () => {
    const r = await bridge.request("asset.refresh", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_reimport", "에셋을 강제 재임포트합니다.", {
    path: z.string().describe("에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("asset.reimport", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_getLabels", "에셋의 라벨을 조회합니다.", {
    path: z.string().describe("에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("asset.getLabels", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_setLabels", "에셋의 라벨을 설정합니다.", {
    path: z.string().describe("에셋 경로"),
    labels: z.array(z.string()).describe("라벨 배열"),
  }, async (p) => {
    const r = await bridge.request("asset.setLabels", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
