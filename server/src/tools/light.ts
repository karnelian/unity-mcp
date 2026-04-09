import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });
const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerLightTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_light_create", "Create Light", {
    name: z.string().optional(),
    type: z.enum(["Directional", "Point", "Spot", "Area"]).optional(),
    color: color.optional(), intensity: z.number().optional(), range: z.number().optional(),
    spotAngle: z.number().optional(), position: vec3.optional(), rotation: vec3.optional(),
    parent: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("light.create", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_properties", "Set Light properties", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    type: z.enum(["Directional", "Point", "Spot", "Area"]).optional(),
    color: color.optional(), intensity: z.number().optional(), range: z.number().optional(),
    spotAngle: z.number().optional(), enabled: z.boolean().optional(),
  }, async (params) => {
    const result = await bridge.request("light.setProperties", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_get_properties", "Get Light properties", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("light.getProperties", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_find", "Find Lights", {
    type: z.enum(["Directional", "Point", "Spot", "Area"]).optional(),
  }, async (params) => {
    const result = await bridge.request("light.find", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_active", "Set Light active", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    active: z.boolean(),
  }, async (params) => {
    const result = await bridge.request("light.setActive", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_delete", "Delete Light", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("light.delete", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_bake_type", "Set Light bake type", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    bakeType: z.enum(["Realtime", "Mixed", "Baked"]),
  }, async (params) => {
    const result = await bridge.request("light.setLightmapBakeType", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_shadows", "Set Light shadows", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    shadowType: z.enum(["None", "Hard", "Soft"]),
    strength: z.number().optional(), bias: z.number().optional(), normalBias: z.number().optional(),
    resolution: z.enum(["FromQualitySettings", "Low", "Medium", "High", "VeryHigh"]).optional(),
  }, async (params) => {
    const result = await bridge.request("light.setShadows", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_ambient", "Set ambient light", {
    mode: z.enum(["Skybox", "Trilight", "Flat", "Custom"]).optional(),
    color: color.optional(), intensity: z.number().optional(),
    skyColor: color.optional(), equatorColor: color.optional(), groundColor: color.optional(),
  }, async (params) => {
    const result = await bridge.request("light.setAmbient", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_get_ambient", "Get ambient light", {}, async () => {
    const result = await bridge.request("light.getAmbient", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_light_set_color_temperature", "Set color temperature", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    temperature: z.number().optional(),
    useColorTemperature: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("light.setColorTemperature", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_set_cookie", "Set Light cookie", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    texturePath: z.string(),
    cookieSize: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("light.setCookie", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_set_culling_mask", "Set Light culling mask", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    cullingMask: z.number().optional(),
    layers: z.array(z.string()).optional(),
  }, async (p) => {
    const r = await bridge.request("light.setCullingMask", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_create_reflection_probe", "Create ReflectionProbe", {
    name: z.string().optional(),
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    size: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    resolution: z.number().optional(),
    mode: z.enum(["Baked", "Realtime", "Custom"]).optional(),
    intensity: z.number().optional(),
    boxProjection: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("light.createReflectionProbe", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_set_reflection_probe", "Set ReflectionProbe", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    size: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    resolution: z.number().optional(), mode: z.enum(["Baked", "Realtime", "Custom"]).optional(),
    intensity: z.number().optional(), boxProjection: z.boolean().optional(),
    nearClipPlane: z.number().optional(), farClipPlane: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("light.setReflectionProbe", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_find_reflection_probes", "Find ReflectionProbes", {}, async () => {
    const r = await bridge.request("light.findReflectionProbes", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_light_create_light_probe_group", "Create LightProbeGroup", {
    name: z.string().optional(),
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    probePositions: z.array(z.object({ x: z.number(), y: z.number(), z: z.number() })).optional(),
  }, async (p) => {
    const r = await bridge.request("light.createLightProbeGroup", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
