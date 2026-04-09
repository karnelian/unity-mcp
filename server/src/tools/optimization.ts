import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerOptimizationTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_optimize_textureOverview",
    "Texture overview",
    {
      folder: z.string().optional(),
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
      folder: z.string().optional(),
      maxTextureSize: z.number().optional(),
      compression: z.string().optional(),
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
      folder: z.string().optional(),
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
      folder: z.string().optional(),
      format: z.string().optional(),
      quality: z.number().optional(),
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
      thresholdMB: z.number().optional(),
      folder: z.string().optional(),
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
      recursive: z.boolean().optional(),
      flags: z.string().optional(),
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
      folder: z.string().optional(),
      onlyUI: z.boolean().optional(),
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
      folder: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("optimize.enableGPUInstancing", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
