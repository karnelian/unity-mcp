import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";
import { withSafety } from "./safety.js";

export function registerSceneTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_scene_hierarchy",
    "Get scene hierarchy",
    {
      path: z.string().optional(),
      depth: z.number().optional().default(2),
      includeComponents: z.boolean().optional().default(false),
      nameFilter: z.string().optional(),
      maxResults: z.number().optional().describe("Paginate large array fields in the response; default 50 when set"),
      offset: z.number().optional(),
      summaryOnly: z.boolean().optional().describe("Return counts for large arrays instead of full item details"),
      includeDetails: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.hierarchy", params);
      return textResult(result, params);
    }
  );

  server.tool(
    "unity_scene_create",
    "Create object",
    withSafety({
      name: z.string(),
      type: z.enum(["empty", "primitive", "prefab"]).optional(),
      primitiveType: z.enum(["Cube", "Sphere", "Capsule", "Cylinder", "Plane", "Quad"]).optional(),
      prefabPath: z.string().optional(),
      parent: z.string().optional(),
      position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    }),
    async (params) => {
      const result = await bridge.request("scene.create", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_scene_setTransform",
    "Set transform",
    withSafety({
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
      rotation: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
      scale: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
      space: z.enum(["world", "local"]).optional(),
    }),
    async (params) => {
      const result = await bridge.request("scene.setTransform", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_scene_addComponent",
    "Add component",
    withSafety({
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      componentType: z.string(),
    }),
    async (params) => {
      const result = await bridge.request("scene.addComponent", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_scene_setComponent",
    "Set component property",
    withSafety({
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      componentType: z.string(),
      properties: z.record(z.string(), z.any()),
    }),
    async (params) => {
      const result = await bridge.request("scene.setComponent", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_scene_delete",
    "Delete object",
    withSafety({
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
    }),
    async (params) => {
      const result = await bridge.request("scene.delete", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_scene_duplicate",
    "Duplicate object",
    withSafety({
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      count: z.number().optional(),
      offset: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    }),
    async (params) => {
      const result = await bridge.request("scene.duplicate", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_scene_manage",
    "Manage scenes",
    withSafety({
      action: z.enum(["save", "open", "create", "info"]),
      scenePath: z.string().optional(),
    }),
    async (params) => {
      const result = await bridge.request("scene.manage", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_scene_find",
    "Find objects",
    {
      name: z.string().optional(),
      tag: z.string().optional(),
      layer: z.string().optional(),
      componentType: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.find", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_scene_select",
    "Set selection",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      ping: z.boolean().optional(),
      frame: z.boolean().optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.select", params);
      return textResult(result);
    }
  );

  server.tool("unity_scene_listLoaded", "List loaded scenes", {}, async () => {
    const r = await bridge.request("scene.listLoaded", {});
    return textResult(r);
  });

  server.tool("unity_scene_setActive", "Set active scene", withSafety({
    sceneName: z.string().optional(),
    scenePath: z.string().optional(),
  }), async (p) => {
    const r = await bridge.request("scene.setActiveScene", p);
    return textResult(r);
  });

  server.tool("unity_scene_moveToScene", "Move to scene", withSafety({
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    targetScene: z.string().optional(),
    targetScenePath: z.string().optional(),
  }), async (p) => {
    const r = await bridge.request("scene.moveToScene", p);
    return textResult(r);
  });

  server.tool("unity_scene_openAdditive", "Open scene additive", withSafety({
    scenePath: z.string(),
  }), async (p) => {
    const r = await bridge.request("scene.openAdditive", p);
    return textResult(r);
  });

  server.tool("unity_scene_close", "Close scene", withSafety({
    sceneName: z.string().optional(),
    scenePath: z.string().optional(),
    removeScene: z.boolean().optional(),
  }), async (p) => {
    const r = await bridge.request("scene.close", p);
    return textResult(r);
  });

  server.tool("unity_scene_saveAs", "Save scene as", withSafety({
    newPath: z.string(),
    sceneName: z.string().optional(),
  }), async (p) => {
    const r = await bridge.request("scene.saveAs", p);
    return textResult(r);
  });

  // ── Editor extensions ──

  server.tool("unity_scene_undo", "Undo", withSafety({
    steps: z.number().optional(),
  }), async (p) => {
    const r = await bridge.request("scene.undo", p);
    return textResult(r);
  });

  server.tool("unity_scene_redo", "Redo", withSafety({
    steps: z.number().optional(),
  }), async (p) => {
    const r = await bridge.request("scene.redo", p);
    return textResult(r);
  });

  server.tool("unity_scene_setSelection", "Set selection", withSafety({
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }), async (p) => {
    const r = await bridge.request("scene.setSelection", p);
    return textResult(r);
  });

  server.tool("unity_scene_getSelection", "Get selection", {}, async () => {
    const r = await bridge.request("scene.getSelection", {});
    return textResult(r);
  });

  server.tool("unity_scene_setParent", "Set parent", withSafety({
    childPath: z.string().optional(), childName: z.string().optional(), childInstanceId: z.number().optional(),
    parentPath: z.string().optional(), parentName: z.string().optional(), parentInstanceId: z.number().optional(),
    worldPositionStays: z.boolean().optional(),
  }), async (p) => {
    const r = await bridge.request("scene.setParent", p);
    return textResult(r);
  });

  server.tool("unity_scene_getContext", "Get scene context", {}, async () => {
    const r = await bridge.request("scene.getContext", {});
    return textResult(r);
  });
}
