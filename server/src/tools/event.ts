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
      eventName: z.string(),
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
      eventName: z.string(),
      componentType: z.string().optional(),
      targetPath: z.string(),
      methodName: z.string(),
      argumentType: z.enum(["void", "int", "float", "string", "bool", "object"]).optional(),
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
      state: z.enum(["Off", "RuntimeOnly", "EditorAndRuntime"]),
    },
    async (params) => {
      const result = await bridge.request("event.setListenerState", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
