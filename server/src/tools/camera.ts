import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });
const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerCameraTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_camera_create", "Camera를 생성합니다.", {
    name: z.string().optional(), orthographic: z.boolean().optional(),
    fieldOfView: z.number().optional(), depth: z.number().optional(),
    position: vec3.optional(), rotation: vec3.optional(), parent: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.create", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_properties", "Camera 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    fieldOfView: z.number().optional(), orthographic: z.boolean().optional(),
    orthographicSize: z.number().optional(), depth: z.number().optional(),
    nearClipPlane: z.number().optional(), farClipPlane: z.number().optional(),
    clearFlags: z.enum(["Skybox", "SolidColor", "Depth", "Nothing"]).optional(),
    backgroundColor: color.optional(),
  }, async (params) => {
    const result = await bridge.request("camera.setProperties", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_get_properties", "Camera 프로퍼티를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.getProperties", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_find", "씬의 모든 Camera를 조회합니다.", {}, async () => {
    const result = await bridge.request("camera.find", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_look_at", "Camera가 특정 지점/오브젝트를 바라보게 합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    target: vec3.optional().describe("월드 좌표"),
    targetObject: z.string().optional().describe("대상 오브젝트 경로"),
  }, async (params) => {
    const result = await bridge.request("camera.lookAt", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_screenshot", "Camera로 스크린샷을 찍습니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    width: z.number().optional().describe("너비 (기본: 1920)"),
    height: z.number().optional().describe("높이 (기본: 1080)"),
    savePath: z.string().optional().describe("저장 경로"),
    superSize: z.number().optional().describe("슈퍼 샘플링 배수"),
  }, async (params) => {
    const result = await bridge.request("camera.screenshot", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_culling_mask", "Camera의 Culling Mask를 설정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    layers: z.array(z.string()).optional().describe("레이어 이름 배열"),
    mask: z.number().optional().describe("비트마스크 직접 지정"),
  }, async (params) => {
    const result = await bridge.request("camera.setCullingMask", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_main", "Camera를 MainCamera로 설정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.setMain", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_viewport", "Camera의 뷰포트 Rect를 설정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    x: z.number().optional(), y: z.number().optional(),
    width: z.number().optional(), height: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.setViewport", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_clip_planes", "Camera의 Near/Far Clip Plane을 설정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    near: z.number().optional(), far: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.setClipPlanes", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_get_main", "Main Camera 정보를 조회합니다.", {}, async () => {
    const result = await bridge.request("camera.getMain", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });
}
