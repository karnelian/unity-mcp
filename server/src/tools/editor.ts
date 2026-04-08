import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerEditorTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_editor_playMode",
    "Play 모드를 제어합니다 (Play/Stop/Pause/Step/Status).",
    {
      action: z.enum(["play", "stop", "pause", "step", "status"]).describe("수행할 작업"),
    },
    async (params) => {
      const result = await bridge.request("editor.playMode", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_build",
    "플레이어를 빌드합니다. Windows/Android/iOS/WebGL/macOS/Linux 지원.",
    {
      target: z.enum(["Windows", "Android", "iOS", "WebGL", "macOS", "Linux"]).describe("빌드 타겟"),
      scenes: z.array(z.string()).optional().describe("포함할 씬 경로 목록 (기본: 빌드 설정의 활성 씬)"),
      outputPath: z.string().optional().describe("출력 경로"),
      options: z.array(z.string()).optional().describe("빌드 옵션 (예: ['Development', 'AllowDebugging'])"),
    },
    async (params) => {
      const result = await bridge.request("editor.build", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_buildSettings",
    "빌드 설정을 조회하거나 수정합니다.",
    {
      action: z.enum(["get", "set"]).describe("조회 또는 수정"),
      settings: z.record(z.string(), z.any()).optional().describe("수정할 설정"),
    },
    async (params) => {
      const result = await bridge.request("editor.buildSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_executeMenu",
    "Unity 메뉴 아이템을 실행합니다.",
    {
      menuPath: z.string().describe("메뉴 경로 (예: 'GameObject/3D Object/Cube')"),
    },
    async (params) => {
      const result = await bridge.request("editor.executeMenu", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_runTests",
    "EditMode 또는 PlayMode 테스트를 실행합니다.",
    {
      mode: z.enum(["EditMode", "PlayMode"]).optional().describe("테스트 모드 (기본: EditMode)"),
      filter: z.string().optional().describe("테스트 이름 필터"),
    },
    async (params) => {
      const result = await bridge.request("editor.runTests", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_testList",
    "프로젝트의 테스트 어셈블리 목록을 조회합니다.",
    {
      mode: z.enum(["EditMode", "PlayMode"]).optional().describe("테스트 모드 (기본: EditMode)"),
    },
    async (params) => {
      const result = await bridge.request("editor.testList", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_testResults",
    "가장 최근 테스트 실행 결과를 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("editor.testResults", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_connectionStatus",
    "MCP와 Unity Editor 간 연결 상태를 확인합니다.",
    {},
    async () => {
      const status = bridge.getStatus();
      return { content: [{ type: "text", text: JSON.stringify(status, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_console",
    "Unity 콘솔 로그를 조회합니다.",
    {
      type: z.enum(["error", "warning", "log", "all"]).optional().describe("로그 타입 필터 (기본: all)"),
      count: z.number().optional().describe("조회 개수 (기본: 50)"),
      clear: z.boolean().optional().describe("콘솔 클리어 후 조회"),
    },
    async (params) => {
      const result = await bridge.request("editor.console", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_projectInfo",
    "에디터 프로젝트 상세 정보를 조회합니다 (렌더 파이프라인, 스크립팅 백엔드, 컬러 스페이스 등).",
    {},
    async (params) => {
      const result = await bridge.request("editor.projectInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_editor_diagnostics",
    "서버 진단 정보를 조회합니다 (큐, 통계, 연결 상태).",
    {},
    async () => {
      const result = await bridge.request("editor.diagnostics", {});
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

}
