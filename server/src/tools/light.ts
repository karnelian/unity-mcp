import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });
const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerLightTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_light_create", "Light를 생성합니다.", {
    name: z.string().optional().describe("이름 (기본: New Light)"),
    type: z.enum(["Directional", "Point", "Spot", "Area"]).optional().describe("라이트 타입"),
    color: color.optional(), intensity: z.number().optional(), range: z.number().optional(),
    spotAngle: z.number().optional(), position: vec3.optional(), rotation: vec3.optional(),
    parent: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("light.create", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_properties", "Light 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    type: z.enum(["Directional", "Point", "Spot", "Area"]).optional(),
    color: color.optional(), intensity: z.number().optional(), range: z.number().optional(),
    spotAngle: z.number().optional(), enabled: z.boolean().optional(),
  }, async (params) => {
    const result = await bridge.request("light.setProperties", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_get_properties", "Light 프로퍼티를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("light.getProperties", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_find", "씬의 모든 Light를 검색합니다.", {
    type: z.enum(["Directional", "Point", "Spot", "Area"]).optional().describe("타입 필터"),
  }, async (params) => {
    const result = await bridge.request("light.find", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_active", "Light 오브젝트를 활성화/비활성화합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    active: z.boolean().describe("활성화 여부"),
  }, async (params) => {
    const result = await bridge.request("light.setActive", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_delete", "Light 오브젝트를 삭제합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("light.delete", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_bake_type", "Light의 Lightmap Bake Type을 설정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    bakeType: z.enum(["Realtime", "Mixed", "Baked"]).describe("베이크 타입"),
  }, async (params) => {
    const result = await bridge.request("light.setLightmapBakeType", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_shadows", "Light의 그림자 설정을 변경합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    shadowType: z.enum(["None", "Hard", "Soft"]).describe("그림자 타입"),
    strength: z.number().optional(), bias: z.number().optional(), normalBias: z.number().optional(),
    resolution: z.enum(["FromQualitySettings", "Low", "Medium", "High", "VeryHigh"]).optional(),
  }, async (params) => {
    const result = await bridge.request("light.setShadows", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_ambient", "씬의 Ambient Light 설정을 변경합니다.", {
    mode: z.enum(["Skybox", "Trilight", "Flat", "Custom"]).optional(),
    color: color.optional(), intensity: z.number().optional(),
    skyColor: color.optional(), equatorColor: color.optional(), groundColor: color.optional(),
  }, async (params) => {
    const result = await bridge.request("light.setAmbient", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_get_ambient", "씬의 Ambient Light 설정을 조회합니다.", {}, async () => {
    const result = await bridge.request("light.getAmbient", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_color_temperature", "라이트의 색온도를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    temperature: z.number().optional().describe("색온도 (Kelvin, 1000~20000)"),
    useColorTemperature: z.boolean().optional().describe("색온도 사용 여부"),
  }, async (p) => {
    const r = await bridge.request("light.setColorTemperature", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_set_cookie", "라이트 쿠키 텍스처를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    texturePath: z.string().describe("쿠키 텍스처 경로"),
    cookieSize: z.number().optional().describe("쿠키 크기 (Directional Light용)"),
  }, async (p) => {
    const r = await bridge.request("light.setCookie", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_set_culling_mask", "라이트의 컬링 마스크를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    cullingMask: z.number().optional().describe("비트마스크"),
    layers: z.array(z.string()).optional().describe("레이어 이름 배열"),
  }, async (p) => {
    const r = await bridge.request("light.setCullingMask", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_create_reflection_probe", "리플렉션 프로브를 생성합니다.", {
    name: z.string().optional(),
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    size: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional().describe("프로브 크기"),
    resolution: z.number().optional().describe("해상도 (128, 256, 512 등)"),
    mode: z.enum(["Baked", "Realtime", "Custom"]).optional(),
    intensity: z.number().optional(),
    boxProjection: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("light.createReflectionProbe", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_set_reflection_probe", "리플렉션 프로브 설정을 수정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    size: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    resolution: z.number().optional(), mode: z.enum(["Baked", "Realtime", "Custom"]).optional(),
    intensity: z.number().optional(), boxProjection: z.boolean().optional(),
    nearClipPlane: z.number().optional(), farClipPlane: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("light.setReflectionProbe", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_find_reflection_probes", "씬의 리플렉션 프로브를 검색합니다.", {}, async () => {
    const r = await bridge.request("light.findReflectionProbes", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_create_light_probe_group", "라이트 프로브 그룹을 생성합니다.", {
    name: z.string().optional(),
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    probePositions: z.array(z.object({ x: z.number(), y: z.number(), z: z.number() })).optional().describe("프로브 위치 배열"),
  }, async (p) => {
    const r = await bridge.request("light.createLightProbeGroup", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
