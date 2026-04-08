import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAnimatorTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_animator_create_controller", "Animator Controller를 생성합니다.", {
    name: z.string().describe("컨트롤러 이름"), savePath: z.string().optional(),
    parameters: z.array(z.object({ name: z.string(), type: z.enum(["Float", "Int", "Bool", "Trigger"]) })).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.createController", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_assign_controller", "Animator에 Controller를 할당합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    controllerPath: z.string().describe("컨트롤러 에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("animator.assignController", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_add_parameter", "Animator Controller에 파라미터를 추가합니다.", {
    controllerPath: z.string().describe("컨트롤러 경로"),
    parameterName: z.string(), parameterType: z.enum(["Float", "Int", "Bool", "Trigger"]).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.addParameter", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_remove_parameter", "파라미터를 제거합니다.", {
    controllerPath: z.string(), parameterName: z.string(),
  }, async (p) => {
    const r = await bridge.request("animator.removeParameter", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_get_parameters", "Animator Controller의 파라미터를 조회합니다.", {
    controllerPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("animator.getParameters", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_set_parameter", "런타임 Animator 파라미터를 설정합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    parameterName: z.string(), parameterType: z.enum(["Float", "Int", "Bool", "Trigger"]).optional(),
    value: z.union([z.number(), z.boolean()]).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.setParameter", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_add_state", "Animator State를 추가합니다.", {
    controllerPath: z.string(), stateName: z.string(), layerIndex: z.number().optional(),
    clipPath: z.string().optional(), isDefault: z.boolean().optional(), speed: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("animator.addState", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_add_transition", "State 간 Transition을 추가합니다.", {
    controllerPath: z.string(), fromState: z.string(), toState: z.string(),
    layerIndex: z.number().optional(), hasExitTime: z.boolean().optional(),
    duration: z.number().optional(), exitTime: z.number().optional(),
    conditions: z.array(z.object({
      parameter: z.string(), mode: z.enum(["If", "IfNot", "Greater", "Less", "Equals", "NotEqual"]),
      threshold: z.number().optional(),
    })).optional(),
  }, async (p) => {
    const r = await bridge.request("animator.addTransition", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_get_states", "Animator Layer의 State 목록을 조회합니다.", {
    controllerPath: z.string(), layerIndex: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("animator.getStates", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_get_info", "GameObject의 Animator 정보를 조회합니다.", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("animator.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_add_layer", "애니메이터 컨트롤러에 레이어를 추가합니다.", {
    controllerPath: z.string().describe("컨트롤러 에셋 경로"),
    layerName: z.string().describe("레이어 이름"),
    defaultWeight: z.number().optional().describe("기본 가중치 (기본: 0)"),
    blendingMode: z.enum(["Override", "Additive"]).optional().describe("블렌딩 모드"),
  }, async (p) => {
    const r = await bridge.request("animator.addLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_remove_layer", "애니메이터 레이어를 제거합니다.", {
    controllerPath: z.string().describe("컨트롤러 에셋 경로"),
    layerIndex: z.number().describe("레이어 인덱스 (0=Base 제거 불가)"),
  }, async (p) => {
    const r = await bridge.request("animator.removeLayer", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_set_layer_weight", "런타임 레이어 가중치를 설정합니다.", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    layerIndex: z.number().describe("레이어 인덱스"),
    weight: z.number().describe("가중치 (0~1)"),
  }, async (p) => {
    const r = await bridge.request("animator.setLayerWeight", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_get_layers", "애니메이터 레이어 목록을 조회합니다.", {
    controllerPath: z.string().describe("컨트롤러 에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("animator.getLayers", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_create_blend_tree", "블렌드 트리를 생성합니다.", {
    controllerPath: z.string().describe("컨트롤러 에셋 경로"),
    treeName: z.string().optional().describe("블렌드 트리 이름"),
    parameterName: z.string().describe("블렌드 파라미터 이름"),
    layerIndex: z.number().optional().describe("레이어 인덱스 (기본: 0)"),
    blendType: z.enum(["Simple1D", "SimpleDirectional2D", "FreeformDirectional2D", "FreeformCartesian2D"]).optional(),
    motions: z.array(z.object({
      clipPath: z.string().optional(), threshold: z.number().optional(),
    })).optional().describe("모션 목록"),
  }, async (p) => {
    const r = await bridge.request("animator.createBlendTree", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_remove_state", "애니메이터 상태를 제거합니다.", {
    controllerPath: z.string().describe("컨트롤러 에셋 경로"),
    stateName: z.string().describe("상태 이름"),
    layerIndex: z.number().optional().describe("레이어 인덱스 (기본: 0)"),
  }, async (p) => {
    const r = await bridge.request("animator.removeState", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_animator_remove_transition", "애니메이터 트랜지션을 제거합니다.", {
    controllerPath: z.string().describe("컨트롤러 에셋 경로"),
    fromState: z.string().describe("출발 상태"), toState: z.string().describe("도착 상태"),
    layerIndex: z.number().optional().describe("레이어 인덱스 (기본: 0)"),
  }, async (p) => {
    const r = await bridge.request("animator.removeTransition", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
