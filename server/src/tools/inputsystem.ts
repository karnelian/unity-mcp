import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerInputSystemTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_input_createActionAsset", "Create InputActionAsset", {
    name: z.string().optional().describe("에셋 이름 (기본: InputActions)"),
    folder: z.string().optional().describe("저장 폴더 (기본: Assets)"),
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
    expectedControlType: z.string().optional().describe("컨트롤 타입 (예: Vector2, Button, Axis)"),
  }, async (p) => {
    const r = await bridge.request("input.addAction", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addBinding", "Add input binding", {
    assetPath: z.string(),
    mapName: z.string(),
    actionName: z.string(),
    bindingPath: z.string().describe("바인딩 경로 (예: <Keyboard>/space, <Gamepad>/buttonSouth)"),
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
    defaultMap: z.string().optional().describe("기본 ActionMap 이름"),
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
