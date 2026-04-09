import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerScriptableObjectTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_so_create",
    "Create ScriptableObject",
    {
      typeName: z.string(),
      savePath: z.string(),
      name: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("so.create", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_find",
    "Find ScriptableObjects",
    {
      typeName: z.string().optional(),
      folder: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("so.find", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_getProperties",
    "Get SO properties",
    {
      assetPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("so.getProperties", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_setProperty",
    "Set SO property",
    {
      assetPath: z.string(),
      propertyName: z.string(),
      value: z.union([z.string(), z.number(), z.boolean()]),
    },
    async (params) => {
      const result = await bridge.request("so.setProperty", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_duplicate",
    "Duplicate ScriptableObject",
    {
      assetPath: z.string(),
      newName: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("so.duplicate", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_delete",
    "Delete ScriptableObject",
    {
      assetPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("so.delete", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_toJson",
    "Export SO to JSON",
    {
      assetPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("so.toJson", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_fromJson",
    "Import SO from JSON",
    {
      assetPath: z.string(),
      json: z.string(),
    },
    async (params) => {
      const result = await bridge.request("so.fromJson", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_getTypes",
    "Get SO types",
    {},
    async (params) => {
      const result = await bridge.request("so.getTypes", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_so_getInfo",
    "Get SO info",
    {
      assetPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("so.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
