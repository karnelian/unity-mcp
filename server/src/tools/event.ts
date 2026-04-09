import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerEventTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_event_listEvents",
    "List UnityEvents",
    {
      path: z.string(),
      componentType: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("event.listEvents", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_event_getListeners",
    "Get event listeners",
    {
      path: z.string(),
      eventName: z.string().describe("이벤트 필드명 (예: onClick, onValueChanged)"),
      componentType: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("event.getListeners", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_event_addListener",
    "Add event listener",
    {
      path: z.string(),
      eventName: z.string().describe("이벤트 필드명 (예: onClick)"),
      componentType: z.string().optional(),
      targetPath: z.string().describe("대상 GameObject 경로 (또는 'self')"),
      methodName: z.string(),
      argumentType: z.enum(["void", "int", "float", "string", "bool", "object"]).optional().describe("인자 타입 (기본: void)"),
      argumentValue: z.any().optional(),
    },
    async (params) => {
      const result = await bridge.request("event.addListener", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_event_removeListener",
    "Remove event listener",
    {
      path: z.string(),
      eventName: z.string(),
      componentType: z.string().optional(),
      index: z.number(),
    },
    async (params) => {
      const result = await bridge.request("event.removeListener", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_event_setListenerState",
    "Set listener state",
    {
      path: z.string(),
      eventName: z.string(),
      componentType: z.string().optional(),
      index: z.number(),
      state: z.enum(["Off", "RuntimeOnly", "EditorAndRuntime"]).describe("호출 상태"),
    },
    async (params) => {
      const result = await bridge.request("event.setListenerState", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
