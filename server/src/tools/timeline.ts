import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerTimelineTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_timeline_createClip",
    "Create AnimationClip",
    {
      savePath: z.string().describe("저장 경로 (예: Assets/Animations/Walk.anim)"),
      name: z.string().optional(),
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
    "Get clip info",
    {
      clipPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("timeline.getClipInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_setCurve",
    "Set animation curve",
    {
      clipPath: z.string(),
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
    "Get animation curves",
    {
      clipPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("timeline.getCurves", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_addEvent",
    "Add animation event",
    {
      clipPath: z.string(),
      time: z.number(),
      functionName: z.string(),
      stringParameter: z.string().optional(),
      intParameter: z.number().optional(),
      floatParameter: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("timeline.addEvent", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_getEvents",
    "Get animation events",
    {
      clipPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("timeline.getEvents", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_setClipSettings",
    "Set clip settings",
    {
      clipPath: z.string(),
      loopTime: z.boolean().optional(),
      loopBlend: z.boolean().optional(),
      keepOriginalOrientation: z.boolean().optional(),
      keepOriginalPositionXZ: z.boolean().optional(),
      keepOriginalPositionY: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("timeline.setClipSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_duplicateClip",
    "Duplicate clip",
    {
      clipPath: z.string(),
      newName: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("timeline.duplicateClip", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_findClips",
    "Find AnimationClips",
    {
      folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
      nameFilter: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("timeline.findClips", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_timeline_deleteKey",
    "Delete keyframe",
    {
      clipPath: z.string(),
      propertyName: z.string(),
      objectPath: z.string().optional(),
      keyIndex: z.number(),
    },
    async (params) => {
      const result = await bridge.request("timeline.deleteKey", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
