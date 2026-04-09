import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerInputSystemTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_input_createActionAsset", "Create InputActionAsset", {
    name: z.string().optional(),
    folder: z.string().optional(),
    maps: z.array(z.string()).optional(),
  }, async (p) => {
    const r = await bridge.request("input.createActionAsset", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_getActionAsset", "Get InputActionAsset", {
    assetPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("input.getActionAsset", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addActionMap", "Add ActionMap", {
    assetPath: z.string(),
    mapName: z.string(),
  }, async (p) => {
    const r = await bridge.request("input.addActionMap", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addAction", "Add InputAction", {
    assetPath: z.string(),
    mapName: z.string(),
    actionName: z.string(),
    actionType: z.enum(["Value", "Button", "PassThrough"]).optional(),
    expectedControlType: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("input.addAction", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addBinding", "Add input binding", {
    assetPath: z.string(),
    mapName: z.string(),
    actionName: z.string(),
    bindingPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("input.addBinding", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_removeAction", "Remove InputAction", {
    assetPath: z.string(),
    mapName: z.string(),
    actionName: z.string(),
  }, async (p) => {
    const r = await bridge.request("input.removeAction", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_removeActionMap", "Remove ActionMap", {
    assetPath: z.string(),
    mapName: z.string(),
  }, async (p) => {
    const r = await bridge.request("input.removeActionMap", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_findActionAssets", "Find InputActionAssets", {}, async () => {
    const r = await bridge.request("input.findActionAssets", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addPlayerInput", "Add PlayerInput", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    assetPath: z.string().optional(),
    defaultMap: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("input.addPlayerInput", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_getPlayerInput", "Get PlayerInput", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("input.getPlayerInput", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
