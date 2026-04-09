import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerCleanerTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_cleaner_findUnusedAssets",
    "Find unused assets",
    {
      folder: z.string().optional(),
      types: z.array(z.string()).optional(),
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
      folder: z.string().optional(),
      type: z.string().optional(),
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
      remove: z.boolean().optional(),
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
      folder: z.string().optional(),
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
      recursive: z.boolean().optional(),
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
      folder: z.string().optional(),
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
      folder: z.string().optional(),
      thresholdMB: z.number().optional(),
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
      folder: z.string().optional(),
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
      folder: z.string().optional(),
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
      folder: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("cleaner.projectSizeReport", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
