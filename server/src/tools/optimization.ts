import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerOptimizationTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_optimize_textureOverview",
    "프로젝트 텍스처의 최적화 현황을 분석합니다.",
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
    "폴더 내 비압축 텍스처를 일괄 압축합니다.",
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
    "프로젝트 메시/모델의 최적화 현황을 분석합니다.",
    {},
    async (params) => {
      const result = await bridge.request("optimize.meshOverview", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_enableMeshCompression",
    "폴더 내 비압축 메시에 압축을 일괄 적용합니다.",
    {
      folder: z.string().optional().describe("대상 폴더 (기본: Assets)"),
      level: z.string().optional().describe("압축 레벨 (Low, Medium, High; 기본: Medium)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.enableMeshCompression", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_audioOverview",
    "프로젝트 오디오 클립의 최적화 현황을 분석합니다.",
    {},
    async (params) => {
      const result = await bridge.request("optimize.audioOverview", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_optimize_compressAudio",
    "폴더 내 비압축 오디오를 일괄 압축합니다.",
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
    "특정 크기 이상의 대형 에셋을 찾습니다.",
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
    "GameObject에 Static 플래그를 설정합니다 (배칭, GI 등).",
    {
      path: z.string().optional().describe("오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
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
    "UI/스프라이트 텍스처의 밉맵을 일괄 비활성화합니다.",
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
    "Material에 GPU Instancing을 일괄 활성화합니다.",
    {
      folder: z.string().optional().describe("대상 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("optimize.enableGPUInstancing", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
