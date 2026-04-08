import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerSmartTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_smart_sceneQuery",
    "SQL-like 씬 쿼리. 컴포넌트 프로퍼티 조건으로 오브젝트를 검색합니다. 예: 'Light.intensity > 1', 'Renderer.enabled == false', 'Collider'",
    {
      query: z.string().describe("쿼리 문자열 (예: 'Light.intensity > 1', 'AudioSource', 'MeshRenderer.enabled == false')"),
      limit: z.number().optional().describe("최대 결과 수 (기본: 100)"),
    },
    async (params) => {
      const result = await bridge.request("smart.sceneQuery", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_smart_referenceBind",
    "컴포넌트의 배열/리스트 필드를 태그/이름/컴포넌트 패턴으로 자동 바인딩합니다.",
    {
      path: z.string().describe("대상 GameObject 경로"),
      componentType: z.string().describe("컴포넌트 타입 이름"),
      fieldName: z.string().describe("배열/리스트 필드 이름"),
      matchBy: z.enum(["tag", "name", "component"]).optional().describe("매칭 기준 (기본: tag)"),
      pattern: z.string().describe("매칭 패턴 (태그명, 이름 패턴, 또는 컴포넌트 타입)"),
    },
    async (params) => {
      const result = await bridge.request("smart.referenceBind", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
