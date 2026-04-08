import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerSkeletal2DTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_skeletal2d_addBone", "Add a 2D bone to a GameObject hierarchy.", {
    ...goRef,
    parent: z.string().optional().describe("Parent bone name"),
    position: z.object({ x: z.number(), y: z.number() }),
    rotation: z.number().optional().describe("Rotation in degrees"),
    length: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("skeletal2d.addBone", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_skeletal2d_addIK", "Add an IK Solver 2D component.", {
    ...goRef,
    solverType: z.enum(["Limb", "CCD", "FABRIK"]).describe("IK solver type"),
    targetName: z.string().optional().describe("IK target GameObject"),
    chainLength: z.number().optional(),
    iterations: z.number().optional(),
    tolerance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("skeletal2d.addIK", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_skeletal2d_addSpriteSkin", "Add a SpriteSkin component for skeletal mesh deformation.", {
    ...goRef,
    autoRebind: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("skeletal2d.addSpriteSkin", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_skeletal2d_getInfo", "Get 2D skeletal animation info from a GameObject.", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("skeletal2d.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_skeletal2d_find", "Find all 2D skeletal components in the scene.", {
    type: z.enum(["Bone", "IK", "SpriteSkin", "All"]).optional(),
  }, async (p) => {
    const r = await bridge.request("skeletal2d.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
