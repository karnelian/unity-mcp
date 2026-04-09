import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerSceneTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_scene_hierarchy",
    "Get scene hierarchy",
    {
      path: z.string().optional(),
      depth: z.number().optional(),
      includeComponents: z.boolean().optional(),
      nameFilter: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.hierarchy", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_create",
    "Create object",
    {
      name: z.string(),
      type: z.enum(["empty", "primitive", "prefab"]).optional(),
      primitiveType: z.enum(["Cube", "Sphere", "Capsule", "Cylinder", "Plane", "Quad"]).optional(),
      prefabPath: z.string().optional(),
      parent: z.string().optional(),
      position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.create", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_setTransform",
    "Set transform",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
      rotation: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
      scale: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
      space: z.enum(["world", "local"]).optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.setTransform", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_addComponent",
    "Add component",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      componentType: z.string(),
    },
    async (params) => {
      const result = await bridge.request("scene.addComponent", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_setComponent",
    "Set component property",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      componentType: z.string(),
      properties: z.record(z.string(), z.any()),
    },
    async (params) => {
      const result = await bridge.request("scene.setComponent", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_delete",
    "Delete object",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.delete", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_duplicate",
    "Duplicate object",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      count: z.number().optional(),
      offset: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.duplicate", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_scene_manage",
    "Manage scenes",
    {
      action: z.enum(["save", "open", "create", "info"]),
      scenePath: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("scene.manage", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool("unity_scene_listLoaded", "List loaded scenes", {}, async () => {
    const r = await bridge.request("scene.listLoaded", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_setActive", "Set active scene", {
    sceneName: z.string().optional(),
    scenePath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.setActiveScene", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_moveToScene", "Move to scene", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    targetScene: z.string().optional(),
    targetScenePath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.moveToScene", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_openAdditive", "Open scene additive", {
    scenePath: z.string(),
  }, async (p) => {
    const r = await bridge.request("scene.openAdditive", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_close", "Close scene", {
    sceneName: z.string().optional(),
    scenePath: z.string().optional(),
    removeScene: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.close", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_saveAs", "Save scene as", {
    newPath: z.string(),
    sceneName: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.saveAs", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  // ── Editor extensions ──

  server.tool("unity_scene_undo", "Undo", {
    steps: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.undo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_redo", "Redo", {
    steps: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.redo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_setSelection", "Set selection", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.setSelection", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_getSelection", "Get selection", {}, async () => {
    const r = await bridge.request("scene.getSelection", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_setParent", "Set parent", {
    childPath: z.string().optional(), childName: z.string().optional(), childInstanceId: z.number().optional(),
    parentPath: z.string().optional(), parentName: z.string().optional(), parentInstanceId: z.number().optional(),
    worldPositionStays: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("scene.setParent", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scene_getContext", "Get scene context", {}, async () => {
    const r = await bridge.request("scene.getContext", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
