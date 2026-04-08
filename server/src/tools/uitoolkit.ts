import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerUIToolkitTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_uitoolkit_createUIDocument", "UIDocument 오브젝트를 생성합니다.", {
    name: z.string().optional(),
    uxmlPath: z.string().optional().describe("UXML 에셋 경로"),
    panelSettingsPath: z.string().optional().describe("PanelSettings 에셋 경로"),
    sortingOrder: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.createUIDocument", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_getInfo", "UIDocument 정보를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_findUIDocuments", "씬의 UIDocument를 검색합니다.", {}, async () => {
    const r = await bridge.request("uitoolkit.findUIDocuments", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_createUXML", "UXML 파일을 생성합니다.", {
    name: z.string().optional().describe("파일 이름 (기본: NewUI)"),
    folder: z.string().optional().describe("저장 폴더 (기본: Assets)"),
    elements: z.array(z.object({
      tag: z.string().describe("UI 요소 태그 (예: Label, Button, VisualElement, TextField, Toggle)"),
      name: z.string().optional(),
      text: z.string().optional(),
      classes: z.string().optional().describe("CSS 클래스 (공백 구분)"),
    })).optional().describe("초기 UI 요소 배열"),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.createUXML", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_createUSS", "USS 스타일시트를 생성합니다.", {
    name: z.string().optional().describe("파일 이름 (기본: NewStyle)"),
    folder: z.string().optional().describe("저장 폴더 (기본: Assets)"),
    rules: z.array(z.object({
      selector: z.string().describe("CSS 선택자 (예: #root, .my-class, Button)"),
      properties: z.record(z.string(), z.string()).describe("속성 (예: {background-color: '#333', font-size: '14px'})"),
    })).optional().describe("스타일 규칙 배열"),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.createUSS", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_setPanelSettings", "UIDocument의 PanelSettings/UXML을 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    panelSettingsPath: z.string().optional(),
    uxmlPath: z.string().optional(),
    sortingOrder: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.setPanelSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_createPanelSettings", "PanelSettings 에셋을 생성합니다.", {
    name: z.string().optional(),
    folder: z.string().optional(),
    scaleMode: z.enum(["ConstantPixelSize", "ConstantPhysicalSize", "ScaleWithScreenSize"]).optional(),
    referenceResolutionX: z.number().optional(),
    referenceResolutionY: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.createPanelSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_uitoolkit_getHierarchy", "UIDocument의 VisualElement 계층 구조를 조회합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("uitoolkit.getHierarchy", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
