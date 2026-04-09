import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerModelTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_model_getSettings",
    "Get model settings",
    {
      modelPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("model.getSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setSettings",
    "Set model settings",
    {
      modelPath: z.string(),
      globalScale: z.number().optional(),
      meshCompression: z.string().optional(),
      isReadable: z.boolean().optional(),
      importNormals: z.string().optional(),
      importAnimation: z.boolean().optional(),
      generateColliders: z.boolean().optional(),
      optimizeMesh: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("model.setSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_getInfo",
    "Get mesh info",
    {
      modelPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("model.getInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_find",
    "Find 3D models",
    {
      folder: z.string().optional(),
      nameFilter: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("model.find", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_getMeshInfo",
    "Get model info",
    {
      modelPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("model.getMeshInfo", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setRigType",
    "Set rig type",
    {
      modelPath: z.string(),
      rigType: z.string(),
    },
    async (params) => {
      const result = await bridge.request("model.setRigType", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_getAnimations",
    "Get model animations",
    {
      modelPath: z.string(),
    },
    async (params) => {
      const result = await bridge.request("model.getAnimations", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setAnimationSettings",
    "Set animation import settings",
    {
      modelPath: z.string(),
      clipName: z.string(),
      loop: z.boolean().optional(),
      name: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("model.setAnimationSettings", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setScale",
    "Set model scale",
    {
      modelPath: z.string(),
      scale: z.number(),
    },
    async (params) => {
      const result = await bridge.request("model.setScale", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_model_setMaterialImport",
    "Set material import settings",
    {
      modelPath: z.string(),
      mode: z.string(),
    },
    async (params) => {
      const result = await bridge.request("model.setMaterialImport", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
