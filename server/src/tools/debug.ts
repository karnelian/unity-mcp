import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerDebugTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_debug_visualQaBundle",
    "Capture a compact visual QA bundle: Console, Hierarchy, Inspector, and Game/Scene views. Use when diagnosing visual/editor state.",
    {
      windows: z.array(z.enum(["console", "hierarchy", "inspector", "project", "game", "scene"])).optional(),
      width: z.number().optional(),
      height: z.number().optional(),
      savePathPrefix: z.string().optional(),
    },
    async (params) => {
      const windows = params.windows ?? ["console", "hierarchy", "inspector", "game", "scene"];
      const content: Array<any> = [];
      const summary: Array<any> = [];

      for (const window of windows) {
        try {
          const result = window === "game" || window === "scene"
            ? await bridge.request("debug.screenshot", { view: window, width: params.width, height: params.height })
            : await bridge.request("debug.captureEditorWindow", {
                window,
                savePath: params.savePathPrefix ? `${params.savePathPrefix}-${window}.png` : undefined,
              });

          summary.push({ window, ok: true, width: result?.width, height: result?.height, savedTo: result?.savedTo });
          if (result?.data) {
            content.push({ type: "image", data: result.data, mimeType: "image/png" });
          }
        } catch (error) {
          summary.push({ window, ok: false, error: error instanceof Error ? error.message : String(error) });
        }
      }

      content.unshift({ type: "text", text: JSON.stringify({ captured: summary.length, summary }) });
      return { content };
    }
  );

  server.tool(
    "unity_debug_screenshot",
    "Take screenshot",
    {
      view: z.enum(["game", "scene", "both"]).optional(),
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
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_captureEditorWindow",
    "Capture EditorWindow (inspector/hierarchy/project/console/etc)",
    {
      window: z.string().optional(),
      savePath: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.captureEditorWindow", params);
      const label = `${result?.window} (${result?.width}x${result?.height})`;
      if (result?.data) {
        return {
          content: [
            { type: "image", data: result.data, mimeType: "image/png" },
            { type: "text", text: `Captured ${label}` },
          ],
        };
      }
      if (result?.savedTo) {
        return {
          content: [{ type: "text", text: `Captured ${label} → ${result.savedTo}` }],
        };
      }
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_log",
    "Log to console",
    {
      message: z.string(),
      level: z.enum(["info", "warning", "error"]).optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.log", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_clearConsole",
    "Clear console",
    {},
    async (params) => {
      const result = await bridge.request("debug.clearConsole", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_getPrefs",
    "Get PlayerPrefs",
    {
      key: z.string(),
      type: z.enum(["string", "int", "float"]).optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.getPrefs", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_setPrefs",
    "Set PlayerPrefs",
    {
      key: z.string(),
      value: z.union([z.string(), z.number()]),
      type: z.enum(["string", "int", "float"]).optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.setPrefs", params);
      return textResult(result);
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
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_getEditorPrefs",
    "Get EditorPrefs",
    {
      key: z.string(),
      type: z.enum(["string", "int", "float", "bool"]).optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.getEditorPrefs", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_setEditorPrefs",
    "Set EditorPrefs",
    {
      key: z.string(),
      value: z.union([z.string(), z.number(), z.boolean()]),
      type: z.enum(["string", "int", "float", "bool"]).optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.setEditorPrefs", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_drawGizmo",
    "Draw debug gizmo",
    {
      type: z.enum(["line", "ray"]),
      from: vec3,
      to: vec3.optional(),
      duration: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.drawGizmo", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_getSystemInfo",
    "Get SystemInfo",
    {},
    async (params) => {
      const result = await bridge.request("debug.getSystemInfo", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_startCapture",
    "Start log capture",
    {},
    async (params) => {
      const result = await bridge.request("debug.startCapture", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_stopCapture",
    "Stop log capture",
    {},
    async (params) => {
      const result = await bridge.request("debug.stopCapture", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_getCapturedLogs",
    "Get captured logs",
    {
      type: z.string().optional(),
      count: z.number().optional(),
      clear: z.boolean().optional(),
      includeStackTrace: z.boolean().optional(),
      maxResults: z.number().optional(),
      offset: z.number().optional(),
      summaryOnly: z.boolean().optional(),
      includeDetails: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("debug.getCapturedLogs", params);
      return textResult(result, params);
    }
  );

  server.tool(
    "unity_debug_getDefines",
    "Get define symbols",
    {},
    async (params) => {
      const result = await bridge.request("debug.getDefines", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_setDefines",
    "Set define symbols",
    {
      action: z.enum(["add", "remove"]),
      symbol: z.string(),
    },
    async (params) => {
      const result = await bridge.request("debug.setDefines", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_debug_forceRecompile",
    "Force recompile",
    {},
    async (params) => {
      const result = await bridge.request("debug.forceRecompile", params);
      return textResult(result);
    }
  );
}
