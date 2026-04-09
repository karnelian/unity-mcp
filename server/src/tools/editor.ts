import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerEditorTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_editor_playMode",
    "Control play mode",
    {
      action: z.enum(["play", "stop", "pause", "step", "status"]),
    },
    async (params) => {
      const result = await bridge.request("editor.playMode", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_testResults",
    "Get test results",
    {},
    async (params) => {
      const result = await bridge.request("editor.testResults", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
          return { content: [{ type: "text", text: JSON.stringify({ ...status, ...info }, null, 2) }] };
        } catch {
          return { content: [{ type: "text", text: JSON.stringify(status, null, 2) }] };
        }
      }
      return { content: [{ type: "text", text: JSON.stringify(status, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_projectInfo",
    "Get project info",
    {},
    async (params) => {
      const result = await bridge.request("editor.projectInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_diagnostics",
    "Get editor diagnostics",
    {},
    async () => {
      const result = await bridge.request("editor.diagnostics", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

}
