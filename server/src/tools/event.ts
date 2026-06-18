import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

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
      return textResult(result);
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
      return textResult(result);
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
      return textResult(result);
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
      return textResult(result);
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
      return textResult(result);
    }
  );
}
