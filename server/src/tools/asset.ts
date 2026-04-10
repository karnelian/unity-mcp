import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAssetTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_asset_search",
    "Search assets",
    {
      query: z.string().optional(),
      type: z.string().optional(),
      folder: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("asset.search", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_createMaterial",
    "Create Material",
    {
      name: z.string(),
      shaderName: z.string().optional(),
      path: z.string().optional(),
      properties: z.record(z.string(), z.any()).optional(),
    },
    async (params) => {
      const result = await bridge.request("asset.createMaterial", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_createPrefab",
    "Create Prefab from object",
    {
      objectPath: z.string(),
      savePath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("asset.createPrefab", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_importSettings",
    "Get/set import settings",
    {
      path: z.string(),
      action: z.enum(["get", "set"]),
      settings: z.record(z.string(), z.any()).optional(),
    },
    async (params) => {
      const result = await bridge.request("asset.importSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_asset_createFolder",
    "Create folder",
    {
      path: z.string(),
    },
    async (params) => {
      const result = await bridge.request("asset.createFolder", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool("unity_asset_delete", "Delete asset", {
    path: z.string(),
    useTrash: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("asset.delete", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_move", "Move/rename asset", {
    oldPath: z.string(),
    newPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.move", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_copy", "Copy asset", {
    sourcePath: z.string(),
    destPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.copy", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_refresh", "Refresh AssetDatabase", {}, async () => {
    const r = await bridge.request("asset.refresh", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_reimport", "Reimport asset", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.reimport", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_getLabels", "Get asset labels", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("asset.getLabels", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_setLabels", "Set asset labels", {
    path: z.string(),
    labels: z.array(z.string()),
  }, async (p) => {
    const r = await bridge.request("asset.setLabels", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asset_importPackage", "Import .unitypackage file (non-interactive)", {
    path: z.string().describe("Path to .unitypackage file (e.g. Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage)"),
  }, async (p) => {
    const r = await bridge.request("asset.importPackage", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
