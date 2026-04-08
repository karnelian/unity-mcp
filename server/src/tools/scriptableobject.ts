import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerScriptableObjectTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_so_create",
    "ScriptableObject 인스턴스를 생성합니다.",
    {
      typeName: z.string().describe("ScriptableObject 타입 이름 (Name 또는 FullName)"),
      savePath: z.string().describe("저장 경로 (예: Assets/Data/MyConfig.asset)"),
      name: z.string().optional().describe("에셋 이름 (생략 시 파일명 사용)"),
    },
    async (params) => {
      const result = await bridge.request("so.create", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_find",
    "ScriptableObject 에셋을 검색합니다.",
    {
      typeName: z.string().optional().describe("타입 이름으로 필터링"),
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
    },
    async (params) => {
      const result = await bridge.request("so.find", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_getProperties",
    "ScriptableObject의 직렬화된 프로퍼티 목록을 가져옵니다.",
    {
      assetPath: z.string().describe("에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("so.getProperties", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_setProperty",
    "ScriptableObject의 프로퍼티 값을 설정합니다.",
    {
      assetPath: z.string().describe("에셋 경로"),
      propertyName: z.string().describe("프로퍼티 이름"),
      value: z.union([z.string(), z.number(), z.boolean()]).describe("설정할 값"),
    },
    async (params) => {
      const result = await bridge.request("so.setProperty", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_duplicate",
    "ScriptableObject 에셋을 복제합니다.",
    {
      assetPath: z.string().describe("원본 에셋 경로"),
      newName: z.string().optional().describe("새 이름 (생략 시 _Copy 접미사)"),
    },
    async (params) => {
      const result = await bridge.request("so.duplicate", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_delete",
    "ScriptableObject 에셋을 삭제합니다.",
    {
      assetPath: z.string().describe("삭제할 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("so.delete", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_toJson",
    "ScriptableObject를 JSON으로 직렬화합니다.",
    {
      assetPath: z.string().describe("에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("so.toJson", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_fromJson",
    "JSON 데이터를 ScriptableObject에 덮어씁니다.",
    {
      assetPath: z.string().describe("대상 에셋 경로"),
      json: z.string().describe("적용할 JSON 문자열"),
    },
    async (params) => {
      const result = await bridge.request("so.fromJson", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_getTypes",
    "프로젝트에서 사용 가능한 ScriptableObject 타입 목록을 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("so.getTypes", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_getInfo",
    "ScriptableObject 에셋의 기본 정보를 조회합니다.",
    {
      assetPath: z.string().describe("에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("so.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
