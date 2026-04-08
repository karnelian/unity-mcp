import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerPresetTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_preset_create", "Create a Preset from a component or asset.", {
    sourcePath: z.string().optional().describe("Source asset path"),
    sourceGameObject: z.string().optional().describe("Source GameObject name"),
    componentType: z.string().optional().describe("Component type to preset"),
    savePath: z.string().describe("Save path for the preset (e.g., Assets/Presets/MyPreset.preset)"),
  }, async (p) => {
    const r = await bridge.request("preset.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_preset_apply", "Apply a Preset to a component or asset.", {
    presetPath: z.string().describe("Preset asset path"),
    targetPath: z.string().optional().describe("Target asset path"),
    targetGameObject: z.string().optional().describe("Target GameObject name"),
    componentType: z.string().optional().describe("Target component type"),
  }, async (p) => {
    const r = await bridge.request("preset.apply", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_preset_getInfo", "Get Preset information.", {
    path: z.string().describe("Preset asset path"),
  }, async (p) => {
    const r = await bridge.request("preset.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_preset_find", "Find all Preset assets in the project.", {
    nameFilter: z.string().optional(),
    typeFilter: z.string().optional().describe("Filter by target component type"),
    folder: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("preset.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_preset_setAsDefault", "Set a Preset as the default for its type.", {
    presetPath: z.string().describe("Preset asset path"),
    enabled: z.boolean().optional().describe("Enable/disable as default (default: true)"),
    filter: z.string().optional().describe("Optional filter string"),
  }, async (p) => {
    const r = await bridge.request("preset.setAsDefault", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
