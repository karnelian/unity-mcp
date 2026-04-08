import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec4 = z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number().optional() });
const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerVFXTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_vfx_create", "Visual Effect 오브젝트를 생성합니다.", {
    name: z.string().optional(),
    position: vec3.optional(),
    assetPath: z.string().optional().describe("VFX Graph 에셋 경로"),
    playOnAwake: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("vfx.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_getInfo", "Visual Effect 정보를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("vfx.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_play", "Visual Effect를 재생합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    eventName: z.string().optional().describe("이벤트 이름 (미지정 시 기본 Play)"),
  }, async (p) => {
    const r = await bridge.request("vfx.play", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_stop", "Visual Effect를 정지합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    eventName: z.string().optional().describe("이벤트 이름 (미지정 시 기본 Stop)"),
  }, async (p) => {
    const r = await bridge.request("vfx.stop", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_setFloat", "VFX 프로퍼티(float)를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    property: z.string().describe("프로퍼티 이름"),
    value: z.number().describe("값"),
  }, async (p) => {
    const r = await bridge.request("vfx.setFloat", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_setInt", "VFX 프로퍼티(int)를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    property: z.string().describe("프로퍼티 이름"),
    value: z.number().describe("값"),
  }, async (p) => {
    const r = await bridge.request("vfx.setInt", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_setBool", "VFX 프로퍼티(bool)를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    property: z.string().describe("프로퍼티 이름"),
    value: z.boolean().describe("값"),
  }, async (p) => {
    const r = await bridge.request("vfx.setBool", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_setVector", "VFX 프로퍼티(Vector4)를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    property: z.string().describe("프로퍼티 이름"),
    value: vec4.describe("값 (x,y,z,w)"),
  }, async (p) => {
    const r = await bridge.request("vfx.setVector", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_vfx_find", "씬의 VisualEffect를 검색합니다.", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("vfx.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
