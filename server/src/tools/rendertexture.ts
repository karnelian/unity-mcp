import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerRenderTextureTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_renderTexture_create", "Create a RenderTexture asset.", {
    path: z.string().describe("Asset path (e.g., Assets/Textures/MyRT.renderTexture)"),
    width: z.number().optional().describe("Width in pixels (default: 256)"),
    height: z.number().optional().describe("Height in pixels (default: 256)"),
    depth: z.number().optional().describe("Depth buffer bits (0, 16, 24, 32)"),
    colorFormat: z.string().optional().describe("RenderTextureFormat (e.g., ARGB32, ARGBHalf)"),
    antiAliasing: z.number().optional().describe("Anti-aliasing (1, 2, 4, 8)"),
    enableRandomWrite: z.boolean().optional(),
    useMipMap: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("renderTexture.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderTexture_set", "Set RenderTexture properties.", {
    path: z.string().describe("Asset path"),
    width: z.number().optional(),
    height: z.number().optional(),
    depth: z.number().optional(),
    colorFormat: z.string().optional(),
    antiAliasing: z.number().optional(),
    filterMode: z.enum(["Point", "Bilinear", "Trilinear"]).optional(),
    wrapMode: z.enum(["Repeat", "Clamp", "Mirror", "MirrorOnce"]).optional(),
  }, async (p) => {
    const r = await bridge.request("renderTexture.set", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderTexture_assign", "Assign a RenderTexture to a Camera or Material.", {
    renderTexturePath: z.string().describe("RenderTexture asset path"),
    targetCamera: z.string().optional().describe("Camera GameObject to assign target texture"),
    targetMaterial: z.string().optional().describe("Material asset path to assign texture"),
    materialProperty: z.string().optional().describe("Material property name (default: _MainTex)"),
  }, async (p) => {
    const r = await bridge.request("renderTexture.assign", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderTexture_getInfo", "Get RenderTexture information.", {
    path: z.string().describe("Asset path"),
  }, async (p) => {
    const r = await bridge.request("renderTexture.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderTexture_find", "Find all RenderTexture assets in the project.", {
    nameFilter: z.string().optional(),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("renderTexture.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
