import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerSkeletal2DTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_skeletal2d_addBone", "Add 2D bone", {
    ...goRef,
    parent: z.string().optional(),
    position: z.object({ x: z.number(), y: z.number() }),
    rotation: z.number().optional(),
    length: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("skeletal2d.addBone", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_skeletal2d_addIK", "Add 2D IK", {
    ...goRef,
    solverType: z.enum(["Limb", "CCD", "FABRIK"]),
    targetName: z.string().optional(),
    chainLength: z.number().optional(),
    iterations: z.number().optional(),
    tolerance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("skeletal2d.addIK", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_skeletal2d_addSpriteSkin", "Add SpriteSkin", {
    ...goRef,
    autoRebind: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("skeletal2d.addSpriteSkin", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_skeletal2d_getInfo", "Get skeletal 2D info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("skeletal2d.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_skeletal2d_find", "Find skeletal 2D objects", {
    type: z.enum(["Bone", "IK", "SpriteSkin", "All"]).optional(),
  }, async (p) => {
    const r = await bridge.request("skeletal2d.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
