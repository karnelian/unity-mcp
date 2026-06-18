import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerClothTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_cloth_add", "Add Cloth", {
    ...goRef,
    stretchingStiffness: z.number().optional(),
    bendingStiffness: z.number().optional(),
    damping: z.number().optional(),
    friction: z.number().optional(),
    worldVelocityScale: z.number().optional(),
    worldAccelerationScale: z.number().optional(),
    useGravity: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("cloth.add", p);
    return textResult(r);
  });

  server.tool("unity_cloth_set", "Set Cloth", {
    ...goRef,
    stretchingStiffness: z.number().optional(),
    bendingStiffness: z.number().optional(),
    damping: z.number().optional(),
    friction: z.number().optional(),
    worldVelocityScale: z.number().optional(),
    worldAccelerationScale: z.number().optional(),
    useGravity: z.boolean().optional(),
    externalAcceleration: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    randomAcceleration: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    clothSolverFrequency: z.number().optional(),
    sleepThreshold: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("cloth.set", p);
    return textResult(r);
  });

  server.tool("unity_cloth_getInfo", "Get Cloth info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("cloth.getInfo", p);
    return textResult(r);
  });

  server.tool("unity_cloth_find", "Find Cloth objects", {}, async (p) => {
    const r = await bridge.request("cloth.find", p);
    return textResult(r);
  });

  server.tool("unity_cloth_setColliders", "Set Cloth colliders", {
    ...goRef,
    capsuleColliders: z.array(z.string()).optional(),
    sphereColliders: z.array(z.string()).optional(),
  }, async (p) => {
    const r = await bridge.request("cloth.setColliders", p);
    return textResult(r);
  });
}
