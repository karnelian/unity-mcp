import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerScriptTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_script_create",
    "Create a C# script file. Use templates (MonoBehaviour, ScriptableObject, Editor, Interface, Struct, Enum, Static) or provide custom code directly.",
    {
      path: z.string().describe("저장 경로 (예: 'Assets/Scripts/PlayerController.cs')"),
      template: z.enum(["MonoBehaviour", "ScriptableObject", "Editor", "Interface", "Struct", "Enum", "Static"]).optional().describe("템플릿 타입"),
      code: z.string().optional().describe("직접 작성할 코드 (template 대신 사용)"),
    },
    async (params) => {
      const result = await bridge.request("script.create", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_read",
    "Read the contents of a C# script file. Use this to understand existing code before editing.",
    {
      path: z.string().describe("스크립트 경로 (예: 'Assets/Scripts/Player.cs')"),
    },
    async (params) => {
      const result = await bridge.request("script.read", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_edit",
    "스크립트를 수정합니다. 전체 교체 또는 줄번호 기반 편집.",
    {
      path: z.string().describe("스크립트 경로"),
      content: z.string().optional().describe("전체 교체할 코드"),
      lineEdits: z.array(z.object({
        line: z.number().describe("줄 번호 (1-based)"),
        action: z.enum(["replace", "insert", "delete"]),
        text: z.string().optional().describe("교체/삽입할 텍스트"),
      })).optional().describe("줄번호 기반 편집 목록"),
    },
    async (params) => {
      const result = await bridge.request("script.edit", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_compileCheck",
    "현재 컴파일 상태와 에러를 확인합니다.",
    {},
    async () => {
      const result = await bridge.request("script.compileCheck");
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_list",
    "프로젝트의 스크립트 파일 목록을 조회합니다.",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: 'Assets')"),
      pattern: z.string().optional().describe("파일명 패턴 (예: '*Controller*')"),
      recursive: z.boolean().optional().describe("하위 폴더 포함 (기본: true)"),
    },
    async (params) => {
      const result = await bridge.request("script.list", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_delete",
    "스크립트 파일을 삭제합니다.",
    {
      path: z.string().describe("삭제할 스크립트 경로"),
    },
    async (params) => {
      const result = await bridge.request("script.delete", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_rename",
    "스크립트 파일 이름을 변경합니다.",
    {
      path: z.string().describe("현재 스크립트 경로"),
      newName: z.string().describe("새 이름 (확장자 제외)"),
    },
    async (params) => {
      const result = await bridge.request("script.rename", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_search",
    "스크립트 파일 내용에서 텍스트를 검색합니다 (grep).",
    {
      pattern: z.string().describe("검색 패턴"),
      folder: z.string().optional().describe("검색 폴더 (기본: 'Assets')"),
      caseSensitive: z.boolean().optional().describe("대소문자 구분 (기본: false)"),
      maxResults: z.number().optional().describe("최대 결과 수 (기본: 50)"),
    },
    async (params) => {
      const result = await bridge.request("script.search", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_getInfo",
    "리플렉션으로 스크립트 타입 정보를 조회합니다 (필드, 메서드, 상속).",
    {
      typeName: z.string().describe("타입 이름 (예: PlayerController)"),
    },
    async (params) => {
      const result = await bridge.request("script.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
