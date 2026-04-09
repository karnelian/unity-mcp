import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerPhysics2DTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_physics2d_addRigidbody", "Add Rigidbody2D", {
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

  server.tool("unity_physics2d_setRigidbody", "Set Rigidbody2D", {
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

  server.tool("unity_physics2d_addCollider", "Add Collider2D", {
    ...goRef,
    type: z.enum(["Box", "Circle", "Capsule", "Polygon", "Edge", "Composite"]),
    isTrigger: z.boolean().optional(),
    offset: z.object({ x: z.number(), y: z.number() }).optional(),
    size: z.object({ x: z.number(), y: z.number() }).optional(),
    radius: z.number().optional(),
    direction: z.enum(["Vertical", "Horizontal"]).optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.addCollider", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_setCollider", "Set Collider2D", {
    ...goRef,
    colliderType: z.string(),
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

  server.tool("unity_physics2d_addJoint", "Add Joint2D", {
    ...goRef,
    type: z.enum(["Hinge", "Spring", "Distance", "Fixed", "Slider", "Relative", "Friction", "Target", "Wheel"]),
    connectedBody: z.string().optional(),
    anchor: z.object({ x: z.number(), y: z.number() }).optional(),
    autoConfigureConnectedAnchor: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.addJoint", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_raycast", "Raycast2D", {
    origin: z.object({ x: z.number(), y: z.number() }),
    direction: z.object({ x: z.number(), y: z.number() }),
    distance: z.number().optional(),
    layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.raycast", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_overlapCircle", "OverlapCircle2D", {
    point: z.object({ x: z.number(), y: z.number() }),
    radius: z.number(),
    layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.overlapCircle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_overlapBox", "OverlapBox2D", {
    point: z.object({ x: z.number(), y: z.number() }),
    size: z.object({ x: z.number(), y: z.number() }),
    angle: z.number().optional(),
    layerMask: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.overlapBox", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_createMaterial", "Create PhysicsMaterial2D", {
    name: z.string(),
    savePath: z.string().optional(),
    friction: z.number().optional(),
    bounciness: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.createMaterial", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_getGravity", "Get 2D gravity", {}, async (p) => {
    const r = await bridge.request("physics2d.getGravity", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_setGravity", "Set 2D gravity", {
    gravity: z.object({ x: z.number(), y: z.number() }),
  }, async (p) => {
    const r = await bridge.request("physics2d.setGravity", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_addEffector", "Add Effector2D", {
    ...goRef,
    type: z.enum(["Area", "Buoyancy", "Point", "Platform", "Surface"]),
    useColliderMask: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("physics2d.addEffector", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_physics2d_setEffector", "Set Effector2D", {
    ...goRef,
    effectorType: z.string(),
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
