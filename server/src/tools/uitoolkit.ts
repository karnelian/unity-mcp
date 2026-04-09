import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerUIToolkitTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_uitoolkit_createUIDocument", "Create UIDocument", {
    name: z.string().optional(),
    uxmlPath: z.string().optional(),
    panelSettingsPath: z.string().optional(),
    sortingOrder: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.createUIDocument", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_getInfo", "Get UIDocument info", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_findUIDocuments", "Find UIDocuments", {}, async () => {
    const r = await bridge.request("uitoolkit.findUIDocuments", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_createUXML", "Create UXML", {
    name: z.string().optional(),
    folder: z.string().optional(),
    elements: z.array(z.object({
      tag: z.string(),
      name: z.string().optional(),
      text: z.string().optional(),
      classes: z.string().optional(),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.createUXML", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_createUSS", "Create USS", {
    name: z.string().optional(),
    folder: z.string().optional(),
    rules: z.array(z.object({
      selector: z.string(),
      properties: z.record(z.string(), z.string()),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.createUSS", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_setPanelSettings", "Set panel settings", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    panelSettingsPath: z.string().optional(),
    uxmlPath: z.string().optional(),
    sortingOrder: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.setPanelSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_createPanelSettings", "Create PanelSettings", {
    name: z.string().optional(),
    folder: z.string().optional(),
    scaleMode: z.enum(["ConstantPixelSize", "ConstantPhysicalSize", "ScaleWithScreenSize"]).optional(),
    referenceResolutionX: z.number().optional(),
    referenceResolutionY: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.createPanelSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_getHierarchy", "Get UI hierarchy", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.getHierarchy", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
