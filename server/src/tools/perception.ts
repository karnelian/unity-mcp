import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerPerceptionTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_perception_sceneSummary",
    "Get scene summary",
    {},
    async (params) => {
      const result = await bridge.request("perception.sceneSummary", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_perception_hierarchyDescribe",
    "Describe hierarchy node",
    {
      maxDepth: z.number().optional().describe("최대 깊이 (기본: 5)"),
      includeComponents: z.boolean().optional().describe("컴포넌트 포함 여부 (기본: false)"),
      root: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("perception.hierarchyDescribe", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_perception_scriptAnalyze",
    "Analyze script",
    {
      typeName: z.string().describe("분석할 타입 이름 (예: PlayerController)"),
    },
    async (params) => {
      const result = await bridge.request("perception.scriptAnalyze", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_perception_sceneMaterials",
    "Get scene materials",
    {},
    async (params) => {
      const result = await bridge.request("perception.sceneMaterials", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_perception_sceneContext",
    "Get scene context",
    {},
    async (params) => {
      const result = await bridge.request("perception.sceneContext", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
