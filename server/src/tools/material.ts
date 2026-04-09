import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });
const vec4 = z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number().optional() });

export function registerMaterialTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_material_create",
    "Create Material",
    {
      name: z.string(),
      savePath: z.string().optional().describe("저장 경로 (기본: Assets/Materials/{name}.mat)"),
      shader: z.string().optional().describe("셰이더 이름 (기본: Standard)"),
      color: color.optional(),
    },
    async (params) => {
      const result = await bridge.request("material.create", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_assign",
    "Assign Material",
    {
      materialPath: z.string(),
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      materialIndex: z.number().optional().describe("머티리얼 슬롯 인덱스 (기본: 0)"),
    },
    async (params) => {
      const result = await bridge.request("material.assign", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_create_batch",
    "Batch create Materials",
    {
      items: z.array(z.object({
        name: z.string(),
        savePath: z.string().optional(),
        shader: z.string().optional(),
        color: color.optional(),
      })),
    },
    async (params) => {
      const result = await bridge.request("material.createBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_assign_batch",
    "Batch assign Materials",
    {
      items: z.array(z.object({
        materialPath: z.string(),
        path: z.string().optional(),
        name: z.string().optional(),
        instanceId: z.number().optional(),
        materialIndex: z.number().optional(),
      })),
    },
    async (params) => {
      const result = await bridge.request("material.assignBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_duplicate",
    "Duplicate Material",
    {
      materialPath: z.string(),
      newName: z.string().optional(),
      savePath: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("material.duplicate", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_color",
    "Set Material color",
    {
      materialPath: z.string(),
      property: z.string().optional().describe("프로퍼티 이름 (기본: _Color)"),
      color: color,
    },
    async (params) => {
      const result = await bridge.request("material.setColor", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_colors_batch",
    "Batch set Material colors",
    {
      items: z.array(z.object({
        materialPath: z.string(),
        property: z.string().optional(),
        color: color,
      })),
    },
    async (params) => {
      const result = await bridge.request("material.setColorsBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_emission",
    "Set Material emission",
    {
      materialPath: z.string(),
      color: color,
      intensity: z.number().optional().describe("강도 (기본: 1.0)"),
    },
    async (params) => {
      const result = await bridge.request("material.setEmission", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_emission_batch",
    "Batch set Material emission",
    {
      items: z.array(z.object({
        materialPath: z.string(),
        color: color,
        intensity: z.number().optional(),
      })),
    },
    async (params) => {
      const result = await bridge.request("material.setEmissionBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_texture",
    "Set Material texture",
    {
      materialPath: z.string(),
      texturePath: z.string(),
      property: z.string().optional().describe("프로퍼티 이름 (기본: _MainTex)"),
    },
    async (params) => {
      const result = await bridge.request("material.setTexture", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_float",
    "Set Material float",
    {
      materialPath: z.string(),
      property: z.string().describe("프로퍼티 이름 (예: _Metallic, _Glossiness)"),
      value: z.number(),
    },
    async (params) => {
      const result = await bridge.request("material.setFloat", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_int",
    "Set Material int",
    {
      materialPath: z.string(),
      property: z.string(),
      value: z.number(),
    },
    async (params) => {
      const result = await bridge.request("material.setInt", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_vector",
    "Set Material vector",
    {
      materialPath: z.string(),
      property: z.string(),
      value: vec4,
    },
    async (params) => {
      const result = await bridge.request("material.setVector", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_texture_offset",
    "Set texture offset",
    {
      materialPath: z.string(),
      property: z.string().optional().describe("프로퍼티 이름 (기본: _MainTex)"),
      x: z.number(),
      y: z.number(),
    },
    async (params) => {
      const result = await bridge.request("material.setTextureOffset", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_texture_scale",
    "Set texture scale",
    {
      materialPath: z.string(),
      property: z.string().optional().describe("프로퍼티 이름 (기본: _MainTex)"),
      x: z.number(),
      y: z.number(),
    },
    async (params) => {
      const result = await bridge.request("material.setTextureScale", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_keyword",
    "Set Material keyword",
    {
      materialPath: z.string(),
      keyword: z.string().describe("키워드 이름 (예: _EMISSION, _NORMALMAP)"),
      enabled: z.boolean().optional().describe("활성화 여부 (기본: true)"),
    },
    async (params) => {
      const result = await bridge.request("material.setKeyword", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_render_queue",
    "Set render queue",
    {
      materialPath: z.string(),
      renderQueue: z.number().describe("렌더 큐 값 (예: 2000=Geometry, 3000=Transparent)"),
    },
    async (params) => {
      const result = await bridge.request("material.setRenderQueue", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_shader",
    "Set Material shader",
    {
      materialPath: z.string(),
      shader: z.string().describe("셰이더 이름 (예: 'Standard', 'Universal Render Pipeline/Lit')"),
    },
    async (params) => {
      const result = await bridge.request("material.setShader", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_gi_flags",
    "Set GI flags",
    {
      materialPath: z.string(),
      flags: z.enum(["None", "RealtimeEmissive", "BakedEmissive", "EmissiveIsBlack", "AnyEmissive"]).describe("GI 플래그"),
    },
    async (params) => {
      const result = await bridge.request("material.setGIFlags", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_get_properties",
    "Get Material properties",
    {
      materialPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("material.getProperties", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_get_keywords",
    "Get Material keywords",
    {
      materialPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("material.getKeywords", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
