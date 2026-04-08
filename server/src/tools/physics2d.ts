import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerPhysics2DTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_physics2d_addRigidbody", "Add Rigidbody2D to a GameObject.", {
    ...goRef,
    bodyType: z.enum(["Dynamic", "Kinematic", "Static"]).optional(),
    gravityScale: z.number().optional(),
    mass: z.number().optional(),
    linearDamping: z.number().optional(),
    angularDamping: z.number().optional(),
    freezeRotation: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.addRigidbody", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_setRigidbody", "Set Rigidbody2D properties.", {
    ...goRef,
    bodyType: z.enum(["Dynamic", "Kinematic", "Static"]).optional(),
    gravityScale: z.number().optional(),
    mass: z.number().optional(),
    linearDamping: z.number().optional(),
    angularDamping: z.number().optional(),
    freezeRotation: z.boolean().optional(),
    simulated: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.setRigidbody", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_addCollider", "Add a 2D collider to a GameObject.", {
    ...goRef,
    type: z.enum(["Box", "Circle", "Capsule", "Polygon", "Edge", "Composite"]).describe("Collider type"),
    isTrigger: z.boolean().optional(),
    offset: z.object({ x: z.number(), y: z.number() }).optional(),
    size: z.object({ x: z.number(), y: z.number() }).optional().describe("For BoxCollider2D"),
    radius: z.number().optional().describe("For CircleCollider2D/CapsuleCollider2D"),
    direction: z.enum(["Vertical", "Horizontal"]).optional().describe("For CapsuleCollider2D"),
  }, async (p) => {
    const r = await bridge.request("physics2d.addCollider", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_setCollider", "Set 2D collider properties.", {
    ...goRef,
    colliderType: z.string().describe("Collider type name"),
    isTrigger: z.boolean().optional(),
    offset: z.object({ x: z.number(), y: z.number() }).optional(),
    size: z.object({ x: z.number(), y: z.number() }).optional(),
    radius: z.number().optional(),
    usedByComposite: z.boolean().optional(),
    usedByEffector: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.setCollider", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_addJoint", "Add a 2D joint to a GameObject.", {
    ...goRef,
    type: z.enum(["Hinge", "Spring", "Distance", "Fixed", "Slider", "Relative", "Friction", "Target", "Wheel"]).describe("Joint type"),
    connectedBody: z.string().optional().describe("Connected Rigidbody2D name"),
    anchor: z.object({ x: z.number(), y: z.number() }).optional(),
    autoConfigureConnectedAnchor: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.addJoint", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_raycast", "Perform a 2D raycast.", {
    origin: z.object({ x: z.number(), y: z.number() }).describe("Ray origin"),
    direction: z.object({ x: z.number(), y: z.number() }).describe("Ray direction"),
    distance: z.number().optional(),
    layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.raycast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_overlapCircle", "Find all colliders within a circle.", {
    point: z.object({ x: z.number(), y: z.number() }).describe("Center point"),
    radius: z.number().describe("Circle radius"),
    layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.overlapCircle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_overlapBox", "Find all colliders within a box.", {
    point: z.object({ x: z.number(), y: z.number() }).describe("Center point"),
    size: z.object({ x: z.number(), y: z.number() }).describe("Box size"),
    angle: z.number().optional(),
    layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.overlapBox", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_createMaterial", "Create a PhysicsMaterial2D asset.", {
    name: z.string().describe("Material name"),
    savePath: z.string().optional(),
    friction: z.number().optional(),
    bounciness: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.createMaterial", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_getGravity", "Get 2D physics gravity settings.", {}, async (p) => {
    const r = await bridge.request("physics2d.getGravity", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_setGravity", "Set 2D physics gravity.", {
    gravity: z.object({ x: z.number(), y: z.number() }).describe("Gravity vector"),
  }, async (p) => {
    const r = await bridge.request("physics2d.setGravity", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_addEffector", "Add a 2D effector to a GameObject.", {
    ...goRef,
    type: z.enum(["Area", "Buoyancy", "Point", "Platform", "Surface"]).describe("Effector type"),
    useColliderMask: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.addEffector", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_setEffector", "Set 2D effector properties.", {
    ...goRef,
    effectorType: z.string().describe("Effector type name"),
    forceMagnitude: z.number().optional(),
    forceAngle: z.number().optional(),
    surfaceSpeed: z.number().optional(),
    useOneWay: z.boolean().optional(),
    density: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.setEffector", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
