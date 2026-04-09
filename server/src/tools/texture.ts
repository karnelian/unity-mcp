import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerTextureTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_texture_getSettings",
    "Get texture settings",
    {
      texturePath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("texture.getSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setSettings",
    "Set texture settings",
    {
      texturePath: z.string(),
      textureType: z.string().optional(),
      maxTextureSize: z.number().optional(),
      textureCompression: z.string().optional(),
      isReadable: z.boolean().optional(),
      mipmapEnabled: z.boolean().optional(),
      filterMode: z.string().optional(),
      wrapMode: z.string().optional(),
      sRGBTexture: z.boolean().optional(),
      alphaIsTransparency: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("texture.setSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_getInfo",
    "Get texture info",
    {
      texturePath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("texture.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_find",
    "Find textures",
    {
      folder: z.string().optional(),
      nameFilter: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("texture.find", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setPlatformSettings",
    "Set platform texture settings",
    {
      texturePath: z.string(),
      platform: z.string(),
      maxTextureSize: z.number().optional(),
      format: z.string().optional(),
      compressionQuality: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("texture.setPlatformSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setSpriteSettings",
    "Set sprite settings",
    {
      texturePath: z.string(),
      spriteImportMode: z.string().optional(),
      pixelsPerUnit: z.number().optional(),
      pivot: z.object({ x: z.number(), y: z.number() }).optional(),
    },
    async (params) => {
      const result = await bridge.request("texture.setSpriteSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setNormalMap",
    "Set as normal map",
    {
      texturePath: z.string(),
      bumpScale: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("texture.setNormalMap", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_resize",
    "Resize texture",
    {
      texturePath: z.string(),
      maxTextureSize: z.number(),
    },
    async (params) => {
      const result = await bridge.request("texture.resize", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_getMemorySize",
    "Get texture memory",
    {
      texturePath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("texture.getMemorySize", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setReadable",
    "Set texture readable",
    {
      texturePath: z.string(),
      readable: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("texture.setReadable", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
