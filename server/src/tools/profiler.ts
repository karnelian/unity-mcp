import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerProfilerTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_profiler_memoryOverview",
    "Memory overview",
    {},
    async (params) => {
      const result = await bridge.request("profiler.memoryOverview", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_objectCount",
    "Count objects",
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
    "Get rendering stats",
    {},
    async (params) => {
      const result = await bridge.request("profiler.renderingStats", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_textureMemory",
    "Get texture memory",
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
    "Get mesh memory",
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
    "Count materials",
    {},
    async (params) => {
      const result = await bridge.request("profiler.materialCount", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_componentStats",
    "Component stats",
    {},
    async (params) => {
      const result = await bridge.request("profiler.componentStats", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_shaderVariants",
    "Shader variant count",
    {},
    async (params) => {
      const result = await bridge.request("profiler.shaderVariants", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_assetCount",
    "Count assets",
    {},
    async (params) => {
      const result = await bridge.request("profiler.assetCount", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_profiler_sceneComplexity",
    "Scene complexity",
    {},
    async (params) => {
      const result = await bridge.request("profiler.sceneComplexity", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
