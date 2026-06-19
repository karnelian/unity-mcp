import { randomUUID } from "node:crypto";
import { createServer, type IncomingMessage, type ServerResponse } from "node:http";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { SSEServerTransport } from "@modelcontextprotocol/sdk/server/sse.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { StreamableHTTPServerTransport } from "@modelcontextprotocol/sdk/server/streamableHttp.js";
import { UnityBridge } from "./bridge/unity-bridge.js";
import { describeUnityTarget, resolveUnityTarget } from "./registry.js";
import { registerSceneTools } from "./tools/scene.js";
import { registerScriptTools } from "./tools/script.js";
import { registerAssetTools } from "./tools/asset.js";
import { registerEditorTools } from "./tools/editor.js";
import { registerDebugTools } from "./tools/debug.js";
import { registerWorkflowTools } from "./tools/workflow.js";
import { registerBatchTools } from "./tools/batch.js";
import { registerMaterialTools } from "./tools/material.js";
import { registerLightTools } from "./tools/light.js";
import { registerCameraTools } from "./tools/camera.js";
import { registerPhysicsTools } from "./tools/physics.js";
import { registerUITools } from "./tools/ui.js";
import { registerPrefabTools } from "./tools/prefab.js";
import { registerComponentTools } from "./tools/component.js";
import { registerAudioTools } from "./tools/audio.js";
import { registerAnimatorTools } from "./tools/animator.js";
import { registerTerrainTools } from "./tools/terrain.js";
import { registerNavMeshTools } from "./tools/navmesh.js";
import { registerShaderTools } from "./tools/shader.js";
import { registerProjectTools } from "./tools/project.js";
import { registerScriptableObjectTools } from "./tools/scriptableobject.js";
import { registerTextureTools } from "./tools/texture.js";
import { registerModelTools } from "./tools/model.js";
import { registerPackageTools } from "./tools/package.js";
import { registerValidationTools } from "./tools/validation.js";
import { registerOptimizationTools } from "./tools/optimization.js";
import { registerPlacementTools } from "./tools/placement.js";
import { registerTimelineTools } from "./tools/timeline.js";
import { registerProfilerTools } from "./tools/profiler.js";
import { registerCleanerTools } from "./tools/cleaner.js";
import { registerCinemachineTools } from "./tools/cinemachine.js";
import { registerProBuilderTools } from "./tools/probuilder.js";
import { registerXRTools } from "./tools/xr.js";
import { registerPerceptionTools } from "./tools/perception.js";
import { registerEventTools } from "./tools/event.js";
import { registerSmartTools } from "./tools/smart.js";
import { registerParticleTools } from "./tools/particle.js";
import { registerTilemap2DTools } from "./tools/tilemap2d.js";
import { registerRenderingTools } from "./tools/rendering.js";
import { registerSplineTools } from "./tools/spline.js";
import { registerVFXTools } from "./tools/vfx.js";
import { registerInputSystemTools } from "./tools/inputsystem.js";
import { registerAddressablesTools } from "./tools/addressables.js";
import { registerUIToolkitTools } from "./tools/uitoolkit.js";
import { registerSpriteTools } from "./tools/sprite.js";
import { registerAnimation2DTools } from "./tools/animation2d.js";
import { registerLocalizationTools } from "./tools/localization.js";
import { registerVersionControlTools } from "./tools/versioncontrol.js";
import { registerJointTools } from "./tools/joint.js";
import { registerPhysics2DTools } from "./tools/physics2d.js";
import { registerLODTools } from "./tools/lod.js";
import { registerCharacterControllerTools } from "./tools/charactercontroller.js";
import { registerTextMeshProTools } from "./tools/textmeshpro.js";
import { registerLightmappingTools } from "./tools/lightmapping.js";
import { registerVideoTools } from "./tools/video.js";
import { registerLineRendererTools } from "./tools/linerenderer.js";
import { registerConstraintTools } from "./tools/constraint.js";
import { registerScrollRectTools } from "./tools/scrollrect.js";
import { registerCanvasGroupTools } from "./tools/canvasgroup.js";
import { registerUIMaskTools } from "./tools/uimask.js";
import { registerClothTools } from "./tools/cloth.js";
import { registerSortingLayerTools } from "./tools/sortinglayer.js";
import { registerOcclusionCullingTools } from "./tools/occlusionculling.js";
import { registerRenderTextureTools } from "./tools/rendertexture.js";
import { registerSceneViewTools } from "./tools/sceneview.js";
import { registerAsmdefTools } from "./tools/asmdef.js";
import { registerSpriteShapeTools } from "./tools/spriteshape.js";
import { registerSkeletal2DTools } from "./tools/skeletal2d.js";
import { registerRuleTileTools } from "./tools/ruletile.js";
import { registerComputeShaderTools } from "./tools/computeshader.js";
import { registerRenderFeatureTools } from "./tools/renderfeature.js";
import { registerPresetTools } from "./tools/preset.js";
import { registerUnitySearchTools } from "./tools/unitysearch.js";
import { registerGridLayoutTools } from "./tools/gridlayout.js";
import { registerResources } from "./resources/index.js";

type ToolRegistrar = (server: McpServer, bridge: UnityBridge) => void;

interface ToolGroup {
  name: string;
  register: ToolRegistrar;
}

const TOOL_GROUPS: ToolGroup[] = [
  { name: "scene", register: registerSceneTools },
  { name: "script", register: registerScriptTools },
  { name: "asset", register: registerAssetTools },
  { name: "editor", register: registerEditorTools },
  { name: "debug", register: registerDebugTools },
  { name: "workflow", register: registerWorkflowTools },
  { name: "batch", register: registerBatchTools },
  { name: "material", register: registerMaterialTools },
  { name: "light", register: registerLightTools },
  { name: "camera", register: registerCameraTools },
  { name: "physics", register: registerPhysicsTools },
  { name: "ui", register: registerUITools },
  { name: "prefab", register: registerPrefabTools },
  { name: "component", register: registerComponentTools },
  { name: "audio", register: registerAudioTools },
  { name: "animator", register: registerAnimatorTools },
  { name: "terrain", register: registerTerrainTools },
  { name: "navmesh", register: registerNavMeshTools },
  { name: "shader", register: registerShaderTools },
  { name: "project", register: registerProjectTools },
  { name: "scriptableobject", register: registerScriptableObjectTools },
  { name: "texture", register: registerTextureTools },
  { name: "model", register: registerModelTools },
  { name: "package", register: registerPackageTools },
  { name: "validation", register: registerValidationTools },
  { name: "optimization", register: registerOptimizationTools },
  { name: "placement", register: registerPlacementTools },
  { name: "timeline", register: registerTimelineTools },
  { name: "profiler", register: registerProfilerTools },
  { name: "cleaner", register: registerCleanerTools },
  { name: "cinemachine", register: registerCinemachineTools },
  { name: "probuilder", register: registerProBuilderTools },
  { name: "xr", register: registerXRTools },
  { name: "perception", register: registerPerceptionTools },
  { name: "event", register: registerEventTools },
  { name: "smart", register: registerSmartTools },
  { name: "particle", register: registerParticleTools },
  { name: "tilemap2d", register: registerTilemap2DTools },
  { name: "rendering", register: registerRenderingTools },
  { name: "spline", register: registerSplineTools },
  { name: "vfx", register: registerVFXTools },
  { name: "inputsystem", register: registerInputSystemTools },
  { name: "addressables", register: registerAddressablesTools },
  { name: "uitoolkit", register: registerUIToolkitTools },
  { name: "sprite", register: registerSpriteTools },
  { name: "animation2d", register: registerAnimation2DTools },
  { name: "localization", register: registerLocalizationTools },
  { name: "versioncontrol", register: registerVersionControlTools },
  { name: "joint", register: registerJointTools },
  { name: "physics2d", register: registerPhysics2DTools },
  { name: "lod", register: registerLODTools },
  { name: "charactercontroller", register: registerCharacterControllerTools },
  { name: "textmeshpro", register: registerTextMeshProTools },
  { name: "lightmapping", register: registerLightmappingTools },
  { name: "video", register: registerVideoTools },
  { name: "linerenderer", register: registerLineRendererTools },
  { name: "constraint", register: registerConstraintTools },
  { name: "scrollrect", register: registerScrollRectTools },
  { name: "canvasgroup", register: registerCanvasGroupTools },
  { name: "uimask", register: registerUIMaskTools },
  { name: "cloth", register: registerClothTools },
  { name: "sortinglayer", register: registerSortingLayerTools },
  { name: "occlusionculling", register: registerOcclusionCullingTools },
  { name: "rendertexture", register: registerRenderTextureTools },
  { name: "sceneview", register: registerSceneViewTools },
  { name: "asmdef", register: registerAsmdefTools },
  { name: "spriteshape", register: registerSpriteShapeTools },
  { name: "skeletal2d", register: registerSkeletal2DTools },
  { name: "ruletile", register: registerRuleTileTools },
  { name: "computeshader", register: registerComputeShaderTools },
  { name: "renderfeature", register: registerRenderFeatureTools },
  { name: "preset", register: registerPresetTools },
  { name: "unitysearch", register: registerUnitySearchTools },
  { name: "gridlayout", register: registerGridLayoutTools }
];

const ALL_TOOL_GROUPS = TOOL_GROUPS.map(group => group.name);

const PROFILE_GROUPS: Record<string, string[]> = {
  core: [
    "project", "editor", "debug", "scene", "script", "asset", "component",
    "prefab", "package", "validation", "workflow", "batch", "asmdef", "unitysearch"
  ],
  ui: ["ui", "uitoolkit", "textmeshpro", "scrollrect", "canvasgroup", "uimask", "gridlayout"],
  "2d": ["tilemap2d", "sprite", "animation2d", "physics2d", "spriteshape", "skeletal2d", "ruletile", "sortinglayer"],
  rendering: ["material", "light", "camera", "shader", "texture", "rendering", "rendertexture", "renderfeature", "lightmapping", "occlusionculling", "computeshader"],
  animation: ["animator", "timeline", "cinemachine", "animation2d"],
  physics: ["physics", "physics2d", "navmesh", "joint", "charactercontroller", "cloth", "constraint"],
  assets: ["asset", "material", "texture", "model", "scriptableobject", "preset", "addressables", "localization"],
  mobile: ["profiler", "optimization", "cleaner", "texture", "model", "lod", "audio"],
  xr: ["xr", "inputsystem", "camera", "rendering"],
  media: ["audio", "video", "particle", "vfx", "linerenderer"],
  terrain: ["terrain", "navmesh", "placement"],
  full: ALL_TOOL_GROUPS,
};

function readOption(name: string, envName: string): string | undefined {
  const cliValue = process.argv.find(arg => arg.startsWith(`--${name}=`));
  if (cliValue) return cliValue.slice(name.length + 3);
  return process.env[envName];
}

function splitList(value: string | undefined): string[] {
  if (!value) return [];
  return value
    .split(",")
    .map(item => item.trim().toLowerCase())
    .filter(Boolean);
}

function resolveEnabledToolGroups(): string[] {
  const requestedProfiles = splitList(readOption("profile", "UNITY_MCP_PROFILE"));
  const requestedTools = splitList(readOption("tools", "UNITY_MCP_TOOLS"));
  const profiles = requestedProfiles.length > 0 ? requestedProfiles : ["core"];
  const knownToolGroups = new Set(ALL_TOOL_GROUPS);
  const enabled = new Set<string>();

  for (const profile of profiles) {
    const groups = PROFILE_GROUPS[profile];
    if (!groups) {
      console.error(`[Unity MCP] Unknown profile '${profile}'. Known profiles: ${Object.keys(PROFILE_GROUPS).join(", ")}`);
      continue;
    }
    for (const group of groups) enabled.add(group);
  }

  for (const tool of requestedTools) {
    if (!knownToolGroups.has(tool)) {
      console.error(`[Unity MCP] Unknown tool group '${tool}'. Known groups: ${ALL_TOOL_GROUPS.join(", ")}`);
      continue;
    }
    enabled.add(tool);
  }

  return ALL_TOOL_GROUPS.filter(group => enabled.has(group));
}

function readNumberOption(name: string, envName: string, fallback: number): number {
  const raw = readOption(name, envName);
  if (!raw) return fallback;
  const parsed = Number.parseInt(raw, 10);
  if (Number.isFinite(parsed) && parsed > 0 && parsed <= 65_535) return parsed;
  console.error(`[Unity MCP] Invalid --${name} value '${raw}', using ${fallback}`);
  return fallback;
}

function readMcpTransport(): "stdio" | "http" | "sse" {
  const raw = (readOption("transport", "UNITY_MCP_TRANSPORT") || "stdio").toLowerCase();
  if (raw === "stdio" || raw === "http" || raw === "sse") return raw;
  console.error(`[Unity MCP] Unknown transport '${raw}', using stdio. Expected: stdio, http, sse`);
  return "stdio";
}

function writeJson(res: ServerResponse, status: number, body: unknown): void {
  res.writeHead(status, { "content-type": "application/json" });
  res.end(JSON.stringify(body));
}

function writeNotFound(res: ServerResponse): void {
  writeJson(res, 404, { error: "not_found" });
}

function buildServer(bridge: UnityBridge): McpServer {
  const server = new McpServer({
    name: "karnellabs-unity-mcp",
    version: "0.3.10",
  });

  for (const group of TOOL_GROUPS) {
    if (enabledToolGroups.has(group.name)) {
      group.register(server, bridge);
    }
  }

  registerResources(server, bridge);
  return server;
}

async function startStdioTransport(bridge: UnityBridge): Promise<void> {
  const server = buildServer(bridge);
  const transport = new StdioServerTransport();
  await server.connect(transport);
}

async function startHttpTransport(bridge: UnityBridge): Promise<void> {
  const host = readOption("mcp-host", "UNITY_MCP_HTTP_HOST") || "127.0.0.1";
  const port = readNumberOption("mcp-port", "UNITY_MCP_HTTP_PORT", 3001);
  const endpoint = readOption("mcp-endpoint", "UNITY_MCP_HTTP_ENDPOINT") || "/mcp";
  const transport = new StreamableHTTPServerTransport({
    sessionIdGenerator: () => randomUUID(),
  });
  const server = buildServer(bridge);
  await server.connect(transport);

  const httpServer = createServer(async (req: IncomingMessage, res: ServerResponse) => {
    try {
      const url = new URL(req.url || "/", `http://${req.headers.host || `${host}:${port}`}`);
      if (url.pathname === "/health") {
        writeJson(res, 200, { ok: true, transport: "http", endpoint });
        return;
      }
      if (url.pathname !== endpoint) {
        writeNotFound(res);
        return;
      }
      await transport.handleRequest(req, res);
    } catch (error) {
      console.error("[Unity MCP] HTTP transport error:", error);
      if (!res.headersSent) {
        writeJson(res, 500, { error: "internal_error" });
      } else {
        res.end();
      }
    }
  });

  await new Promise<void>((resolve) => httpServer.listen(port, host, resolve));
  console.error(`[Unity MCP] MCP HTTP transport listening at http://${host}:${port}${endpoint}`);
}

async function startSseTransport(bridge: UnityBridge): Promise<void> {
  const host = readOption("mcp-host", "UNITY_MCP_HTTP_HOST") || "127.0.0.1";
  const port = readNumberOption("mcp-port", "UNITY_MCP_HTTP_PORT", 3001);
  const endpoint = readOption("sse-endpoint", "UNITY_MCP_SSE_ENDPOINT") || "/sse";
  const messageEndpoint = readOption("message-endpoint", "UNITY_MCP_SSE_MESSAGE_ENDPOINT") || "/message";
  const transports = new Map<string, SSEServerTransport>();

  const httpServer = createServer(async (req: IncomingMessage, res: ServerResponse) => {
    try {
      const url = new URL(req.url || "/", `http://${req.headers.host || `${host}:${port}`}`);
      if (url.pathname === "/health") {
        writeJson(res, 200, { ok: true, transport: "sse", endpoint, messageEndpoint });
        return;
      }
      if (req.method === "GET" && url.pathname === endpoint) {
        const transport = new SSEServerTransport(messageEndpoint, res);
        transports.set(transport.sessionId, transport);
        transport.onclose = () => transports.delete(transport.sessionId);
        const server = buildServer(bridge);
        await server.connect(transport);
        return;
      }
      if (req.method === "POST" && url.pathname === messageEndpoint) {
        const sessionId = url.searchParams.get("sessionId") || "";
        const transport = transports.get(sessionId);
        if (!transport) {
          writeJson(res, 404, { error: "unknown_session" });
          return;
        }
        await transport.handlePostMessage(req, res);
        return;
      }
      writeNotFound(res);
    } catch (error) {
      console.error("[Unity MCP] SSE transport error:", error);
      if (!res.headersSent) {
        writeJson(res, 500, { error: "internal_error" });
      } else {
        res.end();
      }
    }
  });

  await new Promise<void>((resolve) => httpServer.listen(port, host, resolve));
  console.error(`[Unity MCP] MCP SSE transport listening at http://${host}:${port}${endpoint}`);
}

const explicitPort = readOption("port", "UNITY_WS_PORT");
const unityTarget = resolveUnityTarget({
  host: process.env.UNITY_WS_HOST || "127.0.0.1",
  explicitPort,
  cwd: process.cwd(),
});
console.error(`[Unity MCP] Unity target: ${describeUnityTarget(unityTarget)}`);

const bridge = new UnityBridge({
  host: unityTarget.host,
  port: unityTarget.port,
  resolvePort: explicitPort ? undefined : async () => resolveUnityTarget({ host: unityTarget.host, cwd: process.cwd() }).port,
  timeouts: {
    default: 30_000,
    build: 300_000,
  },
  reconnect: {
    interval: 3_000,
    maxInterval: 30_000,
    maxAttempts: Infinity,
  },
});

const enabledToolGroups = new Set(resolveEnabledToolGroups());

console.error(
  `[Unity MCP] Tool profile '${readOption("profile", "UNITY_MCP_PROFILE") || "core"}' enabled ${enabledToolGroups.size}/${TOOL_GROUPS.length} tool groups: ${Array.from(enabledToolGroups).join(", ")}`
);

const mcpTransport = readMcpTransport();
console.error(`[Unity MCP] MCP client transport: ${mcpTransport}`);

if (mcpTransport === "http") {
  await startHttpTransport(bridge);
} else if (mcpTransport === "sse") {
  await startSseTransport(bridge);
} else {
  await startStdioTransport(bridge);
}
