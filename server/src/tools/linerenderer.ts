import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerLineRendererTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_lineRenderer_add", "Add LineRenderer", {
    ...goRef,
    positions: z.array(vec3).optional(),
    startWidth: z.number().optional(),
    endWidth: z.number().optional(),
    startColor: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    endColor: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    materialPath: z.string().optional(),
    useWorldSpace: z.boolean().optional(),
    loop: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("lineRenderer.add", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lineRenderer_setPositions", "Set LineRenderer positions", {
    ...goRef,
    positions: z.array(vec3),
  }, async (p) => {
    const r = await bridge.request("lineRenderer.setPositions", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lineRenderer_setProperties", "Set LineRenderer", {
    ...goRef,
    startWidth: z.number().optional(),
    endWidth: z.number().optional(),
    startColor: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    endColor: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    materialPath: z.string().optional(),
    useWorldSpace: z.boolean().optional(),
    loop: z.boolean().optional(),
    numCornerVertices: z.number().optional(),
    numCapVertices: z.number().optional(),
    textureMode: z.enum(["Stretch", "Tile", "DistributePerSegment", "RepeatPerSegment"]).optional(),
    shadowCastingMode: z.enum(["Off", "On", "TwoSided", "ShadowsOnly"]).optional(),
  }, async (p) => {
    const r = await bridge.request("lineRenderer.setProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lineRenderer_getInfo", "Get LineRenderer info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("lineRenderer.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_trailRenderer_add", "Add TrailRenderer", {
    ...goRef,
    time: z.number().optional(),
    startWidth: z.number().optional(),
    endWidth: z.number().optional(),
    startColor: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    endColor: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    materialPath: z.string().optional(),
    minVertexDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("lineRenderer.addTrail", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_trailRenderer_setProperties", "Set TrailRenderer", {
    ...goRef,
    time: z.number().optional(),
    startWidth: z.number().optional(),
    endWidth: z.number().optional(),
    startColor: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    endColor: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    minVertexDistance: z.number().optional(),
    autodestruct: z.boolean().optional(),
    emitting: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("lineRenderer.setTrailProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_lineRenderer_find", "Find LineRenderers", {
    type: z.enum(["Line", "Trail", "All"]).optional(),
  }, async (p) => {
    const r = await bridge.request("lineRenderer.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
