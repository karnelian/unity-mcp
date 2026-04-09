import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerPhysicsTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_physics_raycast", "Raycast", {
    origin: vec3, direction: vec3,
    maxDistance: z.number().optional(), layerMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.raycast", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_overlap_sphere", "Overlap sphere", {
    center: vec3, radius: z.number(),
    layerMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.overlapSphere", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_overlap_box", "Overlap box", {
    center: vec3, halfExtents: vec3,
    layerMask: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.overlapBox", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_set_gravity", "Set gravity", {
    gravity: vec3.describe("중력 벡터 (기본: 0,-9.81,0)"),
  }, async (params) => {
    const result = await bridge.request("physics.setGravity", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_get_gravity", "Get gravity", {}, async () => {
    const result = await bridge.request("physics.getGravity", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_add_rigidbody", "Add Rigidbody", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    mass: z.number().optional(), drag: z.number().optional(), angularDrag: z.number().optional(),
    useGravity: z.boolean().optional(), isKinematic: z.boolean().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.addRigidbody", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_set_rigidbody", "Set Rigidbody", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    mass: z.number().optional(), drag: z.number().optional(), angularDrag: z.number().optional(),
    useGravity: z.boolean().optional(), isKinematic: z.boolean().optional(),
    constraints: z.number().optional().describe("RigidbodyConstraints 비트플래그"),
  }, async (params) => {
    const result = await bridge.request("physics.setRigidbody", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_add_collider", "Add Collider", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    colliderType: z.enum(["box", "sphere", "capsule", "mesh"]).describe("콜라이더 타입"),
    isTrigger: z.boolean().optional(), convex: z.boolean().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.addCollider", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_set_collider", "Set Collider", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    colliderType: z.string().optional(), isTrigger: z.boolean().optional(), enabled: z.boolean().optional(),
    center: vec3.optional(), size: vec3.optional(), radius: z.number().optional(), height: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("physics.setCollider", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_create_material", "Create PhysicMaterial", {
    name: z.string(), savePath: z.string().optional(),
    dynamicFriction: z.number().optional(), staticFriction: z.number().optional(),
    bounciness: z.number().optional(),
    frictionCombine: z.enum(["Average", "Minimum", "Maximum", "Multiply"]).optional(),
    bounceCombine: z.enum(["Average", "Minimum", "Maximum", "Multiply"]).optional(),
  }, async (params) => {
    const result = await bridge.request("physics.createPhysicsMaterial", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_set_layer_collision", "Set layer collision", {
    layer1: z.number(), layer2: z.number(),
    ignore: z.boolean().optional().describe("충돌 무시 (기본: true)"),
  }, async (params) => {
    const result = await bridge.request("physics.setLayerCollision", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_get_collision_matrix", "Get collision matrix", {}, async () => {
    const result = await bridge.request("physics.getLayerCollisionMatrix", {});
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_physics_sphere_cast", "SphereCast", {
    origin: vec3, direction: vec3,
    radius: z.number(),
    maxDistance: z.number().optional(), layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.sphereCast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics_box_cast", "BoxCast", {
    center: vec3, halfExtents: vec3,
    direction: vec3,
    maxDistance: z.number().optional(), layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.boxCast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics_capsule_cast", "CapsuleCast", {
    point1: vec3, point2: vec3,
    radius: z.number(), direction: vec3,
    maxDistance: z.number().optional(), layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.capsuleCast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics_linecast", "Linecast", {
    start: vec3, end: vec3,
    layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.linecast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics_closest_point", "Closest point", {
    point: vec3,
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics.closestPoint", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
