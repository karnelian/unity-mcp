import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerSpriteShapeTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_spriteShape_create", "Create SpriteShape", {
    ...goRef,
    profilePath: z.string().optional(),
    fillPixelsPerUnit: z.number().optional(),
    stretchTiling: z.number().optional(),
    splineDetail: z.number().optional(),
    adaptiveUV: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("spriteShape.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spriteShape_addPoint", "Add SpriteShape point", {
    ...goRef,
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }),
    index: z.number().optional(),
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

  server.tool("unity_spriteShape_setPoint", "Set SpriteShape point", {
    ...goRef,
    index: z.number(),
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

  server.tool("unity_spriteShape_getInfo", "Get SpriteShape info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("spriteShape.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_spriteShape_find", "Find SpriteShapes", {}, async (p) => {
    const r = await bridge.request("spriteShape.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
