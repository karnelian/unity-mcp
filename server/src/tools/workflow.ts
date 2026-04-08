import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerWorkflowTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_workflow_begin",
    "워크플로 세션을 시작합니다. 이후 작업을 하나의 단위로 묶어 롤백할 수 있습니다.",
    {
      name: z.string().optional().describe("세션 이름 (예: 'Level Design Session')"),
    },
    async (params) => {
      const result = await bridge.request("workflow.beginSession", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_workflow_end",
    "현재 워크플로 세션을 종료합니다.",
    {},
    async () => {
      const result = await bridge.request("workflow.endSession", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_workflow_undo_session",
    "현재 세션의 모든 작업을 되돌립니다.",
    {},
    async () => {
      const result = await bridge.request("workflow.undoSession", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_workflow_undo_last",
    "마지막 작업 하나를 되돌립니다.",
    {},
    async () => {
      const result = await bridge.request("workflow.undoLast", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_workflow_status",
    "현재 워크플로 세션 상태를 조회합니다.",
    {},
    async () => {
      const result = await bridge.request("workflow.status", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

}
