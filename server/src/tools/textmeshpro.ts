import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerTextMeshProTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_tmp_createUI", "Create TMP UI text", {
    name: z.string().optional(),
    parent: z.string().optional(),
    text: z.string().optional(),
    fontSize: z.number().optional(),
    color: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    alignment: z.string().optional().describe("Text alignment (e.g., Center, Left, Right)"),
    fontStyle: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.createUI", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_create3D", "Create TMP 3D text", {
    name: z.string().optional(),
    text: z.string().optional(),
    fontSize: z.number().optional(),
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    color: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.create3D", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_setText", "Set TMP text", {
    ...goRef,
    text: z.string(),
  }, async (p) => {
    const r = await bridge.request("tmp.setText", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_setStyle", "Set TMP style", {
    ...goRef,
    fontSize: z.number().optional(),
    fontStyle: z.string().optional(),
    alignment: z.string().optional(),
    color: z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }).optional(),
    overflowMode: z.string().optional(),
    enableWordWrapping: z.boolean().optional(),
    lineSpacing: z.number().optional(),
    characterSpacing: z.number().optional(),
    paragraphSpacing: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.setStyle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_setFont", "Set TMP font", {
    ...goRef,
    fontAssetPath: z.string(),
    materialPresetPath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.setFont", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_getInfo", "Get TMP info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("tmp.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_find", "Find TMP objects", {
    textFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_tmp_findFontAssets", "Find TMP fonts", {
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("tmp.findFontAssets", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
