import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerClothTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_cloth_add", "Add a Cloth component to a GameObject with a SkinnedMeshRenderer.", {
    ...goRef,
    stretchingStiffness: z.number().optional().describe("Stretching stiffness (0-1)"),
    bendingStiffness: z.number().optional().describe("Bending stiffness (0-1)"),
    damping: z.number().optional(),
    friction: z.number().optional(),
    worldVelocityScale: z.number().optional(),
    worldAccelerationScale: z.number().optional(),
    useGravity: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("cloth.add", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_cloth_set", "Set Cloth properties.", {
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
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_cloth_getInfo", "Get Cloth component information.", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("cloth.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_cloth_find", "Find all Cloth components in the scene.", {}, async (p) => {
    const r = await bridge.request("cloth.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_cloth_setColliders", "Set capsule and sphere colliders for Cloth interaction.", {
    ...goRef,
    capsuleColliders: z.array(z.string()).optional().describe("Array of CapsuleCollider GameObject names"),
    sphereColliders: z.array(z.string()).optional().describe("Array of SphereCollider GameObject names"),
  }, async (p) => {
    const r = await bridge.request("cloth.setColliders", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
