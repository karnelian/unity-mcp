import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerEventTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_event_listEvents",
    "GameObject의 모든 UnityEvent를 나열합니다 (Button.onClick, Slider.onValueChanged 등).",
    {
      path: z.string().describe("GameObject 경로 또는 이름"),
      componentType: z.string().optional().describe("특정 컴포넌트 타입 필터"),
    },
    async (params) => {
      const result = await bridge.request("event.listEvents", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_event_getListeners",
    "UnityEvent의 등록된 리스너(콜백) 목록을 조회합니다.",
    {
      path: z.string().describe("GameObject 경로 또는 이름"),
      eventName: z.string().describe("이벤트 필드명 (예: onClick, onValueChanged)"),
      componentType: z.string().optional().describe("컴포넌트 타입 (모호할 때)"),
    },
    async (params) => {
      const result = await bridge.request("event.getListeners", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_event_addListener",
    "UnityEvent에 영속 리스너를 추가합니다 (Inspector에서 설정하는 것과 동일).",
    {
      path: z.string().describe("이벤트를 가진 GameObject 경로"),
      eventName: z.string().describe("이벤트 필드명 (예: onClick)"),
      componentType: z.string().optional().describe("컴포넌트 타입"),
      targetPath: z.string().describe("대상 GameObject 경로 (또는 'self')"),
      methodName: z.string().describe("호출할 메서드 이름"),
      argumentType: z.enum(["void", "int", "float", "string", "bool", "object"]).optional().describe("인자 타입 (기본: void)"),
      argumentValue: z.any().optional().describe("인자 값"),
    },
    async (params) => {
      const result = await bridge.request("event.addListener", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_event_removeListener",
    "UnityEvent에서 리스너를 제거합니다.",
    {
      path: z.string().describe("GameObject 경로"),
      eventName: z.string().describe("이벤트 필드명"),
      componentType: z.string().optional().describe("컴포넌트 타입"),
      index: z.number().describe("제거할 리스너 인덱스"),
    },
    async (params) => {
      const result = await bridge.request("event.removeListener", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_event_setListenerState",
    "리스너의 호출 상태를 변경합니다 (Off/RuntimeOnly/EditorAndRuntime).",
    {
      path: z.string().describe("GameObject 경로"),
      eventName: z.string().describe("이벤트 필드명"),
      componentType: z.string().optional().describe("컴포넌트 타입"),
      index: z.number().describe("리스너 인덱스"),
      state: z.enum(["Off", "RuntimeOnly", "EditorAndRuntime"]).describe("호출 상태"),
    },
    async (params) => {
      const result = await bridge.request("event.setListenerState", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
