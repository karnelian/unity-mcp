import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerPackageTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_package_list",
    "List packages",
    {
      includeIndirect: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("package.list", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_info",
    "Get package info",
    {
      packageName: z.string(),
    },
    async (params) => {
      const result = await bridge.request("package.info", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_add",
    "Add package",
    {
      identifier: z.string(),
    },
    async (params) => {
      const result = await bridge.request("package.add", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_remove",
    "Remove package",
    {
      packageName: z.string(),
    },
    async (params) => {
      const result = await bridge.request("package.remove", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_search",
    "Search packages",
    {
      query: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("package.search", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_getVersion",
    "Get package version",
    {
      packageName: z.string(),
    },
    async (params) => {
      const result = await bridge.request("package.getVersion", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_listBuiltIn",
    "List built-in packages",
    {},
    async (params) => {
      const result = await bridge.request("package.listBuiltIn", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_resolve",
    "Resolve packages",
    {},
    async (params) => {
      const result = await bridge.request("package.resolve", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
