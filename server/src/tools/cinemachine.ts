import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerCinemachineTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_cinemachine_createVirtualCamera",
    "Create VirtualCamera",
    {
      name: z.string().optional(),
      priority: z.number().optional(),
      fov: z.number().optional(),
      position: vec3.optional(),
      follow: z.string().optional(),
      lookAt: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.createVirtualCamera", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createFreeLook",
    "Create FreeLook Camera",
    {
      name: z.string().optional(),
      follow: z.string().optional(),
      lookAt: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.createFreeLook", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setBrain",
    "Set Brain settings",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      defaultBlend: z.number().optional(),
      updateMethod: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setBrain", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_getBrain",
    "Get Brain settings",
    {},
    async (params) => {
      const result = await bridge.request("cinemachine.getBrain", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setFollow",
    "Set follow target",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      target: z.string(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setFollow", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setLookAt",
    "Set lookAt target",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      target: z.string(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setLookAt", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setBody",
    "Set body settings",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      bodyType: z.string(),
      followOffset: vec3.optional(),
      damping: z.number().optional(),
      cameraDistance: z.number().optional(),
      screenX: z.number().optional(),
      screenY: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setBody", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setAim",
    "Set aim settings",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      aimType: z.string(),
      trackedObjectOffset: vec3.optional(),
      lookaheadTime: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setAim", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setNoise",
    "Set noise profile",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      amplitudeGain: z.number().optional(),
      frequencyGain: z.number().optional(),
      profileName: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setNoise", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setPriority",
    "Set camera priority",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      priority: z.number(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setPriority", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_getInfo",
    "Get VirtualCamera info",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_findCameras",
    "Find Cinemachine cameras",
    {},
    async (params) => {
      const result = await bridge.request("cinemachine.findCameras", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createBlendList",
    "Create BlendList Camera",
    { name: z.string().optional() },
    async (params) => {
      const result = await bridge.request("cinemachine.createBlendList", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setLens",
    "Set lens settings",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      fieldOfView: z.number().optional(),
      nearClipPlane: z.number().optional(),
      farClipPlane: z.number().optional(),
      orthographicSize: z.number().optional(),
      dutch: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setLens", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createDollyTrack",
    "Create DollyTrack",
    {
      name: z.string().optional(),
      points: z.array(z.array(z.number())).optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.createDollyTrack", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_addDollyPoint",
    "Add dolly point",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      position: vec3,
    },
    async (params) => {
      const result = await bridge.request("cinemachine.addDollyPoint", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setConfiner",
    "Set confiner",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      boundsObject: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setConfiner", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createMixingCamera",
    "Create MixingCamera",
    { name: z.string().optional() },
    async (params) => {
      const result = await bridge.request("cinemachine.createMixingCamera", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createClearShot",
    "Create ClearShot Camera",
    { name: z.string().optional() },
    async (params) => {
      const result = await bridge.request("cinemachine.createClearShot", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setDeadZone",
    "Set dead zone",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      width: z.number().optional(),
      height: z.number().optional(),
      softZoneWidth: z.number().optional(),
      softZoneHeight: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setDeadZone", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createGroup",
    "Create TargetGroup",
    { name: z.string().optional() },
    async (params) => {
      const result = await bridge.request("cinemachine.createGroup", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_addGroupTarget",
    "Add group target",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      target: z.string(),
      weight: z.number().optional(),
      radius: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.addGroupTarget", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setGroupFraming",
    "Set group framing",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      groupFramingSize: z.number().optional(),
      damping: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setGroupFraming", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
