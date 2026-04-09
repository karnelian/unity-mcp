import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });
const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerCameraTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_camera_create", "Create Camera", {
    name: z.string().optional(), orthographic: z.boolean().optional(),
    fieldOfView: z.number().optional(), depth: z.number().optional(),
    position: vec3.optional(), rotation: vec3.optional(), parent: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.create", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_properties", "Set Camera properties", {
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

  server.tool("unity_camera_get_properties", "Get Camera properties", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.getProperties", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_find", "Find Cameras", {}, async () => {
    const result = await bridge.request("camera.find", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_look_at", "Camera look at target", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    target: vec3.optional(),
    targetObject: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.lookAt", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_screenshot", "Capture screenshot", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    width: z.number().optional().describe("너비 (기본: 1920)"),
    height: z.number().optional().describe("높이 (기본: 1080)"),
    savePath: z.string().optional(),
    superSize: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.screenshot", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_culling_mask", "Set culling mask", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    layers: z.array(z.string()).optional(),
    mask: z.number().optional().describe("비트마스크 직접 지정"),
  }, async (params) => {
    const result = await bridge.request("camera.setCullingMask", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_main", "Set main Camera", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.setMain", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_viewport", "Set viewport rect", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    x: z.number().optional(), y: z.number().optional(),
    width: z.number().optional(), height: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.setViewport", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_set_clip_planes", "Set clip planes", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    near: z.number().optional(), far: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("camera.setClipPlanes", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_camera_get_main", "Get main Camera", {}, async () => {
    const result = await bridge.request("camera.getMain", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });
}
