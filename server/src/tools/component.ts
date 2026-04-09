import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerComponentTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_component_list", "List components", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    includeInherited: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("component.list", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_get", "Get component properties", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string(), index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.get", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_remove", "Remove component", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string(), index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.remove", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_enable", "Enable/disable component", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string(),
    enabled: z.boolean().optional(),
    index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.enable", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_copy", "Copy component", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string(), index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.copy", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_paste", "Paste component", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    asNew: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("component.paste", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_get_all_properties", "Get all serialized properties", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string(), index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.getAll", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_move", "Move component", {
    path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
    componentType: z.string(),
    direction: z.enum(["up", "down"]),
    index: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("component.move", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_enable_batch", "Batch enable/disable components", {
    items: z.array(z.object({
      path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
      componentType: z.string(), enabled: z.boolean().optional(), index: z.number().optional(),
    })),
  }, async (p) => {
    const r = await bridge.request("component.enableBatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_component_remove_batch", "Batch remove components", {
    items: z.array(z.object({
      path: z.string().optional(), name: z.string().optional(), instanceId: z.number().optional(),
      componentType: z.string(), index: z.number().optional(),
    })),
  }, async (p) => {
    const r = await bridge.request("component.removeBatch", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
