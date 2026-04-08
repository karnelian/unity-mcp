import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerModelTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_model_getSettings",
    "3D 모델의 임포트 설정을 조회합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로 (FBX, OBJ 등)"),
    },
    async (params) => {
      const result = await bridge.request("model.getSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setSettings",
    "3D 모델의 임포트 설정을 변경합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로"),
      globalScale: z.number().optional().describe("글로벌 스케일"),
      meshCompression: z.string().optional().describe("메시 압축 (Off, Low, Medium, High)"),
      isReadable: z.boolean().optional().describe("CPU 읽기 가능 여부"),
      importNormals: z.string().optional().describe("노멀 임포트 (Import, Calculate, None)"),
      importAnimation: z.boolean().optional().describe("애니메이션 임포트 여부"),
      generateColliders: z.boolean().optional().describe("콜라이더 자동 생성"),
      optimizeMesh: z.boolean().optional().describe("메시 최적화 (폴리곤+버텍스)"),
    },
    async (params) => {
      const result = await bridge.request("model.setSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_getInfo",
    "3D 모델의 메시, 버텍스, 트라이앵글 정보를 조회합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("model.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_find",
    "프로젝트에서 3D 모델을 검색합니다.",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
      nameFilter: z.string().optional().describe("이름 필터 (부분 매칭)"),
    },
    async (params) => {
      const result = await bridge.request("model.find", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_getMeshInfo",
    "모델에 포함된 모든 메시의 상세 정보를 조회합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("model.getMeshInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setRigType",
    "모델의 리깅 타입을 설정합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로"),
      rigType: z.string().describe("리그 타입 (None, Legacy, Generic, Humanoid)"),
    },
    async (params) => {
      const result = await bridge.request("model.setRigType", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_getAnimations",
    "모델에 포함된 애니메이션 클립 목록을 조회합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("model.getAnimations", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setAnimationSettings",
    "모델의 애니메이션 클립 설정을 변경합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로"),
      clipName: z.string().describe("애니메이션 클립 이름"),
      loop: z.boolean().optional().describe("루프 여부"),
      name: z.string().optional().describe("클립 이름 변경"),
    },
    async (params) => {
      const result = await bridge.request("model.setAnimationSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setScale",
    "모델의 글로벌 스케일을 변경합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로"),
      scale: z.number().describe("새 스케일 값"),
    },
    async (params) => {
      const result = await bridge.request("model.setScale", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setMaterialImport",
    "모델의 머티리얼 임포트 모드를 설정합니다.",
    {
      modelPath: z.string().describe("모델 에셋 경로"),
      mode: z.string().describe("임포트 모드 (None, ImportStandard, ImportViaMaterialDescription)"),
    },
    async (params) => {
      const result = await bridge.request("model.setMaterialImport", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
