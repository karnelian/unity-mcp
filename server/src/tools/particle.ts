import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });
const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });

export function registerParticleTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_particle_create", "Create ParticleSystem", {
    name: z.string().optional(),
    parent: z.string().optional(),
    position: vec3.optional(),
    startLifetime: z.number().optional(),
    startSpeed: z.number().optional(),
    startSize: z.number().optional(),
    startColor: color.optional(),
    maxParticles: z.number().optional(),
    duration: z.number().optional(),
    loop: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_getInfo", "Get ParticleSystem info", {
    name: z.string().optional(),
    path: z.string().optional(),
    instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setMain", "Set particle main", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    duration: z.number().optional(),
    loop: z.boolean().optional(),
    startLifetime: z.number().optional(),
    startSpeed: z.number().optional(),
    startSize: z.number().optional(),
    startColor: color.optional(),
    gravityModifier: z.number().optional(),
    maxParticles: z.number().optional(),
    playOnAwake: z.boolean().optional(),
    simulationSpace: z.enum(["Local", "World", "Custom"]).optional(),
    scalingMode: z.enum(["Hierarchy", "Local", "Shape"]).optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setMain", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setEmission", "Set particle emission", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    rateOverTime: z.number().optional(),
    rateOverDistance: z.number().optional(),
    bursts: z.array(z.object({
      time: z.number(),
      count: z.number(),
      cycles: z.number().optional(),
      interval: z.number().optional(),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setEmission", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setShape", "Set particle shape", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    shapeType: z.enum(["Sphere", "Hemisphere", "Cone", "Box", "Circle", "Rectangle", "Donut", "Mesh", "MeshRenderer", "SkinnedMeshRenderer"]).optional(),
    radius: z.number().optional(), angle: z.number().optional(), arc: z.number().optional(),
    position: vec3.optional(), rotation: vec3.optional(), scale: vec3.optional(),
    radiusThickness: z.number().optional(),
    length: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setShape", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setRenderer", "Set particle renderer", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    renderMode: z.enum(["Billboard", "Stretch", "HorizontalBillboard", "VerticalBillboard", "Mesh"]).optional(),
    materialPath: z.string().optional(),
    sortingOrder: z.number().optional(),
    sortingLayerName: z.string().optional(),
    minParticleSize: z.number().optional(),
    maxParticleSize: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setRenderer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setColorOverLifetime", "Set particle color", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    colorKeys: z.array(z.object({
      time: z.number(),
      r: z.number(), g: z.number(), b: z.number(),
    })).optional(),
    alphaKeys: z.array(z.object({
      time: z.number(),
      alpha: z.number(),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setColorOverLifetime", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setSizeOverLifetime", "Set particle size", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    sizeMultiplier: z.number().optional(),
    curve: z.array(z.object({
      time: z.number(), value: z.number(),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setSizeOverLifetime", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setRotationOverLifetime", "Set particle rotation", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    angularVelocity: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setRotationOverLifetime", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setNoise", "Set particle noise", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    strength: z.number().optional(),
    frequency: z.number().optional(),
    scrollSpeed: z.number().optional(),
    octaveCount: z.number().optional(),
    damping: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setNoise", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setCollision", "Set particle collision", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    type: z.enum(["Planes", "World"]).optional(),
    mode: z.enum(["Collision2D", "Collision3D"]).optional(),
    bounce: z.number().optional(), dampen: z.number().optional(),
    lifetimeLoss: z.number().optional(),
    enableDynamicColliders: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setCollision", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setTrails", "Set particle trails", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    ratio: z.number().optional(),
    lifetime: z.number().optional(),
    minVertexDistance: z.number().optional(),
    worldSpace: z.boolean().optional(),
    dieWithParticles: z.boolean().optional(),
    widthOverTrail: z.number().optional(),
    textureMode: z.enum(["Stretch", "Tile", "DistributePerSegment", "RepeatPerSegment"]).optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setTrails", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setVelocityOverLifetime", "Set particle velocity", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    x: z.number().optional(), y: z.number().optional(), z: z.number().optional(),
    space: z.enum(["Local", "World"]).optional(),
    speedModifier: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setVelocityOverLifetime", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_play", "Control particle playback", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    action: z.enum(["play", "stop", "pause", "clear", "restart"]),
  }, async (p) => {
    const r = await bridge.request("particle.play", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_find", "Find ParticleSystems", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setSubEmitters", "Set particle sub-emitters", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setSubEmitters", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_particle_setTextureSheetAnimation", "Set particle texture sheet", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    enabled: z.boolean().optional(),
    numTilesX: z.number().optional(),
    numTilesY: z.number().optional(),
    cycleCount: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("particle.setTextureSheetAnimation", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
