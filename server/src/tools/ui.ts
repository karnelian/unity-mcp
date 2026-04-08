import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const color = z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() });
const vec2 = z.object({ x: z.number(), y: z.number() });

export function registerUITools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_ui_create_canvas", "UI Canvas를 생성합니다.", {
    name: z.string().optional(), renderMode: z.enum(["ScreenSpaceOverlay", "ScreenSpaceCamera", "WorldSpace"]).optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createCanvas", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_create_panel", "UI Panel을 생성합니다.", {
    name: z.string().optional(), parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createPanel", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_create_button", "UI Button을 생성합니다.", {
    name: z.string().optional(), text: z.string().optional(), parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createButton", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_create_text", "UI Text를 생성합니다.", {
    name: z.string().optional(), text: z.string().optional(), fontSize: z.number().optional(),
    alignment: z.string().optional(), color: color.optional(), parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createText", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_create_image", "UI Image를 생성합니다.", {
    name: z.string().optional(), spritePath: z.string().optional(), color: color.optional(), parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createImage", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_create_slider", "UI Slider를 생성합니다.", {
    name: z.string().optional(), minValue: z.number().optional(), maxValue: z.number().optional(),
    value: z.number().optional(), parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createSlider", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_create_toggle", "UI Toggle을 생성합니다.", {
    name: z.string().optional(), isOn: z.boolean().optional(), parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createToggle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_create_dropdown", "UI Dropdown을 생성합니다.", {
    name: z.string().optional(), options: z.array(z.string()).optional(), parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createDropdown", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_create_input_field", "UI InputField를 생성합니다.", {
    name: z.string().optional(), placeholder: z.string().optional(), text: z.string().optional(), parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.createInputField", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_set_rect_transform", "RectTransform을 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    anchoredPosition: vec2.optional(), sizeDelta: vec2.optional(),
    anchorMin: vec2.optional(), anchorMax: vec2.optional(), pivot: vec2.optional(),
  }, async (p) => {
    const r = await bridge.request("ui.setRectTransform", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_set_text", "Text 컴포넌트를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    text: z.string().optional(), fontSize: z.number().optional(), alignment: z.string().optional(),
    color: color.optional(), fontStyle: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.setText", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_set_image", "Image 컴포넌트를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    spritePath: z.string().optional(), color: color.optional(),
    type: z.enum(["Simple", "Sliced", "Tiled", "Filled"]).optional(),
    fillAmount: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.setImage", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_set_button", "Button 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    interactable: z.boolean().optional(), text: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.setButton", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_set_slider", "Slider 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    value: z.number().optional(), minValue: z.number().optional(), maxValue: z.number().optional(),
    wholeNumbers: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.setSlider", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_set_toggle", "Toggle 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    isOn: z.boolean().optional(), interactable: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.setToggle", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_add_layout", "Layout Group을 추가합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    layoutType: z.enum(["horizontal", "vertical", "grid"]).describe("레이아웃 타입"),
    spacing: z.union([z.number(), vec2]).optional(),
    padding: z.object({ left: z.number().optional(), right: z.number().optional(), top: z.number().optional(), bottom: z.number().optional() }).optional(),
    cellSize: vec2.optional().describe("grid 전용"),
  }, async (p) => {
    const r = await bridge.request("ui.addLayout", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_find", "씬의 모든 UI Canvas를 조회합니다.", {}, async () => {
    const r = await bridge.request("ui.findUI", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_ui_set_canvas", "Canvas 프로퍼티를 수정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    renderMode: z.enum(["ScreenSpaceOverlay", "ScreenSpaceCamera", "WorldSpace"]).optional(),
    sortingOrder: z.number().optional(), pixelPerfect: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("ui.setCanvasProperties", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
