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
      textureType: z.string().optional().describe("텍스처 타입 (Default, NormalMap, Sprite 등)"),
      maxTextureSize: z.number().optional(),
      textureCompression: z.string().optional().describe("압축 방식 (Uncompressed, Compressed 등)"),
      isReadable: z.boolean().optional(),
      mipmapEnabled: z.boolean().optional(),
      filterMode: z.string().optional().describe("필터 모드 (Point, Bilinear, Trilinear)"),
      wrapMode: z.string().optional().describe("래핑 모드 (Repeat, Clamp 등)"),
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
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
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
      platform: z.string().describe("플랫폼 (Standalone, Android, iOS, WebGL 등)"),
      maxTextureSize: z.number().optional(),
      format: z.string().optional().describe("텍스처 포맷 (ASTC_6x6, ETC2_RGBA8 등)"),
      compressionQuality: z.number().optional().describe("압축 품질 (0-100)"),
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
      spriteImportMode: z.string().optional().describe("스프라이트 모드 (Single, Multiple, Polygon)"),
      pixelsPerUnit: z.number().optional().describe("픽셀 당 유닛"),
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
      bumpScale: z.number().optional().describe("범프 스케일 (기본: 1)"),
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
      readable: z.boolean().optional().describe("읽기 가능 여부 (기본: true)"),
    },
    async (params) => {
      const result = await bridge.request("texture.setReadable", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
