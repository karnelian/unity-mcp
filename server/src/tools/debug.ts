import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerDebugTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_debug_screenshot",
    "Game/Scene View의 스크린샷을 캡처합니다. base64 PNG로 반환.",
    {
      view: z.enum(["game", "scene", "both"]).optional().describe("캡처 대상 (기본: game)"),
      width: z.number().optional().describe("가로 해상도"),
      height: z.number().optional().describe("세로 해상도"),
    },
    async (params) => {
      const result = await bridge.request("debug.screenshot", params);
      if (result?.data) {
        return {
          content: [
            { type: "image", data: result.data, mimeType: "image/png" },
            { type: "text", text: `Screenshot captured: ${result.view} (${result.width}x${result.height})` },
          ],
        };
      }
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_log",
    "Unity 콘솔에 로그를 출력합니다.",
    {
      message: z.string().describe("로그 메시지"),
      level: z.enum(["info", "warning", "error"]).optional().describe("로그 레벨 (기본: info)"),
    },
    async (params) => {
      const result = await bridge.request("debug.log", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_clearConsole",
    "Unity 콘솔을 클리어합니다.",
    {},
    async (params) => {
      const result = await bridge.request("debug.clearConsole", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getPrefs",
    "PlayerPrefs 값을 조회합니다.",
    {
      key: z.string().describe("키 이름"),
      type: z.enum(["string", "int", "float"]).optional().describe("값 타입 (기본: string)"),
    },
    async (params) => {
      const result = await bridge.request("debug.getPrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_setPrefs",
    "PlayerPrefs 값을 설정합니다.",
    {
      key: z.string().describe("키 이름"),
      value: z.union([z.string(), z.number()]).describe("설정할 값"),
      type: z.enum(["string", "int", "float"]).optional().describe("값 타입 (기본: string)"),
    },
    async (params) => {
      const result = await bridge.request("debug.setPrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_deletePrefs",
    "PlayerPrefs 키를 삭제합니다 (키 생략 시 전체 삭제).",
    {
      key: z.string().optional().describe("키 이름 (생략 시 전체 삭제)"),
    },
    async (params) => {
      const result = await bridge.request("debug.deletePrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getEditorPrefs",
    "EditorPrefs 값을 조회합니다.",
    {
      key: z.string().describe("키 이름"),
      type: z.enum(["string", "int", "float", "bool"]).optional().describe("값 타입 (기본: string)"),
    },
    async (params) => {
      const result = await bridge.request("debug.getEditorPrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_setEditorPrefs",
    "EditorPrefs 값을 설정합니다.",
    {
      key: z.string().describe("키 이름"),
      value: z.union([z.string(), z.number(), z.boolean()]).describe("설정할 값"),
      type: z.enum(["string", "int", "float", "bool"]).optional().describe("값 타입 (기본: string)"),
    },
    async (params) => {
      const result = await bridge.request("debug.setEditorPrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_drawGizmo",
    "Scene View에 디버그 라인/레이를 그립니다.",
    {
      type: z.enum(["line", "ray"]).describe("기즈모 타입"),
      from: vec3.describe("시작점"),
      to: vec3.optional().describe("끝점 (ray인 경우 방향)"),
      duration: z.number().optional().describe("표시 시간 초 (기본: 5)"),
    },
    async (params) => {
      const result = await bridge.request("debug.drawGizmo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getSystemInfo",
    "시스템 정보를 조회합니다 (CPU, GPU, OS, Unity 버전 등).",
    {},
    async (params) => {
      const result = await bridge.request("debug.getSystemInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_startCapture",
    "로그 캡처를 시작합니다. 타임스탬프와 함께 모든 로그를 기록합니다.",
    {},
    async (params) => {
      const result = await bridge.request("debug.startCapture", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_stopCapture",
    "로그 캡처를 중지합니다.",
    {},
    async (params) => {
      const result = await bridge.request("debug.stopCapture", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getCapturedLogs",
    "캡처된 로그를 조회합니다.",
    {
      type: z.string().optional().describe("로그 타입 필터 (Log, Warning, Error)"),
      count: z.number().optional().describe("최대 조회 수 (기본: 200)"),
      clear: z.boolean().optional().describe("조회 후 캡처 버퍼 클리어"),
      includeStackTrace: z.boolean().optional().describe("스택 트레이스 포함"),
    },
    async (params) => {
      const result = await bridge.request("debug.getCapturedLogs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getDefines",
    "현재 플랫폼의 스크립팅 디파인 심볼을 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("debug.getDefines", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_setDefines",
    "스크립팅 디파인 심볼을 추가하거나 제거합니다.",
    {
      action: z.enum(["add", "remove"]).describe("추가 또는 제거"),
      symbol: z.string().describe("디파인 심볼 이름"),
    },
    async (params) => {
      const result = await bridge.request("debug.setDefines", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_forceRecompile",
    "스크립트 강제 재컴파일을 요청합니다.",
    {},
    async (params) => {
      const result = await bridge.request("debug.forceRecompile", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
