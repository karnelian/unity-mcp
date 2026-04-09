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
      maxDepth: z.number().optional(),
      includeComponents: z.boolean().optional(),
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
      typeName: z.string(),
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
