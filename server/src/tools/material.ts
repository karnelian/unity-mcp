import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });
const vec4 = z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number().optional() });

export function registerMaterialTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_material_create",
    "새 Material을 생성합니다.",
    {
      name: z.string().describe("머티리얼 이름"),
      savePath: z.string().optional().describe("저장 경로 (기본: Assets/Materials/{name}.mat)"),
      shader: z.string().optional().describe("셰이더 이름 (기본: Standard)"),
      color: color.optional().describe("초기 색상 (RGBA)"),
    },
    async (params) => {
      const result = await bridge.request("material.create", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_assign",
    "GameObject에 Material을 할당합니다. path/name/instanceId로 대상 지정.",
    {
      materialPath: z.string().describe("머티리얼 에셋 경로"),
      path: z.string().optional().describe("오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      materialIndex: z.number().optional().describe("머티리얼 슬롯 인덱스 (기본: 0)"),
    },
    async (params) => {
      const result = await bridge.request("material.assign", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_create_batch",
    "여러 Material을 한 번에 생성합니다.",
    {
      items: z.array(z.object({
        name: z.string(),
        savePath: z.string().optional(),
        shader: z.string().optional(),
        color: color.optional(),
      })).describe("생성할 머티리얼 배열"),
    },
    async (params) => {
      const result = await bridge.request("material.createBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_assign_batch",
    "여러 GameObject에 Material을 한 번에 할당합니다.",
    {
      items: z.array(z.object({
        materialPath: z.string(),
        path: z.string().optional(),
        name: z.string().optional(),
        instanceId: z.number().optional(),
        materialIndex: z.number().optional(),
      })).describe("할당 배열"),
    },
    async (params) => {
      const result = await bridge.request("material.assignBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_duplicate",
    "Material을 복제합니다.",
    {
      materialPath: z.string().describe("원본 머티리얼 경로"),
      newName: z.string().optional().describe("새 이름"),
      savePath: z.string().optional().describe("저장 경로"),
    },
    async (params) => {
      const result = await bridge.request("material.duplicate", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_color",
    "Material의 색상 프로퍼티를 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      property: z.string().optional().describe("프로퍼티 이름 (기본: _Color)"),
      color: color.describe("색상 (RGBA)"),
    },
    async (params) => {
      const result = await bridge.request("material.setColor", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_colors_batch",
    "여러 Material의 색상을 한 번에 설정합니다.",
    {
      items: z.array(z.object({
        materialPath: z.string(),
        property: z.string().optional(),
        color: color,
      })).describe("색상 설정 배열"),
    },
    async (params) => {
      const result = await bridge.request("material.setColorsBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_emission",
    "Material의 Emission을 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      color: color.describe("발광 색상"),
      intensity: z.number().optional().describe("강도 (기본: 1.0)"),
    },
    async (params) => {
      const result = await bridge.request("material.setEmission", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_emission_batch",
    "여러 Material의 Emission을 한 번에 설정합니다.",
    {
      items: z.array(z.object({
        materialPath: z.string(),
        color: color,
        intensity: z.number().optional(),
      })).describe("Emission 설정 배열"),
    },
    async (params) => {
      const result = await bridge.request("material.setEmissionBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_texture",
    "Material에 텍스처를 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      texturePath: z.string().describe("텍스처 에셋 경로"),
      property: z.string().optional().describe("프로퍼티 이름 (기본: _MainTex)"),
    },
    async (params) => {
      const result = await bridge.request("material.setTexture", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_float",
    "Material의 float 프로퍼티를 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      property: z.string().describe("프로퍼티 이름 (예: _Metallic, _Glossiness)"),
      value: z.number().describe("값"),
    },
    async (params) => {
      const result = await bridge.request("material.setFloat", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_int",
    "Material의 int 프로퍼티를 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      property: z.string().describe("프로퍼티 이름"),
      value: z.number().describe("값"),
    },
    async (params) => {
      const result = await bridge.request("material.setInt", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_vector",
    "Material의 Vector4 프로퍼티를 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      property: z.string().describe("프로퍼티 이름"),
      value: vec4.describe("Vector4 값 (x,y,z,w)"),
    },
    async (params) => {
      const result = await bridge.request("material.setVector", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_texture_offset",
    "Material의 텍스처 오프셋을 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      property: z.string().optional().describe("프로퍼티 이름 (기본: _MainTex)"),
      x: z.number().describe("X 오프셋"),
      y: z.number().describe("Y 오프셋"),
    },
    async (params) => {
      const result = await bridge.request("material.setTextureOffset", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_texture_scale",
    "Material의 텍스처 스케일을 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      property: z.string().optional().describe("프로퍼티 이름 (기본: _MainTex)"),
      x: z.number().describe("X 스케일"),
      y: z.number().describe("Y 스케일"),
    },
    async (params) => {
      const result = await bridge.request("material.setTextureScale", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_keyword",
    "Material의 셰이더 키워드를 활성화/비활성화합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
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
    "Material의 Render Queue를 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      renderQueue: z.number().describe("렌더 큐 값 (예: 2000=Geometry, 3000=Transparent)"),
    },
    async (params) => {
      const result = await bridge.request("material.setRenderQueue", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_shader",
    "Material의 셰이더를 변경합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      shader: z.string().describe("셰이더 이름 (예: 'Standard', 'Universal Render Pipeline/Lit')"),
    },
    async (params) => {
      const result = await bridge.request("material.setShader", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_set_gi_flags",
    "Material의 Global Illumination 플래그를 설정합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
      flags: z.enum(["None", "RealtimeEmissive", "BakedEmissive", "EmissiveIsBlack", "AnyEmissive"]).describe("GI 플래그"),
    },
    async (params) => {
      const result = await bridge.request("material.setGIFlags", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_get_properties",
    "Material의 모든 프로퍼티를 조회합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
    },
    async (params) => {
      const result = await bridge.request("material.getProperties", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_material_get_keywords",
    "Material의 활성화된 셰이더 키워드를 조회합니다.",
    {
      materialPath: z.string().describe("머티리얼 경로"),
    },
    async (params) => {
      const result = await bridge.request("material.getKeywords", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
