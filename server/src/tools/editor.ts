import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

export function registerEditorTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_editor_playMode",
    "Control play mode",
    {
      action: z.enum(["play", "stop", "pause", "step", "status"]),
    },
    async (params) => {
      const result = await bridge.request("editor.playMode", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_build",
    "Build project",
    {
      target: z.enum(["Windows", "Android", "iOS", "WebGL", "macOS", "Linux"]),
      scenes: z.array(z.string()).optional(),
      outputPath: z.string().optional(),
      options: z.array(z.string()).optional(),
    },
    async (params) => {
      const result = await bridge.request("editor.build", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_buildSettings",
    "Get/set build settings",
    {
      action: z.enum(["get", "set"]),
      settings: z.record(z.string(), z.any()).optional(),
    },
    async (params) => {
      const result = await bridge.request("editor.buildSettings", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_executeMenu",
    "Execute menu item",
    {
      menuPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("editor.executeMenu", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_runTests",
    "Run tests",
    {
      mode: z.enum(["EditMode", "PlayMode"]).optional(),
      filter: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("editor.runTests", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_testList",
    "List tests",
    {
      mode: z.enum(["EditMode", "PlayMode"]).optional(),
    },
    async (params) => {
      const result = await bridge.request("editor.testList", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_testResults",
    "Get test results",
    {},
    async (params) => {
      const result = await bridge.request("editor.testResults", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_connectionStatus",
    "Check connection status",
    {},
    async () => {
      const status = bridge.getStatus();
      if (status.connected) {
        try {
          const info = await bridge.request("editor.projectInfo", {});
          return textResult({ ...status, ...info });
        } catch {
          return textResult(status);
        }
      }
      return textResult(status);
    }
  );

  server.tool(
    "unity_editor_console",
    "Get console logs",
    {
      type: z.enum(["error", "warning", "log", "all"]).optional(),
      count: z.number().optional(),
      clear: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("editor.console", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_projectInfo",
    "Get project info",
    {},
    async (params) => {
      const result = await bridge.request("editor.projectInfo", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_diagnostics",
    "Get editor diagnostics",
    {},
    async () => {
      const result = await bridge.request("editor.diagnostics", {});
      return textResult(result);
    }
  );

  server.tool(
    "unity_editor_autoRefresh",
    "Control Unity Auto Refresh. Pause before parallel agent work, resume after completion to trigger single recompile.",
    {
      action: z.enum(["pause", "resume", "status"]).describe("pause: disable auto-refresh, resume: re-enable + recompile, status: check current state"),
    },
    async (p) => {
      const result = await bridge.request("editor.autoRefresh", p);
      return textResult(result);
    }
  );

}
