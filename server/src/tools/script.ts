import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerScriptTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_script_create",
    "Create C# script",
    {
      path: z.string(),
      template: z.enum(["MonoBehaviour", "ScriptableObject", "Editor", "Interface", "Struct", "Enum", "Static"]).optional(),
      code: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("script.create", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_read",
    "Read script",
    {
      path: z.string(),
    },
    async (params) => {
      const result = await bridge.request("script.read", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_edit",
    "Edit script",
    {
      path: z.string(),
      content: z.string().optional(),
      lineEdits: z.array(z.object({
        line: z.number(),
        action: z.enum(["replace", "insert", "delete"]),
        text: z.string().optional(),
      })).optional(),
    },
    async (params) => {
      const result = await bridge.request("script.edit", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_compileCheck",
    "Check compilation",
    {},
    async () => {
      const result = await bridge.request("script.compileCheck");
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_list",
    "List scripts",
    {
      folder: z.string().optional(),
      pattern: z.string().optional(),
      recursive: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("script.list", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_delete",
    "Delete script",
    {
      path: z.string(),
    },
    async (params) => {
      const result = await bridge.request("script.delete", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_rename",
    "Rename script",
    {
      path: z.string(),
      newName: z.string(),
    },
    async (params) => {
      const result = await bridge.request("script.rename", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_search",
    "Search in scripts",
    {
      pattern: z.string(),
      folder: z.string().optional(),
      caseSensitive: z.boolean().optional(),
      maxResults: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("script.search", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_script_getInfo",
    "Get script type info",
    {
      typeName: z.string(),
    },
    async (params) => {
      const result = await bridge.request("script.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
