import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerSpriteShapeTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_spriteShape_create", "Create a SpriteShapeController on a GameObject.", {
    ...goRef,
    profilePath: z.string().optional().describe("SpriteShapeProfile asset path"),
    fillPixelsPerUnit: z.number().optional(),
    stretchTiling: z.number().optional(),
    splineDetail: z.number().optional(),
    adaptiveUV: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("spriteShape.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spriteShape_addPoint", "Add a spline point to a SpriteShapeController.", {
    ...goRef,
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }),
    index: z.number().optional().describe("Insert at index (appends if omitted)"),
    tangentMode: z.enum(["Linear", "Continuous", "Broken"]).optional(),
    leftTangent: z.object({ x: z.number(), y: z.number() }).optional(),
    rightTangent: z.object({ x: z.number(), y: z.number() }).optional(),
    height: z.number().optional(),
    spriteIndex: z.number().optional(),
    corner: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("spriteShape.addPoint", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spriteShape_setPoint", "Modify a spline point on a SpriteShapeController.", {
    ...goRef,
    index: z.number().describe("Point index"),
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    tangentMode: z.enum(["Linear", "Continuous", "Broken"]).optional(),
    leftTangent: z.object({ x: z.number(), y: z.number() }).optional(),
    rightTangent: z.object({ x: z.number(), y: z.number() }).optional(),
    height: z.number().optional(),
    spriteIndex: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("spriteShape.setPoint", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spriteShape_getInfo", "Get SpriteShapeController information.", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("spriteShape.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spriteShape_find", "Find all SpriteShapeControllers in the scene.", {}, async (p) => {
    const r = await bridge.request("spriteShape.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
