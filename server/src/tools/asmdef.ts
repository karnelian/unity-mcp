import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAsmdefTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_asmdef_create", "Create an Assembly Definition (.asmdef) file.", {
    path: z.string().describe("File path (e.g., Assets/Scripts/MyAssembly/MyAssembly.asmdef)"),
    name: z.string().describe("Assembly name"),
    references: z.array(z.string()).optional().describe("Referenced assembly names"),
    includePlatforms: z.array(z.string()).optional(),
    excludePlatforms: z.array(z.string()).optional(),
    allowUnsafeCode: z.boolean().optional(),
    autoReferenced: z.boolean().optional(),
    defineConstraints: z.array(z.string()).optional(),
    noEngineReferences: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("asmdef.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asmdef_getInfo", "Get Assembly Definition information.", {
    path: z.string().describe("Asmdef asset path"),
  }, async (p) => {
    const r = await bridge.request("asmdef.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asmdef_set", "Modify an Assembly Definition.", {
    path: z.string().describe("Asmdef asset path"),
    references: z.array(z.string()).optional(),
    includePlatforms: z.array(z.string()).optional(),
    excludePlatforms: z.array(z.string()).optional(),
    allowUnsafeCode: z.boolean().optional(),
    autoReferenced: z.boolean().optional(),
    defineConstraints: z.array(z.string()).optional(),
  }, async (p) => {
    const r = await bridge.request("asmdef.set", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_asmdef_find", "Find all Assembly Definition files in the project.", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("asmdef.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
