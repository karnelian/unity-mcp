import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerPrefabTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_prefab_create", "Create Prefab", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    savePath: z.string().optional().describe("저장 경로 (기본: Assets/Prefabs/{name}.prefab)"),
  }, async (p) => {
    const r = await bridge.request("prefab.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_instantiate", "Instantiate Prefab", {
    prefabPath: z.string(),
    name: z.string().optional(), position: vec3.optional(), rotation: vec3.optional(),
    parent: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.instantiate", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_instantiate_batch", "Batch instantiate Prefabs", {
    items: z.array(z.object({
      prefabPath: z.string(), name: z.string().optional(),
      position: vec3.optional(), rotation: vec3.optional(), parent: z.string().optional(),
    })),
  }, async (p) => {
    const r = await bridge.request("prefab.instantiateBatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_apply", "Apply Prefab overrides", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.apply", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_revert", "Revert Prefab overrides", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.revert", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_unpack", "Unpack Prefab", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    completely: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.unpack", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_get_overrides", "Get Prefab overrides", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.getOverrides", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_create_variant", "Create Prefab Variant", {
    basePrefabPath: z.string().describe("기본 프리팹 경로"),
    name: z.string().optional(), savePath: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.createVariant", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_find", "Find Prefabs", {
    folder: z.string().optional().describe("검색 폴더 (기본: Assets)"),
    nameFilter: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_get_info", "Get Prefab info", {
    prefabPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("prefab.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_set_active", "Set Prefab active", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    active: z.boolean(),
  }, async (p) => {
    const r = await bridge.request("prefab.setActive", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_getAssetType", "Get Prefab asset type", {
    assetPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("prefab.getAssetType", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_getInstanceStatus", "Get Prefab instance status", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("prefab.getInstanceStatus", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_prefab_replace", "Replace Prefab", {
    name: z.string().optional(), path: z.string().optional(), instanceId: z.number().optional(),
    newPrefabPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("prefab.replace", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
