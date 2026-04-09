import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerCleanerTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_cleaner_findUnusedAssets",
    "Find unused assets",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
      types: z.array(z.string()).optional().describe("에셋 타입 필터 (기본: Texture2D, Material, AudioClip)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.findUnusedAssets", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_findDuplicateAssets",
    "Find duplicate assets",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
      type: z.string().optional().describe("에셋 타입 필터 (기본: t:Object)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.findDuplicateAssets", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_findMissingScripts",
    "Find missing scripts",
    {
      remove: z.boolean().optional().describe("true면 Missing Script 자동 제거 (기본: false, 조회만)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.findMissingScripts", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_findEmptyFolders",
    "Find empty folders",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.findEmptyFolders", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_getAssetDependencies",
    "Get asset dependencies",
    {
      assetPath: z.string(),
      recursive: z.boolean().optional().describe("재귀 탐색 (기본: true)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.getAssetDependencies", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_getReferences",
    "Get asset references",
    {
      assetPath: z.string(),
      folder: z.string().optional().describe("검색 범위 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.getReferences", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_findLargeFiles",
    "Find large files",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
      thresholdMB: z.number().optional().describe("임계값 MB (기본: 5)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.findLargeFiles", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_findUnusedMaterials",
    "Find unused materials",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.findUnusedMaterials", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_deleteEmptyFolders",
    "Delete empty folders",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.deleteEmptyFolders", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_projectSizeReport",
    "Project size report",
    {
      folder: z.string().optional().describe("분석 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.projectSizeReport", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
