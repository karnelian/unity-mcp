import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerWorkflowTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_workflow_begin",
    "Begin workflow",
    {
      name: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("workflow.beginSession", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_workflow_end",
    "End workflow",
    {},
    async () => {
      const result = await bridge.request("workflow.endSession", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_workflow_undo_session",
    "Undo entire session",
    {},
    async () => {
      const result = await bridge.request("workflow.undoSession", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_workflow_undo_last",
    "Undo last action",
    {},
    async () => {
      const result = await bridge.request("workflow.undoLast", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_workflow_status",
    "Get workflow status",
    {},
    async () => {
      const result = await bridge.request("workflow.status", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

}
