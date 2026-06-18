import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

export function registerAsmdefTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_asmdef_create", "Create asmdef", {
    path: z.string(),
    name: z.string(),
    references: z.array(z.string()).optional(),
    includePlatforms: z.array(z.string()).optional(),
    excludePlatforms: z.array(z.string()).optional(),
    allowUnsafeCode: z.boolean().optional(),
    autoReferenced: z.boolean().optional(),
    defineConstraints: z.array(z.string()).optional(),
    noEngineReferences: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("asmdef.create", p);
    return textResult(r);
  });

  server.tool("unity_asmdef_getInfo", "Get asmdef info", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("asmdef.getInfo", p);
    return textResult(r);
  });

  server.tool("unity_asmdef_set", "Modify asmdef", {
    path: z.string(),
    references: z.array(z.string()).optional(),
    includePlatforms: z.array(z.string()).optional(),
    excludePlatforms: z.array(z.string()).optional(),
    allowUnsafeCode: z.boolean().optional(),
    autoReferenced: z.boolean().optional(),
    defineConstraints: z.array(z.string()).optional(),
  }, async (p) => {
    const r = await bridge.request("asmdef.set", p);
    return textResult(r);
  });

  server.tool("unity_asmdef_find", "Find asmdef files", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("asmdef.find", p);
    return textResult(r);
  });
}
