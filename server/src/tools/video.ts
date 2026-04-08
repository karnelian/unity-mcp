import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerVideoTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_video_addPlayer", "Add a VideoPlayer component to a GameObject.", {
    ...goRef,
    source: z.enum(["VideoClip", "Url"]).optional(),
    url: z.string().optional().describe("Video URL (if source=Url)"),
    clipPath: z.string().optional().describe("VideoClip asset path (if source=VideoClip)"),
    playOnAwake: z.boolean().optional(),
    isLooping: z.boolean().optional(),
    renderMode: z.enum(["CameraFarPlane", "CameraNearPlane", "RenderTexture", "MaterialOverride"]).optional(),
  }, async (p) => {
    const r = await bridge.request("video.addPlayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_video_setPlayer", "Set VideoPlayer properties.", {
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
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_video_getInfo", "Get VideoPlayer information.", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("video.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_video_find", "Find all VideoPlayer components in the scene.", {}, async (p) => {
    const r = await bridge.request("video.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_video_findClips", "Search for VideoClip assets in the project.", {
    nameFilter: z.string().optional(),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("video.findClips", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
