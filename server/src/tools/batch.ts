import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerBatchTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_batch_create",
    "여러 GameObject를 한 번에 생성합니다. per-item 에러 격리.",
    {
      items: z.array(z.object({
        name: z.string().describe("오브젝트 이름"),
        type: z.enum(["empty", "primitive", "prefab"]).optional(),
        primitiveType: z.enum(["Cube", "Sphere", "Capsule", "Cylinder", "Plane", "Quad"]).optional(),
        prefabPath: z.string().optional(),
        parent: z.string().optional(),
        position: vec3.optional(),
        rotation: vec3.optional(),
        scale: vec3.optional(),
      })).describe("생성할 오브젝트 배열 (최대 500개)"),
    },
    async (params) => {
      const result = await bridge.request("scene.createBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_batch_setTransform",
    "여러 GameObject의 Transform을 한 번에 수정합니다.",
    {
      items: z.array(z.object({
        path: z.string().optional(),
        name: z.string().optional(),
        instanceId: z.number().optional(),
        position: vec3.optional(),
        rotation: vec3.optional(),
        scale: vec3.optional(),
        space: z.enum(["world", "local"]).optional(),
      })).describe("수정할 오브젝트+Transform 배열"),
    },
    async (params) => {
      const result = await bridge.request("scene.setTransformBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_batch_delete",
    "여러 GameObject를 한 번에 삭제합니다.",
    {
      items: z.array(z.object({
        path: z.string().optional(),
        name: z.string().optional(),
        instanceId: z.number().optional(),
      })).describe("삭제할 오브젝트 배열"),
    },
    async (params) => {
      const result = await bridge.request("scene.deleteBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_batch_importSettings",
    "여러 에셋의 임포트 설정을 한 번에 수정합니다.",
    {
      items: z.array(z.object({
        path: z.string().describe("에셋 경로"),
        action: z.enum(["get", "set"]).optional(),
        settings: z.record(z.string(), z.any()).optional(),
      })).describe("에셋 임포트 설정 배열"),
    },
    async (params) => {
      const result = await bridge.request("asset.importSettingsBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
