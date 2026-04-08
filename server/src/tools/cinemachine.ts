import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerCinemachineTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_cinemachine_createVirtualCamera",
    "Cinemachine Virtual Camera를 생성합니다. (com.unity.cinemachine 필요)",
    {
      name: z.string().optional().describe("카메라 이름"),
      priority: z.number().optional().describe("우선순위"),
      fov: z.number().optional().describe("FOV"),
      position: vec3.optional().describe("위치"),
      follow: z.string().optional().describe("Follow 대상 이름"),
      lookAt: z.string().optional().describe("LookAt 대상 이름"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.createVirtualCamera", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createFreeLook",
    "Cinemachine FreeLook 카메라를 생성합니다.",
    {
      name: z.string().optional().describe("카메라 이름"),
      follow: z.string().optional().describe("Follow 대상 이름"),
      lookAt: z.string().optional().describe("LookAt 대상 이름"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.createFreeLook", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setBrain",
    "CinemachineBrain 설정을 변경합니다.",
    {
      path: z.string().optional().describe("카메라 오브젝트 경로"),
      name: z.string().optional().describe("카메라 오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      defaultBlend: z.number().optional().describe("기본 블렌드 시간"),
      updateMethod: z.string().optional().describe("업데이트 메서드"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setBrain", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_getBrain",
    "CinemachineBrain 정보를 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("cinemachine.getBrain", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setFollow",
    "Virtual Camera의 Follow 대상을 설정합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      target: z.string().describe("Follow 대상 이름"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setFollow", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setLookAt",
    "Virtual Camera의 LookAt 대상을 설정합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      target: z.string().describe("LookAt 대상 이름"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setLookAt", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setBody",
    "Virtual Camera의 Body 컴포넌트를 설정합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      bodyType: z.string().describe("Body 타입 (transposer, framingTransposer, hardLockToTarget)"),
      followOffset: vec3.optional().describe("Follow 오프셋 (transposer)"),
      damping: z.number().optional().describe("댐핑 (transposer)"),
      cameraDistance: z.number().optional().describe("카메라 거리 (framingTransposer)"),
      screenX: z.number().optional().describe("화면 X (framingTransposer)"),
      screenY: z.number().optional().describe("화면 Y (framingTransposer)"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setBody", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setAim",
    "Virtual Camera의 Aim 컴포넌트를 설정합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      aimType: z.string().describe("Aim 타입 (composer, hardLookAt, pov)"),
      trackedObjectOffset: vec3.optional().describe("트래킹 오프셋 (composer)"),
      lookaheadTime: z.number().optional().describe("예측 시간 (composer)"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setAim", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setNoise",
    "Virtual Camera에 노이즈(카메라 셰이크)를 설정합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      amplitudeGain: z.number().optional().describe("진폭 게인"),
      frequencyGain: z.number().optional().describe("주파수 게인"),
      profileName: z.string().optional().describe("노이즈 프로파일 이름"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setNoise", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setPriority",
    "Virtual Camera의 우선순위를 설정합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      priority: z.number().describe("우선순위"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setPriority", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_getInfo",
    "Virtual Camera의 상세 정보를 조회합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_findCameras",
    "씬의 모든 Cinemachine 카메라를 찾습니다.",
    {},
    async (params) => {
      const result = await bridge.request("cinemachine.findCameras", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createBlendList",
    "BlendListCamera를 생성합니다.",
    { name: z.string().optional().describe("카메라 이름") },
    async (params) => {
      const result = await bridge.request("cinemachine.createBlendList", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setLens",
    "Virtual Camera의 렌즈 설정을 변경합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      fieldOfView: z.number().optional().describe("FOV"),
      nearClipPlane: z.number().optional().describe("Near Clip"),
      farClipPlane: z.number().optional().describe("Far Clip"),
      orthographicSize: z.number().optional().describe("정사영 크기"),
      dutch: z.number().optional().describe("더치 각도"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setLens", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createDollyTrack",
    "Dolly Track(SmoothPath)을 생성합니다.",
    {
      name: z.string().optional().describe("트랙 이름"),
      points: z.array(z.array(z.number())).optional().describe("경로 점 [[x,y,z], ...]"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.createDollyTrack", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_addDollyPoint",
    "Dolly Track에 포인트를 추가합니다.",
    {
      path: z.string().optional().describe("트랙 경로"),
      name: z.string().optional().describe("트랙 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      position: vec3.describe("포인트 위치"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.addDollyPoint", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setConfiner",
    "Virtual Camera에 Confiner를 설정합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      boundsObject: z.string().optional().describe("경계 Collider 오브젝트 이름"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setConfiner", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createMixingCamera",
    "MixingCamera를 생성합니다.",
    { name: z.string().optional().describe("카메라 이름") },
    async (params) => {
      const result = await bridge.request("cinemachine.createMixingCamera", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createClearShot",
    "ClearShot 카메라를 생성합니다.",
    { name: z.string().optional().describe("카메라 이름") },
    async (params) => {
      const result = await bridge.request("cinemachine.createClearShot", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setDeadZone",
    "Composer의 Dead Zone 설정을 변경합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      width: z.number().optional().describe("Dead Zone 너비"),
      height: z.number().optional().describe("Dead Zone 높이"),
      softZoneWidth: z.number().optional().describe("Soft Zone 너비"),
      softZoneHeight: z.number().optional().describe("Soft Zone 높이"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setDeadZone", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_createGroup",
    "Target Group을 생성합니다.",
    { name: z.string().optional().describe("그룹 이름") },
    async (params) => {
      const result = await bridge.request("cinemachine.createGroup", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_addGroupTarget",
    "Target Group에 대상을 추가합니다.",
    {
      path: z.string().optional().describe("그룹 경로"),
      name: z.string().optional().describe("그룹 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      target: z.string().describe("추가할 대상 이름"),
      weight: z.number().optional().describe("가중치 (기본: 1)"),
      radius: z.number().optional().describe("반경 (기본: 1)"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.addGroupTarget", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_cinemachine_setGroupFraming",
    "Group 카메라의 프레이밍 설정을 변경합니다.",
    {
      path: z.string().optional().describe("카메라 경로"),
      name: z.string().optional().describe("카메라 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      groupFramingSize: z.number().optional().describe("그룹 프레이밍 크기"),
      damping: z.number().optional().describe("댐핑"),
    },
    async (params) => {
      const result = await bridge.request("cinemachine.setGroupFraming", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
