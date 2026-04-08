import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerTextureTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_texture_getSettings",
    "텍스처 임포트 설정을 조회합니다.",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("texture.getSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setSettings",
    "텍스처 임포트 설정을 변경합니다.",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
      textureType: z.string().optional().describe("텍스처 타입 (Default, NormalMap, Sprite 등)"),
      maxTextureSize: z.number().optional().describe("최대 텍스처 크기"),
      textureCompression: z.string().optional().describe("압축 방식 (Uncompressed, Compressed 등)"),
      isReadable: z.boolean().optional().describe("CPU 읽기 가능 여부"),
      mipmapEnabled: z.boolean().optional().describe("밉맵 생성 여부"),
      filterMode: z.string().optional().describe("필터 모드 (Point, Bilinear, Trilinear)"),
      wrapMode: z.string().optional().describe("래핑 모드 (Repeat, Clamp 등)"),
      sRGBTexture: z.boolean().optional().describe("sRGB 색 공간 여부"),
      alphaIsTransparency: z.boolean().optional().describe("알파를 투명도로 사용"),
    },
    async (params) => {
      const result = await bridge.request("texture.setSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_getInfo",
    "텍스처의 런타임 정보를 조회합니다 (크기, 포맷 등).",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("texture.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_find",
    "프로젝트에서 텍스처를 검색합니다.",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
      nameFilter: z.string().optional().describe("이름 필터 (부분 매칭)"),
    },
    async (params) => {
      const result = await bridge.request("texture.find", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setPlatformSettings",
    "플랫폼별 텍스처 설정을 지정합니다.",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
      platform: z.string().describe("플랫폼 (Standalone, Android, iOS, WebGL 등)"),
      maxTextureSize: z.number().optional().describe("최대 텍스처 크기"),
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
    "텍스처를 스프라이트로 설정합니다.",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
      spriteImportMode: z.string().optional().describe("스프라이트 모드 (Single, Multiple, Polygon)"),
      pixelsPerUnit: z.number().optional().describe("픽셀 당 유닛"),
      pivot: z.object({ x: z.number(), y: z.number() }).optional().describe("피봇 포인트 (0-1)"),
    },
    async (params) => {
      const result = await bridge.request("texture.setSpriteSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setNormalMap",
    "텍스처를 노멀맵으로 설정합니다.",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
      bumpScale: z.number().optional().describe("범프 스케일 (기본: 1)"),
    },
    async (params) => {
      const result = await bridge.request("texture.setNormalMap", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_resize",
    "텍스처의 최대 크기를 변경합니다.",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
      maxTextureSize: z.number().describe("새 최대 텍스처 크기 (32, 64, 128, 256, 512, 1024, 2048, 4096, 8192)"),
    },
    async (params) => {
      const result = await bridge.request("texture.resize", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_getMemorySize",
    "텍스처의 런타임 메모리 사용량을 조회합니다.",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("texture.getMemorySize", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_texture_setReadable",
    "텍스처의 Read/Write 설정을 변경합니다.",
    {
      texturePath: z.string().describe("텍스처 에셋 경로"),
      readable: z.boolean().optional().describe("읽기 가능 여부 (기본: true)"),
    },
    async (params) => {
      const result = await bridge.request("texture.setReadable", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
