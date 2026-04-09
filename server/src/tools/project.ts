import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerProjectTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_project_info", "Get project info", {}, async () => {
    const r = await bridge.request("project.info", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_render_pipeline", "Get pipeline info", {}, async () => {
    const r = await bridge.request("project.getRenderPipeline", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_quality_settings", "Get QualitySettings", {}, async () => {
    const r = await bridge.request("project.getQualitySettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_set_quality_level", "Set quality level", {
    level: z.number(),
    applyExpensiveChanges: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setQualityLevel", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_player_settings", "Get PlayerSettings", {}, async () => {
    const r = await bridge.request("project.getPlayerSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_set_player_settings", "Set PlayerSettings", {
    productName: z.string().optional(), companyName: z.string().optional(),
    bundleVersion: z.string().optional(),
    defaultScreenWidth: z.number().optional(), defaultScreenHeight: z.number().optional(),
    runInBackground: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setPlayerSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_get_tags", "Get tags", {}, async () => {
    const r = await bridge.request("project.getTags", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_get_layers", "Get layers", {}, async () => {
    const r = await bridge.request("project.getLayers", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_add_tag", "Add tag", {
    tag: z.string(),
  }, async (p) => {
    const r = await bridge.request("project.addTag", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_add_layer", "Add layer", {
    layer: z.string(),
  }, async (p) => {
    const r = await bridge.request("project.addLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_time_settings", "Get TimeSettings", {}, async () => {
    const r = await bridge.request("project.getTimeSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_set_time", "Set TimeSettings", {
    fixedDeltaTime: z.number().optional(), maximumDeltaTime: z.number().optional(),
    timeScale: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setTimeSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_getBuildTarget", "Get build target", {}, async () => {
    const r = await bridge.request("project.getBuildTarget", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_setBuildTarget", "Set build target", {
    target: z.string(),
  }, async (p) => {
    const r = await bridge.request("project.setBuildTarget", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_getAndroidSettings", "Get Android settings", {}, async () => {
    const r = await bridge.request("project.getAndroidSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_setAndroidSettings", "Set Android settings", {
    packageName: z.string().optional(),
    minSdkVersion: z.string().optional(),
    targetSdkVersion: z.string().optional(),
    targetArchitectures: z.string().optional(),
    scriptingBackend: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setAndroidSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_getIOSSettings", "Get iOS settings", {}, async () => {
    const r = await bridge.request("project.getIOSSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_project_setIOSSettings", "Set iOS settings", {
    bundleIdentifier: z.string().optional(),
    targetOSVersionString: z.string().optional(),
    sdkVersion: z.string().optional(),
    targetDevice: z.string().optional(),
    scriptingBackend: z.string().optional(),
    automaticallySign: z.boolean().optional(),
    teamId: z.string().optional(),
    cameraUsageDescription: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("project.setIOSSettings", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
