import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerValidationTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_validate_missingScripts",
    "씬에서 Missing Script가 있는 GameObject를 찾습니다.",
    {},
    async (params) => {
      const result = await bridge.request("validate.missingScripts", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_missingReferences",
    "씬에서 Missing Reference(null 참조)를 찾습니다.",
    {
      maxResults: z.number().optional().describe("최대 결과 수 (기본: 100)"),
    },
    async (params) => {
      const result = await bridge.request("validate.missingReferences", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_shaderErrors",
    "프로젝트에서 셰이더 오류가 있는 Material을 찾습니다.",
    {},
    async (params) => {
      const result = await bridge.request("validate.shaderErrors", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_emptyGameObjects",
    "씬에서 빈 GameObject(Transform만 있고 자식도 없음)를 찾습니다.",
    {},
    async (params) => {
      const result = await bridge.request("validate.emptyGameObjects", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_duplicateNames",
    "씬에서 중복된 이름의 GameObject를 찾습니다.",
    {},
    async (params) => {
      const result = await bridge.request("validate.duplicateNames", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_disabledRenderers",
    "비활성화된 Renderer를 가진 GameObject를 찾습니다.",
    {},
    async (params) => {
      const result = await bridge.request("validate.disabledRenderers", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_sceneStats",
    "현재 씬의 종합 통계를 조회합니다 (오브젝트 수, 버텍스, 트라이앵글 등).",
    {},
    async (params) => {
      const result = await bridge.request("validate.sceneStats", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_prefabOverrides",
    "씬에서 프리팹 오버라이드가 있는 인스턴스를 찾습니다.",
    {},
    async (params) => {
      const result = await bridge.request("validate.prefabOverrides", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_largeMeshes",
    "특정 버텍스 수 이상의 대형 메시를 찾습니다.",
    {
      vertexThreshold: z.number().optional().describe("버텍스 수 임계값 (기본: 10000)"),
    },
    async (params) => {
      const result = await bridge.request("validate.largeMeshes", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_validate_untaggedObjects",
    "Untagged 태그를 가진 활성 GameObject를 찾습니다.",
    {},
    async (params) => {
      const result = await bridge.request("validate.untaggedObjects", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
