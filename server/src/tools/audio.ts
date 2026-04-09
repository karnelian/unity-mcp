import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAudioTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_audio_add_source", "Add AudioSource", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    clipPath: z.string().optional(), volume: z.number().optional(), pitch: z.number().optional(),
    loop: z.boolean().optional(), playOnAwake: z.boolean().optional(), spatialBlend: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.addSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_source", "Set AudioSource properties", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    clipPath: z.string().optional(), volume: z.number().optional(), pitch: z.number().optional(),
    loop: z.boolean().optional(), playOnAwake: z.boolean().optional(), spatialBlend: z.number().optional(),
    mute: z.boolean().optional(), minDistance: z.number().optional(), maxDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.setSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_get_source", "Get AudioSource properties", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.getSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_play", "Play AudioSource", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.play", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_stop", "Stop AudioSource", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.stop", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_add_listener", "Add AudioListener", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.addListener", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_clip", "Set AudioSource clip", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    clipPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("audio.setClip", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_find_sources", "Find AudioSources", {}, async () => {
    const r = await bridge.request("audio.findSources", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_get_clip_info", "Get AudioClip info", {
    clipPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("audio.getClipInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_global_volume", "Set global volume", {
    volume: z.number().describe("볼륨 (0.0 ~ 1.0)"),
  }, async (p) => {
    const r = await bridge.request("audio.setGlobalVolume", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_pause", "Pause AudioSource", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.pause", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_unpause", "Unpause AudioSource", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.unpause", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_play_one_shot", "Play one shot", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    clipPath: z.string(),
    volume: z.number().optional().describe("볼륨 (기본: 1.0)"),
  }, async (p) => {
    const r = await bridge.request("audio.playOneShot", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_mixer_group", "Set mixer group", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    mixerPath: z.string(),
    groupName: z.string().optional().describe("그룹 이름 (기본: Master)"),
  }, async (p) => {
    const r = await bridge.request("audio.setMixerGroup", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_get_mixer_float", "Get mixer float", {
    mixerPath: z.string(),
    parameterName: z.string(),
  }, async (p) => {
    const r = await bridge.request("audio.getMixerFloat", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_mixer_float", "Set mixer float", {
    mixerPath: z.string(),
    parameterName: z.string(),
    value: z.number(),
  }, async (p) => {
    const r = await bridge.request("audio.setMixerFloat", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
