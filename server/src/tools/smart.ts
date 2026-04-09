import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerSmartTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_smart_sceneQuery",
    "Query scene objects",
    {
      query: z.string(),
      limit: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("smart.sceneQuery", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_smart_referenceBind",
    "Bind references",
    {
      path: z.string(),
      componentType: z.string(),
      fieldName: z.string(),
      matchBy: z.enum(["tag", "name", "component"]).optional(),
      pattern: z.string(),
    },
    async (params) => {
      const result = await bridge.request("smart.referenceBind", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
