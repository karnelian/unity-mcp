import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";
import { withSafety } from "./safety.js";

export function registerAssetTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_asset_search",
    "Search assets",
    {
      query: z.string().optional(),
      type: z.string().optional(),
      folder: z.string().optional(),
      maxResults: z.number().optional().default(50),
      offset: z.number().optional(),
      summaryOnly: z.boolean().optional(),
      includeDetails: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("asset.search", params);
      return textResult(result, params);
    }
  );

  server.tool(
    "unity_asset_info",
    "Get asset info",
    {
      path: z.string(),
    },
    async (params) => {
      const result = await bridge.request("asset.info", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_asset_createMaterial",
    "Create Material",
    withSafety({
      name: z.string(),
      shaderName: z.string().optional(),
      path: z.string().optional(),
      properties: z.record(z.string(), z.any()).optional(),
    }),
    async (params) => {
      const result = await bridge.request("asset.createMaterial", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_asset_createPrefab",
    "Create Prefab from object",
    withSafety({
      objectPath: z.string(),
      savePath: z.string(),
    }),
    async (params) => {
      const result = await bridge.request("asset.createPrefab", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_asset_importSettings",
    "Get/set import settings",
    withSafety({
      path: z.string(),
      action: z.enum(["get", "set"]),
      settings: z.record(z.string(), z.any()).optional(),
    }),
    async (params) => {
      const result = await bridge.request("asset.importSettings", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_asset_createFolder",
    "Create folder",
    withSafety({
      path: z.string(),
    }),
    async (params) => {
      const result = await bridge.request("asset.createFolder", params);
      return textResult(result);
    }
  );

  server.tool("unity_asset_delete", "Delete asset", withSafety({
    path: z.string(),
    useTrash: z.boolean().optional(),
  }), async (p) => {
    const r = await bridge.request("asset.delete", p);
    return textResult(r);
  });

  server.tool("unity_asset_move", "Move/rename asset", withSafety({
    oldPath: z.string(),
    newPath: z.string(),
  }), async (p) => {
    const r = await bridge.request("asset.move", p);
    return textResult(r);
  });

  server.tool("unity_asset_copy", "Copy asset", withSafety({
    sourcePath: z.string(),
    destPath: z.string(),
  }), async (p) => {
    const r = await bridge.request("asset.copy", p);
    return textResult(r);
  });

  server.tool("unity_asset_refresh", "Refresh AssetDatabase", withSafety({}), async (p) => {
    const r = await bridge.request("asset.refresh", p);
    return textResult(r);
  });

  server.tool("unity_asset_reimport", "Reimport asset", withSafety({
    path: z.string(),
  }), async (p) => {
    const r = await bridge.request("asset.reimport", p);
    return textResult(r);
  });

  server.tool("unity_asset_getLabels", "Get asset labels", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.getLabels", p);
    return textResult(r);
  });

  server.tool("unity_asset_setLabels", "Set asset labels", withSafety({
    path: z.string(),
    labels: z.array(z.string()),
  }), async (p) => {
    const r = await bridge.request("asset.setLabels", p);
    return textResult(r);
  });

  server.tool("unity_asset_importPackage", "Import .unitypackage file (non-interactive)", withSafety({
    path: z.string().describe("Path to .unitypackage file (e.g. Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage)"),
  }), async (p) => {
    const r = await bridge.request("asset.importPackage", p);
    return textResult(r);
  });
}
