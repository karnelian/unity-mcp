import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerShaderTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_shader_find", "Find shaders", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("shader.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_properties", "Get shader properties", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.getProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_keywords", "Get shader keywords", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.getKeywords", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_info", "Get shader info", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_list_all", "List shaders", {}, async () => {
    const r = await bridge.request("shader.listAll", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_source", "Get shader source", {
    shaderPath: z.string(),
    maxLines: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("shader.getSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_pass_count", "Get shader pass count", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.getPassCount", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_is_supported", "Check shader support", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.isSupported", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_find_materials", "Find shader materials", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.findMaterials", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_set_global_float", "Set global shader float", {
    property: z.string(), value: z.number(),
  }, async (p) => {
    const r = await bridge.request("shader.setGlobalFloat", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_global_properties", "Get global shader properties", {
    properties: z.array(z.string()),
  }, async (p) => {
    const r = await bridge.request("shader.getGlobalProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
