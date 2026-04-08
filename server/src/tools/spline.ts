import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const quat = z.object({ x: z.number(), y: z.number(), z: z.number(), w: z.number() });

export function registerSplineTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_spline_create", "Spline(SplineContainer)을 생성합니다.", {
    name: z.string().optional(),
    position: vec3.optional(),
    closed: z.boolean().optional().describe("닫힌 스플라인 (기본: false)"),
    knots: z.array(vec3).optional().describe("초기 노트 위치 배열"),
  }, async (p) => {
    const r = await bridge.request("spline.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_getInfo", "Spline 정보를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_addKnot", "Spline에 노트를 추가합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    x: z.number(), y: z.number(), z: z.number(),
    index: z.number().optional().describe("삽입 위치 (미지정 시 끝에 추가)"),
    tangentIn: vec3.optional(), tangentOut: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("spline.addKnot", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_removeKnot", "Spline에서 노트를 제거합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    index: z.number().describe("제거할 노트 인덱스"),
  }, async (p) => {
    const r = await bridge.request("spline.removeKnot", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_setKnot", "Spline 노트의 위치/탄젠트/회전을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    index: z.number().describe("노트 인덱스"),
    position: vec3.optional(), tangentIn: vec3.optional(), tangentOut: vec3.optional(),
    rotation: quat.optional(),
  }, async (p) => {
    const r = await bridge.request("spline.setKnot", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_getKnots", "Spline의 모든 노트를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.getKnots", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_setTangentMode", "노트의 탄젠트 모드를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    index: z.number().describe("노트 인덱스"),
    mode: z.enum(["AutoSmooth", "Linear", "Broken"]).describe("탄젠트 모드"),
  }, async (p) => {
    const r = await bridge.request("spline.setTangentMode", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_find", "씬의 SplineContainer를 검색합니다.", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_addExtrude", "SplineExtrude 컴포넌트를 추가합니다 (메시 생성).", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    radius: z.number().optional(), segments: z.number().optional(),
    sides: z.number().optional(), capped: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("spline.addExtrude", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_addAnimate", "SplineAnimate 컴포넌트를 추가합니다 (경로 이동).", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    duration: z.number().optional(),
    loop: z.enum(["Once", "Loop", "PingPong"]).optional(),
    easingMode: z.enum(["None", "EaseIn", "EaseOut", "EaseInOut"]).optional(),
  }, async (p) => {
    const r = await bridge.request("spline.addAnimate", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spline_addInstantiate", "SplineInstantiate 컴포넌트를 추가합니다 (스플라인 위 오브젝트 배치).", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    prefabPath: z.string().optional().describe("배치할 프리팹 경로"),
    spacing: z.number().optional().describe("간격"),
  }, async (p) => {
    const r = await bridge.request("spline.addInstantiate", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
