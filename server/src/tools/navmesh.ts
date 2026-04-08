import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerNavMeshTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_navmesh_bake", "NavMesh를 베이크합니다.", {
    agentRadius: z.number().optional(), agentHeight: z.number().optional(),
    agentSlope: z.number().optional(), agentClimb: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.bake", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_clear", "모든 NavMesh를 제거합니다.", {}, async () => {
    const r = await bridge.request("navmesh.clear", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_add_agent", "NavMeshAgent를 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    speed: z.number().optional(), angularSpeed: z.number().optional(),
    acceleration: z.number().optional(), stoppingDistance: z.number().optional(),
    radius: z.number().optional(), height: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.addAgent", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_set_agent", "NavMeshAgent 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    speed: z.number().optional(), angularSpeed: z.number().optional(),
    acceleration: z.number().optional(), stoppingDistance: z.number().optional(),
    enabled: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.setAgent", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_add_obstacle", "NavMeshObstacle을 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    carving: z.boolean().optional(), shape: z.enum(["box", "capsule"]).optional(),
    size: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.addObstacle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_set_obstacle", "NavMeshObstacle 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    carving: z.boolean().optional(), enabled: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.setObstacle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_find_path", "두 지점 간 NavMesh 경로를 계산합니다.", {
    from: vec3.describe("시작점"), to: vec3.describe("도착점"),
  }, async (p) => {
    const r = await bridge.request("navmesh.findPath", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_set_destination", "NavMeshAgent의 목적지를 설정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    destination: vec3.describe("목적지"),
  }, async (p) => {
    const r = await bridge.request("navmesh.setDestination", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_get_info", "NavMesh 상태 정보를 조회합니다.", {}, async () => {
    const r = await bridge.request("navmesh.getInfo", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_add_link", "NavMeshLink를 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    startPoint: vec3.optional(), endPoint: vec3.optional(),
    width: z.number().optional(), bidirectional: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.addLink", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
