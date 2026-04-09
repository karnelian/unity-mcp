import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerDebugTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_debug_screenshot",
    "Take screenshot",
    {
      view: z.enum(["game", "scene", "both"]).optional().describe("캡처 대상 (기본: game)"),
      width: z.number().optional(),
      height: z.number().optional(),
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
    "Log to console",
    {
      message: z.string(),
      level: z.enum(["info", "warning", "error"]).optional().describe("로그 레벨 (기본: info)"),
    },
    async (params) => {
      const result = await bridge.request("debug.log", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_clearConsole",
    "Clear console",
    {},
    async (params) => {
      const result = await bridge.request("debug.clearConsole", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getPrefs",
    "Get PlayerPrefs",
    {
      key: z.string(),
      type: z.enum(["string", "int", "float"]).optional().describe("값 타입 (기본: string)"),
    },
    async (params) => {
      const result = await bridge.request("debug.getPrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_setPrefs",
    "Set PlayerPrefs",
    {
      key: z.string(),
      value: z.union([z.string(), z.number()]),
      type: z.enum(["string", "int", "float"]).optional().describe("값 타입 (기본: string)"),
    },
    async (params) => {
      const result = await bridge.request("debug.setPrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_deletePrefs",
    "Delete PlayerPrefs",
    {
      key: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.deletePrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getEditorPrefs",
    "Get EditorPrefs",
    {
      key: z.string(),
      type: z.enum(["string", "int", "float", "bool"]).optional().describe("값 타입 (기본: string)"),
    },
    async (params) => {
      const result = await bridge.request("debug.getEditorPrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_setEditorPrefs",
    "Set EditorPrefs",
    {
      key: z.string(),
      value: z.union([z.string(), z.number(), z.boolean()]),
      type: z.enum(["string", "int", "float", "bool"]).optional().describe("값 타입 (기본: string)"),
    },
    async (params) => {
      const result = await bridge.request("debug.setEditorPrefs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_drawGizmo",
    "Draw debug gizmo",
    {
      type: z.enum(["line", "ray"]).describe("기즈모 타입"),
      from: vec3,
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
    "Get SystemInfo",
    {},
    async (params) => {
      const result = await bridge.request("debug.getSystemInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_startCapture",
    "Start log capture",
    {},
    async (params) => {
      const result = await bridge.request("debug.startCapture", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_stopCapture",
    "Stop log capture",
    {},
    async (params) => {
      const result = await bridge.request("debug.stopCapture", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getCapturedLogs",
    "Get captured logs",
    {
      type: z.string().optional().describe("로그 타입 필터 (Log, Warning, Error)"),
      count: z.number().optional().describe("최대 조회 수 (기본: 200)"),
      clear: z.boolean().optional(),
      includeStackTrace: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.getCapturedLogs", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_getDefines",
    "Get define symbols",
    {},
    async (params) => {
      const result = await bridge.request("debug.getDefines", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_setDefines",
    "Set define symbols",
    {
      action: z.enum(["add", "remove"]).describe("추가 또는 제거"),
      symbol: z.string(),
    },
    async (params) => {
      const result = await bridge.request("debug.setDefines", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_debug_forceRecompile",
    "Force recompile",
    {},
    async (params) => {
      const result = await bridge.request("debug.forceRecompile", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
