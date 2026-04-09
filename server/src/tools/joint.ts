import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerJointTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_joint_addHinge", "Add HingeJoint", {
    ...goRef,
    connectedBody: z.string().optional(),
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

  server.tool("unity_joint_addSpring", "Add SpringJoint", {
    ...goRef,
    connectedBody: z.string().optional(),
    spring: z.number().optional(),
    damper: z.number().optional(),
    minDistance: z.number().optional(),
    maxDistance: z.number().optional(),
    tolerance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("joint.addSpring", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_addFixed", "Add FixedJoint", {
    ...goRef,
    connectedBody: z.string().optional(),
    breakForce: z.number().optional(),
    breakTorque: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("joint.addFixed", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_addCharacter", "Add CharacterJoint", {
    ...goRef,
    connectedBody: z.string().optional(),
    anchor: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    axis: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    swingAxis: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
  }, async (p) => {
    const r = await bridge.request("joint.addCharacter", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_addConfigurable", "Add ConfigurableJoint", {
    ...goRef,
    connectedBody: z.string().optional(),
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

  server.tool("unity_joint_getInfo", "Get joint info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("joint.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_setProperties", "Set joint", {
    ...goRef,
    jointType: z.string(),
    breakForce: z.number().optional(),
    breakTorque: z.number().optional(),
    enableCollision: z.boolean().optional(),
    enablePreprocessing: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("joint.setProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_remove", "Remove joint", {
    ...goRef,
    jointType: z.string().optional().describe("Specific joint type to remove (default: first found)"),
  }, async (p) => {
    const r = await bridge.request("joint.remove", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_joint_find", "Find joints", {
    jointType: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("joint.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
