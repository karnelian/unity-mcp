import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerComputeShaderTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_computeShader_find", "Find ComputeShader assets in the project.", {
    nameFilter: z.string().optional(),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("computeShader.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_computeShader_getInfo", "Get ComputeShader information (kernels, properties).", {
    path: z.string().describe("ComputeShader asset path"),
  }, async (p) => {
    const r = await bridge.request("computeShader.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_computeShader_getSource", "Read ComputeShader source code.", {
    path: z.string().describe("ComputeShader asset path"),
  }, async (p) => {
    const r = await bridge.request("computeShader.getSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_computeShader_dispatch", "Dispatch a ComputeShader kernel (editor only).", {
    path: z.string().describe("ComputeShader asset path"),
    kernel: z.string().describe("Kernel name"),
    threadGroupsX: z.number().describe("Thread groups X"),
    threadGroupsY: z.number().optional().describe("Thread groups Y (default: 1)"),
    threadGroupsZ: z.number().optional().describe("Thread groups Z (default: 1)"),
  }, async (p) => {
    const r = await bridge.request("computeShader.dispatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
