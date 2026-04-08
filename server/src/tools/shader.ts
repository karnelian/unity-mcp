import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerShaderTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_shader_find", "셰이더를 검색합니다.", {
    nameFilter: z.string().optional().describe("이름 필터"),
  }, async (p) => {
    const r = await bridge.request("shader.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_properties", "셰이더의 프로퍼티를 조회합니다.", {
    shaderName: z.string().describe("셰이더 이름"),
  }, async (p) => {
    const r = await bridge.request("shader.getProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_keywords", "셰이더의 키워드를 조회합니다.", {
    shaderName: z.string().describe("셰이더 이름"),
  }, async (p) => {
    const r = await bridge.request("shader.getKeywords", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_info", "셰이더 정보를 조회합니다.", {
    shaderName: z.string().describe("셰이더 이름"),
  }, async (p) => {
    const r = await bridge.request("shader.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_list_all", "프로젝트의 모든 셰이더를 조회합니다.", {}, async () => {
    const r = await bridge.request("shader.listAll", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_source", "셰이더 소스 코드를 조회합니다.", {
    shaderPath: z.string().describe("셰이더 파일 경로"),
    maxLines: z.number().optional().describe("최대 줄 수 (기본: 200)"),
  }, async (p) => {
    const r = await bridge.request("shader.getSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_pass_count", "셰이더의 Pass 수를 조회합니다.", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.getPassCount", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_is_supported", "셰이더가 현재 하드웨어에서 지원되는지 확인합니다.", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.isSupported", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_find_materials", "특정 셰이더를 사용하는 머티리얼을 검색합니다.", {
    shaderName: z.string(),
  }, async (p) => {
    const r = await bridge.request("shader.findMaterials", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_set_global_float", "전역 셰이더 float를 설정합니다.", {
    property: z.string(), value: z.number(),
  }, async (p) => {
    const r = await bridge.request("shader.setGlobalFloat", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_shader_get_global_properties", "전역 셰이더 프로퍼티 값을 조회합니다.", {
    properties: z.array(z.string()).describe("조회할 프로퍼티 이름 배열"),
  }, async (p) => {
    const r = await bridge.request("shader.getGlobalProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
