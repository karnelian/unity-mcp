import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerCleanerTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_cleaner_findUnusedAssets",
    "빌드 씬에서 사용되지 않는 에셋을 찾습니다.",
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
    "파일 크기 기반으로 중복 에셋 후보를 찾습니다.",
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
    "Missing Script를 찾거나 제거합니다.",
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
    "빈 폴더를 찾습니다.",
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
    "에셋의 의존성 목록을 조회합니다.",
    {
      assetPath: z.string().describe("에셋 경로"),
      recursive: z.boolean().optional().describe("재귀 탐색 (기본: true)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.getAssetDependencies", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_getReferences",
    "특정 에셋을 참조하는 다른 에셋을 찾습니다.",
    {
      assetPath: z.string().describe("대상 에셋 경로"),
      folder: z.string().optional().describe("검색 범위 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.getReferences", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cleaner_findLargeFiles",
    "특정 크기 이상의 대형 파일을 찾습니다.",
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
    "씬과 빌드에서 사용되지 않는 Material을 찾습니다.",
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
    "빈 폴더를 삭제합니다.",
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
    "프로젝트 크기를 카테고리별로 분석합니다.",
    {
      folder: z.string().optional().describe("분석 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("cleaner.projectSizeReport", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
