import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerNavMeshTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_navmesh_bake", "Bake NavMesh", {
    agentRadius: z.number().optional(), agentHeight: z.number().optional(),
    agentSlope: z.number().optional(), agentClimb: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.bake", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_clear", "Clear NavMesh", {}, async () => {
    const r = await bridge.request("navmesh.clear", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_add_agent", "Add NavMeshAgent", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    speed: z.number().optional(), angularSpeed: z.number().optional(),
    acceleration: z.number().optional(), stoppingDistance: z.number().optional(),
    radius: z.number().optional(), height: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.addAgent", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_set_agent", "Set NavMeshAgent", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    speed: z.number().optional(), angularSpeed: z.number().optional(),
    acceleration: z.number().optional(), stoppingDistance: z.number().optional(),
    enabled: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.setAgent", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_add_obstacle", "Add NavMeshObstacle", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    carving: z.boolean().optional(), shape: z.enum(["box", "capsule"]).optional(),
    size: vec3.optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.addObstacle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_set_obstacle", "Set NavMeshObstacle", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    carving: z.boolean().optional(), enabled: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.setObstacle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_find_path", "Find NavMesh path", {
    from: vec3, to: vec3,
  }, async (p) => {
    const r = await bridge.request("navmesh.findPath", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_set_destination", "Set agent destination", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    destination: vec3,
  }, async (p) => {
    const r = await bridge.request("navmesh.setDestination", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_get_info", "Get NavMesh info", {}, async () => {
    const r = await bridge.request("navmesh.getInfo", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_navmesh_add_link", "Add NavMeshLink", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    startPoint: vec3.optional(), endPoint: vec3.optional(),
    width: z.number().optional(), bidirectional: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("navmesh.addLink", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
