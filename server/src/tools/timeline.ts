import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerTimelineTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_timeline_createClip",
    "새 AnimationClip을 생성합니다.",
    {
      savePath: z.string().describe("저장 경로 (예: Assets/Animations/Walk.anim)"),
      name: z.string().optional().describe("클립 이름"),
      frameRate: z.number().optional().describe("프레임 레이트 (기본: 60)"),
      wrapMode: z.string().optional().describe("래핑 모드 (Once, Loop, PingPong, ClampForever)"),
    },
    async (params) => {
      const result = await bridge.request("timeline.createClip", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_getClipInfo",
    "AnimationClip의 상세 정보를 조회합니다.",
    {
      clipPath: z.string().describe("클립 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("timeline.getClipInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_setCurve",
    "AnimationClip에 애니메이션 커브를 설정합니다.",
    {
      clipPath: z.string().describe("클립 에셋 경로"),
      propertyName: z.string().describe("프로퍼티 이름 (예: localPosition.x)"),
      componentType: z.string().describe("컴포넌트 타입 (예: Transform)"),
      objectPath: z.string().optional().describe("오브젝트 상대 경로 (기본: 루트)"),
      keys: z.array(z.array(z.number())).describe("키프레임 [[time, value], ...]"),
    },
    async (params) => {
      const result = await bridge.request("timeline.setCurve", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_getCurves",
    "AnimationClip의 모든 커브 정보를 조회합니다.",
    {
      clipPath: z.string().describe("클립 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("timeline.getCurves", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_addEvent",
    "AnimationClip에 이벤트를 추가합니다.",
    {
      clipPath: z.string().describe("클립 에셋 경로"),
      time: z.number().describe("이벤트 시간 (초)"),
      functionName: z.string().describe("호출할 함수 이름"),
      stringParameter: z.string().optional().describe("문자열 파라미터"),
      intParameter: z.number().optional().describe("정수 파라미터"),
      floatParameter: z.number().optional().describe("실수 파라미터"),
    },
    async (params) => {
      const result = await bridge.request("timeline.addEvent", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_getEvents",
    "AnimationClip의 이벤트 목록을 조회합니다.",
    {
      clipPath: z.string().describe("클립 에셋 경로"),
    },
    async (params) => {
      const result = await bridge.request("timeline.getEvents", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_setClipSettings",
    "AnimationClip의 설정을 변경합니다 (루프, 루트 모션 등).",
    {
      clipPath: z.string().describe("클립 에셋 경로"),
      loopTime: z.boolean().optional().describe("루프 여부"),
      loopBlend: z.boolean().optional().describe("루프 블렌드 여부"),
      keepOriginalOrientation: z.boolean().optional().describe("원본 방향 유지"),
      keepOriginalPositionXZ: z.boolean().optional().describe("원본 XZ 위치 유지"),
      keepOriginalPositionY: z.boolean().optional().describe("원본 Y 위치 유지"),
    },
    async (params) => {
      const result = await bridge.request("timeline.setClipSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_duplicateClip",
    "AnimationClip을 복제합니다.",
    {
      clipPath: z.string().describe("원본 클립 경로"),
      newName: z.string().optional().describe("새 이름"),
    },
    async (params) => {
      const result = await bridge.request("timeline.duplicateClip", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_findClips",
    "프로젝트에서 AnimationClip을 검색합니다.",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
      nameFilter: z.string().optional().describe("이름 필터"),
    },
    async (params) => {
      const result = await bridge.request("timeline.findClips", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_deleteKey",
    "AnimationClip 커브에서 키프레임을 삭제합니다.",
    {
      clipPath: z.string().describe("클립 에셋 경로"),
      propertyName: z.string().describe("프로퍼티 이름"),
      objectPath: z.string().optional().describe("오브젝트 상대 경로"),
      keyIndex: z.number().describe("삭제할 키 인덱스"),
    },
    async (params) => {
      const result = await bridge.request("timeline.deleteKey", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
