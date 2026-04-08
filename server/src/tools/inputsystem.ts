import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerInputSystemTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_input_createActionAsset", "InputActionAsset를 생성합니다.", {
    name: z.string().optional().describe("에셋 이름 (기본: InputActions)"),
    folder: z.string().optional().describe("저장 폴더 (기본: Assets)"),
    maps: z.array(z.string()).optional().describe("초기 ActionMap 이름 배열"),
  }, async (p) => {
    const r = await bridge.request("input.createActionAsset", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_getActionAsset", "InputActionAsset의 전체 구조를 조회합니다.", {
    assetPath: z.string().describe("에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("input.getActionAsset", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addActionMap", "ActionMap을 추가합니다.", {
    assetPath: z.string().describe("에셋 경로"),
    mapName: z.string().describe("ActionMap 이름"),
  }, async (p) => {
    const r = await bridge.request("input.addActionMap", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addAction", "ActionMap에 Action을 추가합니다.", {
    assetPath: z.string().describe("에셋 경로"),
    mapName: z.string().describe("ActionMap 이름"),
    actionName: z.string().describe("Action 이름"),
    actionType: z.enum(["Value", "Button", "PassThrough"]).optional(),
    expectedControlType: z.string().optional().describe("컨트롤 타입 (예: Vector2, Button, Axis)"),
  }, async (p) => {
    const r = await bridge.request("input.addAction", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addBinding", "Action에 바인딩을 추가합니다.", {
    assetPath: z.string().describe("에셋 경로"),
    mapName: z.string().describe("ActionMap 이름"),
    actionName: z.string().describe("Action 이름"),
    bindingPath: z.string().describe("바인딩 경로 (예: <Keyboard>/space, <Gamepad>/buttonSouth)"),
  }, async (p) => {
    const r = await bridge.request("input.addBinding", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_removeAction", "Action을 제거합니다.", {
    assetPath: z.string().describe("에셋 경로"),
    mapName: z.string().describe("ActionMap 이름"),
    actionName: z.string().describe("제거할 Action 이름"),
  }, async (p) => {
    const r = await bridge.request("input.removeAction", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_removeActionMap", "ActionMap을 제거합니다.", {
    assetPath: z.string().describe("에셋 경로"),
    mapName: z.string().describe("제거할 ActionMap 이름"),
  }, async (p) => {
    const r = await bridge.request("input.removeActionMap", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_findActionAssets", "프로젝트의 InputActionAsset를 검색합니다.", {}, async () => {
    const r = await bridge.request("input.findActionAssets", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_addPlayerInput", "오브젝트에 PlayerInput 컴포넌트를 추가합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    assetPath: z.string().optional().describe("InputActionAsset 경로"),
    defaultMap: z.string().optional().describe("기본 ActionMap 이름"),
  }, async (p) => {
    const r = await bridge.request("input.addPlayerInput", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_input_getPlayerInput", "PlayerInput 컴포넌트 정보를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("input.getPlayerInput", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
