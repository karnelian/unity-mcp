import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerProfilerTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_profiler_memoryOverview",
    "Unity 메모리 사용 현황을 조회합니다 (총 예약, 할당, Mono, GFX).",
    {},
    async (params) => {
      const result = await bridge.request("profiler.memoryOverview", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_objectCount",
    "씬의 오브젝트 유형별 개수를 조회합니다.",
    {
      includeInactive: z.boolean().optional().describe("비활성 오브젝트 포함 (기본: true)"),
    },
    async (params) => {
      const result = await bridge.request("profiler.objectCount", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_renderingStats",
    "렌더링 통계를 조회합니다 (버텍스, 트라이앵글, 드로우콜 등).",
    {},
    async (params) => {
      const result = await bridge.request("profiler.renderingStats", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_textureMemory",
    "텍스처 메모리 사용량을 분석합니다 (상위 N개).",
    {
      maxResults: z.number().optional().describe("상위 결과 수 (기본: 20)"),
    },
    async (params) => {
      const result = await bridge.request("profiler.textureMemory", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_meshMemory",
    "메시 메모리 사용량을 분석합니다 (상위 N개).",
    {
      maxResults: z.number().optional().describe("상위 결과 수 (기본: 20)"),
    },
    async (params) => {
      const result = await bridge.request("profiler.meshMemory", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_materialCount",
    "프로젝트 Material 수와 셰이더별 사용 현황을 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("profiler.materialCount", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_componentStats",
    "씬의 컴포넌트 유형별 통계를 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("profiler.componentStats", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_shaderVariants",
    "셰이더별 키워드/변형 사용 현황을 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("profiler.shaderVariants", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_assetCount",
    "프로젝트의 에셋 유형별 개수를 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("profiler.assetCount", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_sceneComplexity",
    "씬의 복잡도를 분석합니다 (오브젝트 수, 계층 깊이, 복잡도 점수).",
    {},
    async (params) => {
      const result = await bridge.request("profiler.sceneComplexity", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
