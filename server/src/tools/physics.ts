import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerPhysicsTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_physics_raycast", "물리 레이캐스트를 수행합니다.", {
    origin: vec3.describe("시작점"), direction: vec3.describe("방향"),
    maxDistance: z.number().optional(), layerMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.raycast", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_overlap_sphere", "구 영역 내 오브젝트를 검색합니다.", {
    center: vec3.describe("중심점"), radius: z.number().describe("반지름"),
    layerMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.overlapSphere", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_overlap_box", "박스 영역 내 오브젝트를 검색합니다.", {
    center: vec3.describe("중심점"), halfExtents: vec3.describe("반크기"),
    layerMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.overlapBox", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_set_gravity", "월드 중력을 설정합니다.", {
    gravity: vec3.describe("중력 벡터 (기본: 0,-9.81,0)"),
  }, async (params) => {
    const result = await bridge.request("physics.setGravity", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_get_gravity", "현재 월드 중력을 조회합니다.", {}, async () => {
    const result = await bridge.request("physics.getGravity", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_add_rigidbody", "GameObject에 Rigidbody를 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    mass: z.number().optional(), drag: z.number().optional(), angularDrag: z.number().optional(),
    useGravity: z.boolean().optional(), isKinematic: z.boolean().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.addRigidbody", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_set_rigidbody", "Rigidbody 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    mass: z.number().optional(), drag: z.number().optional(), angularDrag: z.number().optional(),
    useGravity: z.boolean().optional(), isKinematic: z.boolean().optional(),
    constraints: z.number().optional().describe("RigidbodyConstraints 비트플래그"),
  }, async (params) => {
    const result = await bridge.request("physics.setRigidbody", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_add_collider", "GameObject에 Collider를 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    colliderType: z.enum(["box", "sphere", "capsule", "mesh"]).describe("콜라이더 타입"),
    isTrigger: z.boolean().optional(), convex: z.boolean().optional().describe("MeshCollider용"),
  }, async (params) => {
    const result = await bridge.request("physics.addCollider", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_set_collider", "Collider 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    colliderType: z.string().optional(), isTrigger: z.boolean().optional(), enabled: z.boolean().optional(),
    center: vec3.optional(), size: vec3.optional(), radius: z.number().optional(), height: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.setCollider", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_create_material", "PhysicsMaterial을 생성합니다.", {
    name: z.string().describe("머티리얼 이름"), savePath: z.string().optional(),
    dynamicFriction: z.number().optional(), staticFriction: z.number().optional(),
    bounciness: z.number().optional(),
    frictionCombine: z.enum(["Average", "Minimum", "Maximum", "Multiply"]).optional(),
    bounceCombine: z.enum(["Average", "Minimum", "Maximum", "Multiply"]).optional(),
  }, async (params) => {
    const result = await bridge.request("physics.createPhysicsMaterial", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_set_layer_collision", "레이어 간 충돌 여부를 설정합니다.", {
    layer1: z.number().describe("레이어 1"), layer2: z.number().describe("레이어 2"),
    ignore: z.boolean().optional().describe("충돌 무시 (기본: true)"),
  }, async (params) => {
    const result = await bridge.request("physics.setLayerCollision", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_get_collision_matrix", "레이어 충돌 매트릭스를 조회합니다.", {}, async () => {
    const result = await bridge.request("physics.getLayerCollisionMatrix", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_sphere_cast", "구체 캐스트를 수행합니다.", {
    origin: vec3.describe("시작점"), direction: vec3.describe("방향"),
    radius: z.number().describe("구체 반지름"),
    maxDistance: z.number().optional(), layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.sphereCast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics_box_cast", "박스 캐스트를 수행합니다.", {
    center: vec3.describe("시작점"), halfExtents: vec3.describe("반크기"),
    direction: vec3.describe("방향"),
    maxDistance: z.number().optional(), layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.boxCast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics_capsule_cast", "캡슐 캐스트를 수행합니다.", {
    point1: vec3.describe("캡슐 상단"), point2: vec3.describe("캡슐 하단"),
    radius: z.number().describe("캡슐 반지름"), direction: vec3.describe("방향"),
    maxDistance: z.number().optional(), layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.capsuleCast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics_linecast", "두 점 사이의 라인캐스트를 수행합니다.", {
    start: vec3.describe("시작점"), end: vec3.describe("끝점"),
    layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.linecast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics_closest_point", "콜라이더 표면의 가장 가까운 점을 구합니다.", {
    point: vec3.describe("기준점"),
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.closestPoint", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
