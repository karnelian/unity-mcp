import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerScriptTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_script_create",
    "Create C# script",
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
    "Read script",
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
    "Edit script",
    {
      path: z.string(),
      content: z.string().optional(),
      lineEdits: z.array(z.object({
        line: z.number(),
        action: z.enum(["replace", "insert", "delete"]),
        text: z.string().optional(),
      })).optional(),
    },
    async (params) => {
      const result = await bridge.request("script.edit", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_compileCheck",
    "Check compilation",
    {},
    async () => {
      const result = await bridge.request("script.compileCheck");
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_list",
    "List scripts",
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
    "Delete script",
    {
      path: z.string(),
    },
    async (params) => {
      const result = await bridge.request("script.delete", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_rename",
    "Rename script",
    {
      path: z.string(),
      newName: z.string(),
    },
    async (params) => {
      const result = await bridge.request("script.rename", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_search",
    "Search in scripts",
    {
      pattern: z.string(),
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
    "Get script type info",
    {
      typeName: z.string().describe("타입 이름 (예: PlayerController)"),
    },
    async (params) => {
      const result = await bridge.request("script.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
