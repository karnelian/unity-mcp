import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerVideoTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_video_addPlayer", "Add VideoPlayer", {
    ...goRef,
    source: z.enum(["VideoClip", "Url"]).optional(),
    url: z.string().optional(),
    clipPath: z.string().optional(),
    playOnAwake: z.boolean().optional(),
    isLooping: z.boolean().optional(),
    renderMode: z.enum(["CameraFarPlane", "CameraNearPlane", "RenderTexture", "MaterialOverride"]).optional(),
  }, async (p) => {
    const r = await bridge.request("video.addPlayer", p);
    return textResult(r);
  });

  server.tool("unity_video_setPlayer", "Set VideoPlayer", {
    ...goRef,
    url: z.string().optional(),
    clipPath: z.string().optional(),
    playOnAwake: z.boolean().optional(),
    isLooping: z.boolean().optional(),
    playbackSpeed: z.number().optional(),
    renderMode: z.string().optional(),
    audioOutputMode: z.enum(["None", "AudioSource", "Direct"]).optional(),
  }, async (p) => {
    const r = await bridge.request("video.setPlayer", p);
    return textResult(r);
  });

  server.tool("unity_video_getInfo", "Get VideoPlayer info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("video.getInfo", p);
    return textResult(r);
  });

  server.tool("unity_video_find", "Find VideoPlayers", {}, async (p) => {
    const r = await bridge.request("video.find", p);
    return textResult(r);
  });

  server.tool("unity_video_findClips", "Find VideoClips", {
    nameFilter: z.string().optional(),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("video.findClips", p);
    return textResult(r);
  });
}
