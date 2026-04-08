import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { UnityBridge } from "./bridge/unity-bridge.js";
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

const server = new McpServer({
  name: "karnellabs-unity-mcp",
  version: "0.3.0",
});

const bridge = new UnityBridge({
  host: process.env.UNITY_WS_HOST || "127.0.0.1",
  port: parseInt(process.env.UNITY_WS_PORT || "8099", 10),
  timeouts: {
    default: 30_000,
    build: 300_000,
  },
  reconnect: {
    interval: 3_000,
    maxAttempts: Infinity,
  },
});

// Phase 1 도구 등록
registerSceneTools(server, bridge);
registerScriptTools(server, bridge);
registerAssetTools(server, bridge);
registerEditorTools(server, bridge);
registerDebugTools(server, bridge);
registerWorkflowTools(server, bridge);
registerBatchTools(server, bridge);
registerMaterialTools(server, bridge);
registerLightTools(server, bridge);
registerCameraTools(server, bridge);
registerPhysicsTools(server, bridge);
registerUITools(server, bridge);
registerPrefabTools(server, bridge);
registerComponentTools(server, bridge);
registerAudioTools(server, bridge);
registerAnimatorTools(server, bridge);
registerTerrainTools(server, bridge);
registerNavMeshTools(server, bridge);
registerShaderTools(server, bridge);
registerProjectTools(server, bridge);
registerScriptableObjectTools(server, bridge);
registerTextureTools(server, bridge);
registerModelTools(server, bridge);
registerPackageTools(server, bridge);
registerValidationTools(server, bridge);
registerOptimizationTools(server, bridge);
registerPlacementTools(server, bridge);
registerTimelineTools(server, bridge);
registerProfilerTools(server, bridge);
registerCleanerTools(server, bridge);
registerCinemachineTools(server, bridge);
registerProBuilderTools(server, bridge);
registerXRTools(server, bridge);
registerPerceptionTools(server, bridge);
registerEventTools(server, bridge);
registerSmartTools(server, bridge);
registerParticleTools(server, bridge);
registerTilemap2DTools(server, bridge);
registerRenderingTools(server, bridge);
registerSplineTools(server, bridge);
registerVFXTools(server, bridge);
registerInputSystemTools(server, bridge);
registerAddressablesTools(server, bridge);
registerUIToolkitTools(server, bridge);
registerSpriteTools(server, bridge);
registerAnimation2DTools(server, bridge);
registerLocalizationTools(server, bridge);
registerVersionControlTools(server, bridge);
registerJointTools(server, bridge);
registerPhysics2DTools(server, bridge);
registerLODTools(server, bridge);
registerCharacterControllerTools(server, bridge);
registerTextMeshProTools(server, bridge);
registerLightmappingTools(server, bridge);
registerVideoTools(server, bridge);
registerLineRendererTools(server, bridge);
registerConstraintTools(server, bridge);
registerScrollRectTools(server, bridge);
registerCanvasGroupTools(server, bridge);
registerUIMaskTools(server, bridge);
registerClothTools(server, bridge);
registerSortingLayerTools(server, bridge);
registerOcclusionCullingTools(server, bridge);
registerRenderTextureTools(server, bridge);
registerSceneViewTools(server, bridge);
registerAsmdefTools(server, bridge);
registerSpriteShapeTools(server, bridge);
registerSkeletal2DTools(server, bridge);
registerRuleTileTools(server, bridge);
registerComputeShaderTools(server, bridge);
registerRenderFeatureTools(server, bridge);
registerPresetTools(server, bridge);
registerUnitySearchTools(server, bridge);
registerGridLayoutTools(server, bridge);

// 리소스 등록
registerResources(server, bridge);

// stdio transport로 시작
const transport = new StdioServerTransport();
await server.connect(transport);
