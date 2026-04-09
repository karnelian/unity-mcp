import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerPresetTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_preset_create", "Create Preset", {
    sourcePath: z.string().optional(),
    sourceGameObject: z.string().optional(),
    componentType: z.string().optional(),
    savePath: z.string(),
  }, async (p) => {
    const r = await bridge.request("preset.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_preset_apply", "Apply Preset", {
    presetPath: z.string(),
    targetPath: z.string().optional(),
    targetGameObject: z.string().optional(),
    componentType: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("preset.apply", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_preset_getInfo", "Get Preset info", {
    path: z.string(),
  }, async (p) => {
    const r = await bridge.request("preset.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_preset_find", "Find Presets", {
    nameFilter: z.string().optional(),
    typeFilter: z.string().optional(),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("preset.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_preset_setAsDefault", "Set default Preset", {
    presetPath: z.string(),
    enabled: z.boolean().optional().describe("Enable/disable as default (default: true)"),
    filter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("preset.setAsDefault", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
