import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerJointTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_joint_addHinge", "Add a HingeJoint to a GameObject.", {
    ...goRef,
    connectedBody: z.string().optional().describe("Connected Rigidbody name"),
    anchor: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    axis: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    useLimits: z.boolean().optional(),
    limits: z.object({ min: z.number(), max: z.number() }).optional(),
    useMotor: z.boolean().optional(),
    motor: z.object({ targetVelocity: z.number(), force: z.number() }).optional(),
    useSpring: z.boolean().optional(),
    spring: z.object({ spring: z.number(), damper: z.number(), targetPosition: z.number() }).optional(),
  }, async (p) => {
    const r = await bridge.request("joint.addHinge", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_addSpring", "Add a SpringJoint to a GameObject.", {
    ...goRef,
    connectedBody: z.string().optional().describe("Connected Rigidbody name"),
    spring: z.number().optional(),
    damper: z.number().optional(),
    minDistance: z.number().optional(),
    maxDistance: z.number().optional(),
    tolerance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("joint.addSpring", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_addFixed", "Add a FixedJoint to a GameObject.", {
    ...goRef,
    connectedBody: z.string().optional().describe("Connected Rigidbody name"),
    breakForce: z.number().optional(),
    breakTorque: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("joint.addFixed", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_addCharacter", "Add a CharacterJoint for ragdoll setups.", {
    ...goRef,
    connectedBody: z.string().optional().describe("Connected Rigidbody name"),
    anchor: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    axis: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    swingAxis: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
  }, async (p) => {
    const r = await bridge.request("joint.addCharacter", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_addConfigurable", "Add a ConfigurableJoint with full control.", {
    ...goRef,
    connectedBody: z.string().optional().describe("Connected Rigidbody name"),
    xMotion: z.enum(["Free", "Limited", "Locked"]).optional(),
    yMotion: z.enum(["Free", "Limited", "Locked"]).optional(),
    zMotion: z.enum(["Free", "Limited", "Locked"]).optional(),
    angularXMotion: z.enum(["Free", "Limited", "Locked"]).optional(),
    angularYMotion: z.enum(["Free", "Limited", "Locked"]).optional(),
    angularZMotion: z.enum(["Free", "Limited", "Locked"]).optional(),
  }, async (p) => {
    const r = await bridge.request("joint.addConfigurable", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_getInfo", "Get joint information on a GameObject.", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("joint.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_setProperties", "Set joint properties.", {
    ...goRef,
    jointType: z.string().describe("Joint component type name"),
    breakForce: z.number().optional(),
    breakTorque: z.number().optional(),
    enableCollision: z.boolean().optional(),
    enablePreprocessing: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("joint.setProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_remove", "Remove a joint from a GameObject.", {
    ...goRef,
    jointType: z.string().optional().describe("Specific joint type to remove (default: first found)"),
  }, async (p) => {
    const r = await bridge.request("joint.remove", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_find", "Find all GameObjects with joints in the scene.", {
    jointType: z.string().optional().describe("Filter by joint type"),
  }, async (p) => {
    const r = await bridge.request("joint.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
