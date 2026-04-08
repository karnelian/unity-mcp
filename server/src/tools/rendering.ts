import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });

export function registerRenderingTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_rendering_createVolume", "Volume을 생성합니다 (포스트프로세싱 등).", {
    name: z.string().optional(),
    isGlobal: z.boolean().optional().describe("전역 볼륨 (기본: true)"),
    priority: z.number().optional(),
    weight: z.number().optional(),
    profilePath: z.string().optional().describe("기존 VolumeProfile 에셋 경로"),
    position: vec3.optional(),
    size: vec3.optional().describe("로컬 볼륨 크기"),
  }, async (p) => {
    const r = await bridge.request("rendering.createVolume", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_getVolumeInfo", "Volume 정보를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.getVolumeInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_findVolumes", "씬의 Volume을 검색합니다.", {}, async () => {
    const r = await bridge.request("rendering.findVolumes", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_addOverride", "Volume에 오버라이드를 추가합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    overrideType: z.string().describe("오버라이드 타입 (예: Bloom, ColorAdjustments, Vignette, Tonemapping 등)"),
  }, async (p) => {
    const r = await bridge.request("rendering.addOverride", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_removeOverride", "Volume에서 오버라이드를 제거합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    overrideType: z.string().describe("제거할 오버라이드 타입"),
  }, async (p) => {
    const r = await bridge.request("rendering.removeOverride", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_setOverrideProperty", "Volume 오버라이드의 속성을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    overrideType: z.string().describe("오버라이드 타입 (예: Bloom)"),
    property: z.string().describe("속성 이름 (예: intensity, threshold, scatter)"),
    value: z.any().describe("값 (숫자, bool, {r,g,b,a} 등)"),
  }, async (p) => {
    const r = await bridge.request("rendering.setOverrideProperty", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_getOverrides", "사용 가능한 VolumeComponent 타입 목록을 조회합니다.", {}, async () => {
    const r = await bridge.request("rendering.getOverrides", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_getPipelineInfo", "현재 렌더 파이프라인 정보를 조회합니다.", {}, async () => {
    const r = await bridge.request("rendering.getPipelineInfo", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_setFog", "씬 포그를 설정합니다.", {
    enabled: z.boolean().optional(),
    color: color.optional(),
    mode: z.enum(["Linear", "Exponential", "ExponentialSquared"]).optional(),
    density: z.number().optional(),
    startDistance: z.number().optional(),
    endDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.setFog", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_getFog", "씬 포그 설정을 조회합니다.", {}, async () => {
    const r = await bridge.request("rendering.getFog", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_setSkybox", "스카이박스를 설정합니다.", {
    materialPath: z.string().optional().describe("스카이박스 머티리얼 경로"),
    ambientIntensity: z.number().optional(),
    reflectionIntensity: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.setSkybox", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_rendering_setGlobalShaderProperty", "전역 셰이더 프로퍼티를 설정합니다.", {
    property: z.string().describe("프로퍼티 이름"),
    floatValue: z.number().optional(),
    intValue: z.number().optional(),
    colorValue: color.optional(),
    vectorValue: z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number().optional() }).optional(),
  }, async (p) => {
    const r = await bridge.request("rendering.setGlobalShaderProperty", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
