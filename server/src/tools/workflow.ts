import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

export function registerWorkflowTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_workflow_begin",
    "Begin workflow",
    {
      name: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("workflow.beginSession", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_workflow_end",
    "End workflow",
    {},
    async () => {
      const result = await bridge.request("workflow.endSession", {});
      return textResult(result);
    }
  );

  server.tool(
    "unity_workflow_undo_session",
    "Undo entire session",
    {},
    async () => {
      const result = await bridge.request("workflow.undoSession", {});
      return textResult(result);
    }
  );

  server.tool(
    "unity_workflow_undo_last",
    "Undo last action",
    {},
    async () => {
      const result = await bridge.request("workflow.undoLast", {});
      return textResult(result);
    }
  );

  server.tool(
    "unity_workflow_status",
    "Get workflow status",
    {},
    async () => {
      const result = await bridge.request("workflow.status", {});
      return textResult(result);
    }
  );

  server.tool(
    "unity_mcp_safety_describe",
    "Describe central MCP safety policy, risk metadata, dry-run behavior, and confirmation token for an internal Unity method such as scene.delete or project.setBuildTarget",
    {
      method: z.string(),
      params: z.record(z.string(), z.any()).optional(),
    },
    async (params) => {
      const result = await bridge.request("safety.describe", params);
      return textResult(result);
    }
  );
}
