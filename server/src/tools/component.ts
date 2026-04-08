import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerComponentTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_component_list", "GameObject의 모든 컴포넌트를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    includeInherited: z.boolean().optional().describe("상속 타입 포함"),
  }, async (p) => {
    const r = await bridge.request("component.list", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_get", "특정 컴포넌트 정보를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string().describe("컴포넌트 타입"), index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.get", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_remove", "컴포넌트를 제거합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string().describe("컴포넌트 타입"), index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.remove", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_enable", "컴포넌트를 활성화/비활성화합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string().describe("컴포넌트 타입"),
    enabled: z.boolean().optional().describe("활성화 (기본: true)"),
    index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.enable", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_copy", "컴포넌트를 클립보드에 복사합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string().describe("컴포넌트 타입"), index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.copy", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_paste", "복사된 컴포넌트를 붙여넣기합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    asNew: z.boolean().optional().describe("새 컴포넌트로 추가 (기본: false=값만 덮어쓰기)"),
  }, async (p) => {
    const r = await bridge.request("component.paste", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_get_all_properties", "컴포넌트의 모든 SerializedProperty를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string().describe("컴포넌트 타입"), index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.getAll", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_move", "컴포넌트 순서를 변경합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string().describe("컴포넌트 타입"),
    direction: z.enum(["up", "down"]).describe("이동 방향"),
    index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.move", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_enable_batch", "여러 컴포넌트를 한 번에 활성화/비활성화합니다.", {
    items: z.array(z.object({
      path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
      componentType: z.string(), enabled: z.boolean().optional(), index: z.number().optional(),
    })),
  }, async (p) => {
    const r = await bridge.request("component.enableBatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_remove_batch", "여러 컴포넌트를 한 번에 제거합니다.", {
    items: z.array(z.object({
      path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
      componentType: z.string(), index: z.number().optional(),
    })),
  }, async (p) => {
    const r = await bridge.request("component.removeBatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
