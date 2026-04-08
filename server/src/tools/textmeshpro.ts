import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional().describe("GameObject name"),
  path: z.string().optional().describe("GameObject path"),
  instanceId: z.number().optional().describe("Instance ID"),
};

export function registerTextMeshProTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_tmp_createUI", "Create a TextMeshPro UI text element (requires Canvas parent).", {
    name: z.string().optional().describe("GameObject name"),
    parent: z.string().optional().describe("Parent Canvas or UI element"),
    text: z.string().optional().describe("Text content"),
    fontSize: z.number().optional(),
    color: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    alignment: z.string().optional().describe("Text alignment (e.g., Center, Left, Right)"),
    fontStyle: z.string().optional().describe("Bold, Italic, Underline, etc."),
  }, async (p) => {
    const r = await bridge.request("tmp.createUI", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_create3D", "Create a 3D world-space TextMeshPro text.", {
    name: z.string().optional(),
    text: z.string().optional(),
    fontSize: z.number().optional(),
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    color: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.create3D", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_setText", "Set text content on a TMP component.", {
    ...goRef,
    text: z.string().describe("New text content"),
  }, async (p) => {
    const r = await bridge.request("tmp.setText", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_setStyle", "Set TMP text style properties.", {
    ...goRef,
    fontSize: z.number().optional(),
    fontStyle: z.string().optional().describe("Bold, Italic, Underline, Strikethrough"),
    alignment: z.string().optional(),
    color: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    overflowMode: z.string().optional().describe("Overflow, Ellipsis, Masking, Truncate, ScrollRect"),
    enableWordWrapping: z.boolean().optional(),
    lineSpacing: z.number().optional(),
    characterSpacing: z.number().optional(),
    paragraphSpacing: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.setStyle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_setFont", "Set TMP font asset.", {
    ...goRef,
    fontAssetPath: z.string().describe("Path to TMP_FontAsset"),
    materialPresetPath: z.string().optional().describe("Path to material preset"),
  }, async (p) => {
    const r = await bridge.request("tmp.setFont", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_getInfo", "Get TMP component information.", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("tmp.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_find", "Find all TextMeshPro components in the scene.", {
    textFilter: z.string().optional().describe("Filter by text content"),
  }, async (p) => {
    const r = await bridge.request("tmp.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_findFontAssets", "Find TMP font assets in the project.", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.findFontAssets", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
