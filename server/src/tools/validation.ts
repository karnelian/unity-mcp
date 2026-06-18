import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

export function registerValidationTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_validate_missingScripts",
    "Find missing scripts",
    {},
    async (params) => {
      const result = await bridge.request("validate.missingScripts", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_missingReferences",
    "Find missing references",
    {
      maxResults: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("validate.missingReferences", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_shaderErrors",
    "Find shader errors",
    {},
    async (params) => {
      const result = await bridge.request("validate.shaderErrors", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_emptyGameObjects",
    "Find empty GameObjects",
    {},
    async (params) => {
      const result = await bridge.request("validate.emptyGameObjects", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_duplicateNames",
    "Find duplicate names",
    {},
    async (params) => {
      const result = await bridge.request("validate.duplicateNames", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_disabledRenderers",
    "Find disabled Renderers",
    {},
    async (params) => {
      const result = await bridge.request("validate.disabledRenderers", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_sceneStats",
    "Get scene stats",
    {},
    async (params) => {
      const result = await bridge.request("validate.sceneStats", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_prefabOverrides",
    "Find Prefab overrides",
    {},
    async (params) => {
      const result = await bridge.request("validate.prefabOverrides", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_largeMeshes",
    "Find large meshes",
    {
      vertexThreshold: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("validate.largeMeshes", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_validate_untaggedObjects",
    "Find untagged objects",
    {},
    async (params) => {
      const result = await bridge.request("validate.untaggedObjects", params);
      return textResult(result);
    }
  );
}
