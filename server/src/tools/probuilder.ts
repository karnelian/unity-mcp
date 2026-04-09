import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerProBuilderTools(server: McpServer, bridge: UnityBridge) {

  const goId = {
    path: z.string().optional(),
    name: z.string().optional(),
    instanceId: z.number().optional(),
  };

  server.tool("unity_probuilder_createCube", "Create ProBuilder cube", {
    name: z.string().optional(), size: vec3.optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createCube", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createCylinder", "Create ProBuilder cylinder", {
    name: z.string().optional(), radius: z.number().optional(), height: z.number().optional(),
    axisDivisions: z.number().optional(), heightCuts: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createCylinder", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createSphere", "Create ProBuilder sphere", {
    name: z.string().optional(), radius: z.number().optional(), subdivisions: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createSphere", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createPlane", "Create ProBuilder plane", {
    name: z.string().optional(), width: z.number().optional(), height: z.number().optional(),
    widthCuts: z.number().optional(), heightCuts: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createPlane", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createStair", "Create ProBuilder stair", {
    name: z.string().optional(), width: z.number().optional(), height: z.number().optional(),
    depth: z.number().optional(), steps: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createStair", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createArch", "Create ProBuilder arch", {
    name: z.string().optional(), angle: z.number().optional(), radius: z.number().optional(),
    width: z.number().optional(), depth: z.number().optional(), radialCuts: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createArch", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createDoor", "Create ProBuilder door", {
    name: z.string().optional(), totalWidth: z.number().optional(), totalHeight: z.number().optional(),
    depth: z.number().optional(), doorWidth: z.number().optional(), doorHeight: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createDoor", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_createPipe", "Create ProBuilder pipe", {
    name: z.string().optional(), radius: z.number().optional(), height: z.number().optional(),
    thickness: z.number().optional(), subdivisions: z.number().optional(), position: vec3.optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.createPipe", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_getInfo", "Get ProBuilder info", goId, async (params) => {
    const result = await bridge.request("probuilder.getInfo", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_extrude", "Extrude faces", {
    ...goId, distance: z.number().optional(),
    faceIndices: z.array(z.number()).optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.extrude", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_bevel", "Bevel edges", {
    ...goId, amount: z.number().optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.bevel", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_subdivide", "Subdivide mesh", goId, async (params) => {
    const result = await bridge.request("probuilder.subdivide", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_merge", "Merge meshes", {
    names: z.array(z.string()).optional(), paths: z.array(z.string()).optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.merge", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_flip", "Flip normals", goId, async (params) => {
    const result = await bridge.request("probuilder.flip", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_setMaterial", "Set face material", {
    ...goId, materialPath: z.string(),
    faceIndices: z.array(z.number()).optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.setMaterial", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_findProBuilderObjects", "Find ProBuilder objects", {}, async (params) => {
    const result = await bridge.request("probuilder.findProBuilderObjects", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_centerPivot", "Center pivot", goId, async (params) => {
    const result = await bridge.request("probuilder.centerPivot", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_freezeTransform", "Freeze transform", goId, async (params) => {
    const result = await bridge.request("probuilder.freezeTransform", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_triangulate", "Triangulate faces", goId, async (params) => {
    const result = await bridge.request("probuilder.triangulate", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_export", "Export mesh", {
    ...goId, format: z.string().optional(),
    savePath: z.string().optional(),
  }, async (params) => {
    const result = await bridge.request("probuilder.export", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_selectFaces", "Select faces", {
    ...goId, faceIndices: z.array(z.number()),
  }, async (params) => {
    const result = await bridge.request("probuilder.selectFaces", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });

  server.tool("unity_probuilder_deleteFaces", "Delete faces", {
    ...goId, faceIndices: z.array(z.number()),
  }, async (params) => {
    const result = await bridge.request("probuilder.deleteFaces", params);
    return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
  });
}
