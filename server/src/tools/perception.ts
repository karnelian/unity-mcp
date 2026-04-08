import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerPerceptionTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_perception_sceneSummary",
    "현재 씬의 구조적 요약을 반환합니다. 오브젝트 수, 컴포넌트 통계, 메시 정보 등.",
    {},
    async (params) => {
      const result = await bridge.request("perception.sceneSummary", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_perception_hierarchyDescribe",
    "씬 계층 구조를 텍스트 트리로 반환합니다 (tree 명령어처럼).",
    {
      maxDepth: z.number().optional().describe("최대 깊이 (기본: 5)"),
      includeComponents: z.boolean().optional().describe("컴포넌트 포함 여부 (기본: false)"),
      root: z.string().optional().describe("특정 루트 오브젝트 필터"),
    },
    async (params) => {
      const result = await bridge.request("perception.hierarchyDescribe", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_perception_scriptAnalyze",
    "스크립트 타입의 공개 API를 리플렉션으로 분석합니다 (필드, 프로퍼티, 메서드, 상속).",
    {
      typeName: z.string().describe("분석할 타입 이름 (예: PlayerController)"),
    },
    async (params) => {
      const result = await bridge.request("perception.scriptAnalyze", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_perception_sceneMaterials",
    "씬에서 사용 중인 모든 머티리얼과 셰이더를 분석합니다.",
    {},
    async (params) => {
      const result = await bridge.request("perception.sceneMaterials", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_perception_sceneContext",
    "현재 에디터 컨텍스트를 반환합니다 (선택된 오브젝트, 씬 뷰 상태, 플레이 상태 등).",
    {},
    async (params) => {
      const result = await bridge.request("perception.sceneContext", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
