import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAddressablesTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_addressables_getSettings", "Get Addressables settings", {}, async () => {
    const r = await bridge.request("addressables.getSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_listGroups", "List Addressable groups", {}, async () => {
    const r = await bridge.request("addressables.listGroups", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_createGroup", "Create Addressable group", {
    groupName: z.string(),
    packed: z.boolean().optional().describe("Packed Assets 스키마 사용 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("addressables.createGroup", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_removeGroup", "Remove Addressable group", {
    groupName: z.string(),
  }, async (p) => {
    const r = await bridge.request("addressables.removeGroup", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_markAddressable", "Mark asset Addressable", {
    assetPath: z.string(),
    groupName: z.string().optional().describe("대상 그룹 (기본: Default)"),
    address: z.string().optional().describe("커스텀 주소 (기본: 에셋 경로)"),
  }, async (p) => {
    const r = await bridge.request("addressables.markAddressable", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_removeAddressable", "Unmark Addressable", {
    assetPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("addressables.removeAddressable", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_setAddress", "Set Addressable address", {
    assetPath: z.string(),
    address: z.string(),
  }, async (p) => {
    const r = await bridge.request("addressables.setAddress", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_setLabel", "Set Addressable label", {
    assetPath: z.string(),
    label: z.string(),
    enabled: z.boolean().optional().describe("활성화 여부 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("addressables.setLabel", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_getEntry", "Get Addressable entry", {
    assetPath: z.string(),
  }, async (p) => {
    const r = await bridge.request("addressables.getEntry", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_findEntries", "Find Addressable entries", {
    label: z.string().optional(),
    groupName: z.string().optional(),
  }, async (p) => {
    const r = await bridge.request("addressables.findEntries", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
