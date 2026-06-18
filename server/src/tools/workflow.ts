import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";
import { describeSafetyControls, withSafety } from "./safety.js";

export function registerWorkflowTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_workflow_begin",
    "Begin workflow",
    withSafety({
      name: z.string().optional(),
    }),
    async (params) => {
      const result = await bridge.request("workflow.beginSession", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_workflow_end",
    "End workflow",
    withSafety({}),
    async (params) => {
      const result = await bridge.request("workflow.endSession", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_workflow_undo_session",
    "Undo entire session",
    withSafety({}),
    async (params) => {
      const result = await bridge.request("workflow.undoSession", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_workflow_undo_last",
    "Undo last action",
    withSafety({}),
    async (params) => {
      const result = await bridge.request("workflow.undoLast", params);
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

  server.tool(
    "unity_mcp_safety_manifest",
    "List public Unity MCP tools that expose dryRun and confirmationToken, with internal method, risk level, and expected confirmation token when high-risk. Works without a live Unity connection.",
    {},
    async () => textResult(describeSafetyControls())
  );
}
