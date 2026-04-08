import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerProjectTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_project_info", "프로젝트 정보를 조회합니다.", {}, async () => {
    const r = await bridge.request("project.info", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_render_pipeline", "현재 렌더 파이프라인 정보를 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getRenderPipeline", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_quality_settings", "Quality Settings을 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getQualitySettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_set_quality_level", "Quality Level을 설정합니다.", {
    level: z.number().describe("품질 레벨 인덱스"),
    applyExpensiveChanges: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setQualityLevel", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_player_settings", "Player Settings을 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getPlayerSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_set_player_settings", "Player Settings을 수정합니다.", {
    productName: z.string().optional(), companyName: z.string().optional(),
    bundleVersion: z.string().optional(),
    defaultScreenWidth: z.number().optional(), defaultScreenHeight: z.number().optional(),
    runInBackground: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setPlayerSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_get_tags", "프로젝트 태그 목록을 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getTags", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_get_layers", "프로젝트 레이어 목록을 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getLayers", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_add_tag", "태그를 추가합니다.", {
    tag: z.string().describe("태그 이름"),
  }, async (p) => {
    const r = await bridge.request("project.addTag", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_add_layer", "레이어를 추가합니다.", {
    layer: z.string().describe("레이어 이름"),
  }, async (p) => {
    const r = await bridge.request("project.addLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_time_settings", "Time Settings을 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getTimeSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_set_time", "Time Settings을 수정합니다.", {
    fixedDeltaTime: z.number().optional(), maximumDeltaTime: z.number().optional(),
    timeScale: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setTimeSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_getBuildTarget", "현재 빌드 타겟을 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getBuildTarget", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_setBuildTarget", "빌드 타겟을 변경합니다.", {
    target: z.string().describe("빌드 타겟 (예: StandaloneWindows64, Android, iOS, WebGL)"),
  }, async (p) => {
    const r = await bridge.request("project.setBuildTarget", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_getAndroidSettings", "Android 빌드 설정을 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getAndroidSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_setAndroidSettings", "Android 빌드 설정을 수정합니다.", {
    packageName: z.string().optional(),
    minSdkVersion: z.string().optional(),
    targetSdkVersion: z.string().optional(),
    targetArchitectures: z.string().optional().describe("ARM64, ARMv7, X86 등"),
    scriptingBackend: z.string().optional().describe("IL2CPP, Mono2x"),
  }, async (p) => {
    const r = await bridge.request("project.setAndroidSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_getIOSSettings", "iOS 빌드 설정을 조회합니다.", {}, async () => {
    const r = await bridge.request("project.getIOSSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_setIOSSettings", "iOS 빌드 설정을 수정합니다.", {
    bundleIdentifier: z.string().optional(),
    targetOSVersionString: z.string().optional(),
    sdkVersion: z.string().optional(),
    targetDevice: z.string().optional().describe("iPhoneOnly, iPadOnly, iPhoneAndiPad"),
    scriptingBackend: z.string().optional(),
    automaticallySign: z.boolean().optional(),
    teamId: z.string().optional(),
    cameraUsageDescription: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setIOSSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
