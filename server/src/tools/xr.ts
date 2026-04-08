import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerXRTools(server: McpServer, bridge: UnityBridge) {

  const goId = {
    path: z.string().optional().describe("오브젝트 경로"),
    name: z.string().optional().describe("오브젝트 이름"),
    instanceId: z.number().optional().describe("인스턴스 ID"),
  };

  // XR Management tools
  server.tool("unity_xr_getSettings", "XR 플러그인 설정을 조회합니다. (com.unity.xr.management 필요)", {}, async (params) => {
    const result = await bridge.request("xr.getSettings", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_getLoaders", "활성 XR 로더 목록을 조회합니다.", {}, async (params) => {
    const result = await bridge.request("xr.getLoaders", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setLoader", "XR 로더를 설정합니다.", {
    loader: z.string().optional().describe("로더 이름"),
  }, async (params) => {
    const result = await bridge.request("xr.setLoader", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_getDeviceInfo", "연결된 XR 디바이스 정보를 조회합니다.", {}, async (params) => {
    const result = await bridge.request("xr.getDeviceInfo", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  // XR Interaction Toolkit tools
  server.tool("unity_xr_createXRRig", "XR Rig(카메라+컨트롤러)를 생성합니다. (com.unity.xr.interaction.toolkit 필요)", {
    name: z.string().optional().describe("리그 이름"),
  }, async (params) => {
    const result = await bridge.request("xr.createXRRig", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createGrabInteractable", "오브젝트에 XR Grab Interactable을 추가합니다.", {
    ...goId,
    throwOnDetach: z.boolean().optional().describe("놓을 때 던지기 (기본: true)"),
    movementType: z.string().optional().describe("이동 타입 (VelocityTracking, Kinematic, Instantaneous)"),
  }, async (params) => {
    const result = await bridge.request("xr.createGrabInteractable", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createRayInteractor", "오브젝트에 Ray Interactor를 추가합니다.", {
    ...goId,
    maxRaycastDistance: z.number().optional().describe("최대 레이캐스트 거리"),
  }, async (params) => {
    const result = await bridge.request("xr.createRayInteractor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createDirectInteractor", "오브젝트에 Direct Interactor를 추가합니다.", goId, async (params) => {
    const result = await bridge.request("xr.createDirectInteractor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createSocketInteractor", "Socket Interactor를 생성합니다.", {
    name: z.string().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createSocketInteractor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createTeleportArea", "오브젝트를 텔레포트 영역으로 설정합니다.", goId, async (params) => {
    const result = await bridge.request("xr.createTeleportArea", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createTeleportAnchor", "텔레포트 앵커를 생성합니다.", {
    name: z.string().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createTeleportAnchor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setInteractableSettings", "Interactable 설정을 변경합니다.", {
    ...goId, interactionLayerMask: z.number().optional().describe("인터랙션 레이어 마스크"),
  }, async (params) => {
    const result = await bridge.request("xr.setInteractableSettings", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_findInteractables", "씬의 모든 XR Interactable을 찾습니다.", {}, async (params) => {
    const result = await bridge.request("xr.findInteractables", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_findInteractors", "씬의 모든 XR Interactor를 찾습니다.", {}, async (params) => {
    const result = await bridge.request("xr.findInteractors", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createSnapZone", "Snap Zone(Socket)을 생성합니다.", {
    name: z.string().optional(), position: vec3.optional(), radius: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createSnapZone", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setHandTracking", "핸드 트래킹 설정 정보를 조회합니다.", {}, async (params) => {
    const result = await bridge.request("xr.setHandTracking", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createLocomotionSystem", "Locomotion System을 생성합니다.", {
    name: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createLocomotionSystem", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createClimbInteractable", "오브젝트에 클라이밍 기능을 추가합니다.", goId, async (params) => {
    const result = await bridge.request("xr.createClimbInteractable", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setLayerMask", "Interactor의 레이어 마스크를 설정합니다.", {
    ...goId, interactionLayers: z.number().optional(), raycastMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("xr.setLayerMask", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_createUICanvas", "World Space XR UI Canvas를 생성합니다.", {
    name: z.string().optional(), width: z.number().optional(), height: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("xr.createUICanvas", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_getXRRigInfo", "XR Rig 구성 정보를 조회합니다.", {}, async (params) => {
    const result = await bridge.request("xr.getXRRigInfo", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_xr_setTrackingOrigin", "트래킹 오리진 설정 정보를 조회합니다.", {}, async (params) => {
    const result = await bridge.request("xr.setTrackingOrigin", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });
}
