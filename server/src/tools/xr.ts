import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerXRTools(server: McpServer, bridge: UnityBridge) {

  const goId = {
    path: z.string().optional(),
    name: z.string().optional(),
    instanceId: z.number().optional(),
  };

  // XR Management tools
  server.tool("unity_xr_getSettings", "Get XR settings", {}, async (params) => {
    const result = await bridge.request("xr.getSettings", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_getLoaders", "Get XR Loaders", {}, async (params) => {
    const result = await bridge.request("xr.getLoaders", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setLoader", "Set XR Loader", {
    loader: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.setLoader", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_getDeviceInfo", "Get XR device info", {}, async (params) => {
    const result = await bridge.request("xr.getDeviceInfo", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  // XR Interaction Toolkit tools
  server.tool("unity_xr_createXRRig", "Create XR Rig", {
    name: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createXRRig", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createGrabInteractable", "Create GrabInteractable", {
    ...goId,
    throwOnDetach: z.boolean().optional(),
    movementType: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createGrabInteractable", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createRayInteractor", "Create RayInteractor", {
    ...goId,
    maxRaycastDistance: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createRayInteractor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createDirectInteractor", "Create DirectInteractor", goId, async (params) => {
    const result = await bridge.request("xr.createDirectInteractor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createSocketInteractor", "Create SocketInteractor", {
    name: z.string().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createSocketInteractor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createTeleportArea", "Create TeleportArea", goId, async (params) => {
    const result = await bridge.request("xr.createTeleportArea", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createTeleportAnchor", "Create TeleportAnchor", {
    name: z.string().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createTeleportAnchor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setInteractableSettings", "Set Interactable settings", {
    ...goId, interactionLayerMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.setInteractableSettings", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_findInteractables", "Find Interactables", {}, async (params) => {
    const result = await bridge.request("xr.findInteractables", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_findInteractors", "Find Interactors", {}, async (params) => {
    const result = await bridge.request("xr.findInteractors", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createSnapZone", "Create SnapZone", {
    name: z.string().optional(), position: vec3.optional(), radius: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createSnapZone", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setHandTracking", "Set hand tracking", {}, async (params) => {
    const result = await bridge.request("xr.setHandTracking", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createLocomotionSystem", "Create LocomotionSystem", {
    name: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createLocomotionSystem", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createClimbInteractable", "Create ClimbInteractable", goId, async (params) => {
    const result = await bridge.request("xr.createClimbInteractable", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setLayerMask", "Set interaction layer mask", {
    ...goId, interactionLayers: z.number().optional(), raycastMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.setLayerMask", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createUICanvas", "Create XR UI Canvas", {
    name: z.string().optional(), width: z.number().optional(), height: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createUICanvas", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_getXRRigInfo", "Get XR Rig info", {}, async (params) => {
    const result = await bridge.request("xr.getXRRigInfo", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setTrackingOrigin", "Set tracking origin", {}, async (params) => {
    const result = await bridge.request("xr.setTrackingOrigin", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });
}
