import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAudioTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_audio_add_source", "AudioSource를 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    clipPath: z.string().optional(), volume: z.number().optional(), pitch: z.number().optional(),
    loop: z.boolean().optional(), playOnAwake: z.boolean().optional(), spatialBlend: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.addSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_source", "AudioSource 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    clipPath: z.string().optional(), volume: z.number().optional(), pitch: z.number().optional(),
    loop: z.boolean().optional(), playOnAwake: z.boolean().optional(), spatialBlend: z.number().optional(),
    mute: z.boolean().optional(), minDistance: z.number().optional(), maxDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.setSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_get_source", "AudioSource 프로퍼티를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.getSource", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_play", "AudioSource를 재생합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.play", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_stop", "AudioSource를 정지합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.stop", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_add_listener", "AudioListener를 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.addListener", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_clip", "AudioSource의 클립을 변경합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    clipPath: z.string().describe("AudioClip 에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("audio.setClip", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_find_sources", "씬의 모든 AudioSource를 검색합니다.", {}, async () => {
    const r = await bridge.request("audio.findSources", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_get_clip_info", "AudioClip 에셋 정보를 조회합니다.", {
    clipPath: z.string().describe("AudioClip 경로"),
  }, async (p) => {
    const r = await bridge.request("audio.getClipInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_global_volume", "전역 오디오 볼륨을 설정합니다.", {
    volume: z.number().describe("볼륨 (0.0 ~ 1.0)"),
  }, async (p) => {
    const r = await bridge.request("audio.setGlobalVolume", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_pause", "AudioSource를 일시정지합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.pause", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_unpause", "AudioSource 일시정지를 해제합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("audio.unpause", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_play_one_shot", "현재 재생을 중단하지 않고 클립을 한 번 재생합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    clipPath: z.string().describe("AudioClip 에셋 경로"),
    volume: z.number().optional().describe("볼륨 (기본: 1.0)"),
  }, async (p) => {
    const r = await bridge.request("audio.playOneShot", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_mixer_group", "AudioSource의 출력 Mixer 그룹을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    mixerPath: z.string().describe("AudioMixer 에셋 경로"),
    groupName: z.string().optional().describe("그룹 이름 (기본: Master)"),
  }, async (p) => {
    const r = await bridge.request("audio.setMixerGroup", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_get_mixer_float", "AudioMixer의 노출 파라미터를 조회합니다.", {
    mixerPath: z.string().describe("AudioMixer 에셋 경로"),
    parameterName: z.string().describe("파라미터 이름"),
  }, async (p) => {
    const r = await bridge.request("audio.getMixerFloat", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_audio_set_mixer_float", "AudioMixer의 노출 파라미터를 설정합니다.", {
    mixerPath: z.string().describe("AudioMixer 에셋 경로"),
    parameterName: z.string().describe("파라미터 이름"),
    value: z.number().describe("값"),
  }, async (p) => {
    const r = await bridge.request("audio.setMixerFloat", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
