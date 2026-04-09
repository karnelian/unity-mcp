import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerRenderTextureTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_renderTexture_create", "Create RenderTexture", {
    path: z.string(),
    width: z.number().optional(),
    height: z.number().optional(),
    depth: z.number().optional(),
    colorFormat: z.string().optional(),
    antiAliasing: z.number().optional(),
    enableRandomWrite: z.boolean().optional(),
    useMipMap: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("renderTexture.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderTexture_set", "Set RenderTexture", {
    path: z.string(),
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

  server.tool("unity_renderTexture_assign", "Assign RenderTexture", {
    renderTexturePath: z.string(),
    targetCamera: z.string().optional(),
    targetMaterial: z.string().optional(),
    materialProperty: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("renderTexture.assign", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderTexture_getInfo", "Get RenderTexture info", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("renderTexture.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_renderTexture_find", "Find RenderTextures", {
    nameFilter: z.string().optional(),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("renderTexture.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
