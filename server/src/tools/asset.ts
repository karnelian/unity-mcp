import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAssetTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_asset_search",
    "Search assets",
    {
      query: z.string().optional(),
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
    "Get asset info",
    {
      path: z.string(),
    },
    async (params) => {
      const result = await bridge.request("asset.info", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_createMaterial",
    "Create Material",
    {
      name: z.string(),
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
    "Create Prefab from object",
    {
      objectPath: z.string(),
      savePath: z.string().describe("프리팹 저장 경로 (예: 'Assets/Prefabs/Player.prefab')"),
    },
    async (params) => {
      const result = await bridge.request("asset.createPrefab", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_importSettings",
    "Get/set import settings",
    {
      path: z.string(),
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
    "Create folder",
    {
      path: z.string().describe("폴더 경로 (예: 'Assets/Art/Textures/Characters')"),
    },
    async (params) => {
      const result = await bridge.request("asset.createFolder", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool("unity_asset_delete", "Delete asset", {
    path: z.string(),
    useTrash: z.boolean().optional().describe("휴지통 사용 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("asset.delete", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_move", "Move/rename asset", {
    oldPath: z.string(),
    newPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.move", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_copy", "Copy asset", {
    sourcePath: z.string(),
    destPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.copy", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_refresh", "Refresh AssetDatabase", {}, async () => {
    const r = await bridge.request("asset.refresh", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_reimport", "Reimport asset", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.reimport", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_getLabels", "Get asset labels", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.getLabels", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_setLabels", "Set asset labels", {
    path: z.string(),
    labels: z.array(z.string()),
  }, async (p) => {
    const r = await bridge.request("asset.setLabels", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
