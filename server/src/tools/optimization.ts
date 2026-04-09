import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerOptimizationTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_optimize_textureOverview",
    "Texture overview",
    {
      folder: z.string().optional().describe("분석 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.textureOverview", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_compressTextures",
    "Batch compress textures",
    {
      folder: z.string().optional().describe("대상 폴더 (기본: Assets)"),
      maxTextureSize: z.number().optional().describe("최대 크기 (기본: 2048)"),
      compression: z.string().optional().describe("압축 방식 (기본: CompressedHQ)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.compressTextures", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_meshOverview",
    "Mesh overview",
    {},
    async (params) => {
      const result = await bridge.request("optimize.meshOverview", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_enableMeshCompression",
    "Enable mesh compression",
    {
      folder: z.string().optional().describe("대상 폴더 (기본: Assets)"),
      level: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("optimize.enableMeshCompression", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_audioOverview",
    "Audio overview",
    {},
    async (params) => {
      const result = await bridge.request("optimize.audioOverview", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_compressAudio",
    "Batch compress audio",
    {
      folder: z.string().optional().describe("대상 폴더 (기본: Assets)"),
      format: z.string().optional().describe("압축 포맷 (Vorbis, ADPCM, MP3; 기본: Vorbis)"),
      quality: z.number().optional().describe("품질 (0-1, 기본: 0.5)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.compressAudio", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_findLargeAssets",
    "Find large assets",
    {
      thresholdMB: z.number().optional().describe("임계값 MB (기본: 10)"),
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.findLargeAssets", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_setStaticFlags",
    "Set static flags",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      recursive: z.boolean().optional().describe("자식에도 적용 (기본: false)"),
      flags: z.string().optional().describe("Static 플래그 (생략 시 기본값 적용)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.setStaticFlags", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_disableMipmaps",
    "Disable mipmaps",
    {
      folder: z.string().optional().describe("대상 폴더 (기본: Assets)"),
      onlyUI: z.boolean().optional().describe("스프라이트 타입만 대상 (기본: false)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.disableMipmaps", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_enableGPUInstancing",
    "Enable GPU instancing",
    {
      folder: z.string().optional().describe("대상 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.enableGPUInstancing", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
