import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerBatchTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_batch_create",
    "Batch create objects",
    {
      items: z.array(z.object({
        name: z.string(),
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
    "Batch set transforms",
    {
      items: z.array(z.object({
        path: z.string().optional(),
        name: z.string().optional(),
        instanceId: z.number().optional(),
        position: vec3.optional(),
        rotation: vec3.optional(),
        scale: vec3.optional(),
        space: z.enum(["world", "local"]).optional(),
      })),
    },
    async (params) => {
      const result = await bridge.request("scene.setTransformBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_batch_delete",
    "Batch delete objects",
    {
      items: z.array(z.object({
        path: z.string().optional(),
        name: z.string().optional(),
        instanceId: z.number().optional(),
      })),
    },
    async (params) => {
      const result = await bridge.request("scene.deleteBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_batch_importSettings",
    "Batch import settings",
    {
      items: z.array(z.object({
        path: z.string(),
        action: z.enum(["get", "set"]).optional(),
        settings: z.record(z.string(), z.any()).optional(),
      })),
    },
    async (params) => {
      const result = await bridge.request("asset.importSettingsBatch", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
