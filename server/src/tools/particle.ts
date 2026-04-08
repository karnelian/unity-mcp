import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });

export function registerParticleTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_particle_create", "ParticleSystem을 생성합니다.", {
    name: z.string().optional().describe("오브젝트 이름 (기본: ParticleSystem)"),
    parent: z.string().optional().describe("부모 경로"),
    position: vec3.optional().describe("위치"),
    startLifetime: z.number().optional().describe("시작 수명"),
    startSpeed: z.number().optional().describe("시작 속도"),
    startSize: z.number().optional().describe("시작 크기"),
    startColor: color.optional().describe("시작 색상"),
    maxParticles: z.number().optional().describe("최대 파티클 수"),
    duration: z.number().optional().describe("시스템 지속 시간"),
    loop: z.boolean().optional().describe("루프 여부"),
  }, async (p) => {
    const r = await bridge.request("particle.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_getInfo", "ParticleSystem 정보를 조회합니다.", {
    name: z.string().optional().describe("오브젝트 이름"),
    path: z.string().optional().describe("오브젝트 경로"),
    instanceId: z.number().optional().describe("인스턴스 ID"),
  }, async (p) => {
    const r = await bridge.request("particle.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setMain", "ParticleSystem Main 모듈을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    duration: z.number().optional().describe("지속 시간"),
    loop: z.boolean().optional().describe("루프"),
    startLifetime: z.number().optional().describe("시작 수명"),
    startSpeed: z.number().optional().describe("시작 속도"),
    startSize: z.number().optional().describe("시작 크기"),
    startColor: color.optional().describe("시작 색상"),
    gravityModifier: z.number().optional().describe("중력 계수"),
    maxParticles: z.number().optional().describe("최대 파티클 수"),
    playOnAwake: z.boolean().optional().describe("자동 재생"),
    simulationSpace: z.enum(["Local", "World", "Custom"]).optional().describe("시뮬레이션 공간"),
    scalingMode: z.enum(["Hierarchy", "Local", "Shape"]).optional().describe("스케일링 모드"),
  }, async (p) => {
    const r = await bridge.request("particle.setMain", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setEmission", "Emission 모듈을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    rateOverTime: z.number().optional().describe("시간당 방출 수"),
    rateOverDistance: z.number().optional().describe("거리당 방출 수"),
    bursts: z.array(z.object({
      time: z.number().describe("버스트 시간"),
      count: z.number().describe("파티클 수"),
      cycles: z.number().optional().describe("반복 횟수"),
      interval: z.number().optional().describe("반복 간격"),
    })).optional().describe("버스트 설정"),
  }, async (p) => {
    const r = await bridge.request("particle.setEmission", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setShape", "Shape 모듈을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    shapeType: z.enum(["Sphere", "Hemisphere", "Cone", "Box", "Circle", "Rectangle", "Donut", "Mesh", "MeshRenderer", "SkinnedMeshRenderer"]).optional(),
    radius: z.number().optional(), angle: z.number().optional(), arc: z.number().optional(),
    position: vec3.optional(), rotation: vec3.optional(), scale: vec3.optional(),
    radiusThickness: z.number().optional().describe("방출 두께 (0=표면, 1=볼륨)"),
    length: z.number().optional().describe("콘 길이"),
  }, async (p) => {
    const r = await bridge.request("particle.setShape", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setRenderer", "파티클 렌더러를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    renderMode: z.enum(["Billboard", "Stretch", "HorizontalBillboard", "VerticalBillboard", "Mesh"]).optional(),
    materialPath: z.string().optional().describe("머티리얼 에셋 경로"),
    sortingOrder: z.number().optional(),
    sortingLayerName: z.string().optional(),
    minParticleSize: z.number().optional(),
    maxParticleSize: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setRenderer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setColorOverLifetime", "수명별 색상을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    colorKeys: z.array(z.object({
      time: z.number().describe("0~1 시간"),
      r: z.number(), g: z.number(), b: z.number(),
    })).optional().describe("색상 키"),
    alphaKeys: z.array(z.object({
      time: z.number().describe("0~1 시간"),
      alpha: z.number().describe("0~1 투명도"),
    })).optional().describe("알파 키"),
  }, async (p) => {
    const r = await bridge.request("particle.setColorOverLifetime", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setSizeOverLifetime", "수명별 크기를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    sizeMultiplier: z.number().optional().describe("크기 배율"),
    curve: z.array(z.object({
      time: z.number().describe("0~1 시간"), value: z.number().describe("크기 값"),
    })).optional().describe("크기 커브 키프레임"),
  }, async (p) => {
    const r = await bridge.request("particle.setSizeOverLifetime", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setRotationOverLifetime", "수명별 회전을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    angularVelocity: z.number().optional().describe("각속도 (도/초)"),
  }, async (p) => {
    const r = await bridge.request("particle.setRotationOverLifetime", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setNoise", "노이즈 모듈을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    strength: z.number().optional().describe("강도"),
    frequency: z.number().optional().describe("주파수"),
    scrollSpeed: z.number().optional().describe("스크롤 속도"),
    octaveCount: z.number().optional().describe("옥타브 수"),
    damping: z.boolean().optional().describe("댐핑"),
  }, async (p) => {
    const r = await bridge.request("particle.setNoise", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setCollision", "충돌 모듈을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    type: z.enum(["Planes", "World"]).optional(),
    mode: z.enum(["Collision2D", "Collision3D"]).optional(),
    bounce: z.number().optional(), dampen: z.number().optional(),
    lifetimeLoss: z.number().optional(),
    enableDynamicColliders: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setCollision", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setTrails", "트레일 모듈을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    ratio: z.number().optional().describe("트레일 비율 0~1"),
    lifetime: z.number().optional().describe("트레일 수명"),
    minVertexDistance: z.number().optional(),
    worldSpace: z.boolean().optional(),
    dieWithParticles: z.boolean().optional(),
    widthOverTrail: z.number().optional(),
    textureMode: z.enum(["Stretch", "Tile", "DistributePerSegment", "RepeatPerSegment"]).optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setTrails", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setVelocityOverLifetime", "수명별 속도를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    x: z.number().optional(), y: z.number().optional(), z: z.number().optional(),
    space: z.enum(["Local", "World"]).optional(),
    speedModifier: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setVelocityOverLifetime", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_play", "ParticleSystem을 제어합니다 (play/stop/pause/clear/restart).", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    action: z.enum(["play", "stop", "pause", "clear", "restart"]).describe("수행할 작업"),
  }, async (p) => {
    const r = await bridge.request("particle.play", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_find", "씬의 ParticleSystem을 검색합니다.", {
    nameFilter: z.string().optional().describe("이름 필터"),
  }, async (p) => {
    const r = await bridge.request("particle.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setSubEmitters", "서브 이미터 모듈을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setSubEmitters", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setTextureSheetAnimation", "텍스처 시트 애니메이션을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    numTilesX: z.number().optional().describe("가로 타일 수"),
    numTilesY: z.number().optional().describe("세로 타일 수"),
    cycleCount: z.number().optional().describe("사이클 수"),
  }, async (p) => {
    const r = await bridge.request("particle.setTextureSheetAnimation", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
