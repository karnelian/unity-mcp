import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAddressablesTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_addressables_getSettings", "Addressables 설정을 조회합니다.", {}, async () => {
    const r = await bridge.request("addressables.getSettings", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_listGroups", "Addressable 그룹 목록을 조회합니다.", {}, async () => {
    const r = await bridge.request("addressables.listGroups", {});
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_createGroup", "Addressable 그룹을 생성합니다.", {
    groupName: z.string().describe("그룹 이름"),
    packed: z.boolean().optional().describe("Packed Assets 스키마 사용 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("addressables.createGroup", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_removeGroup", "Addressable 그룹을 삭제합니다.", {
    groupName: z.string().describe("삭제할 그룹 이름"),
  }, async (p) => {
    const r = await bridge.request("addressables.removeGroup", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_markAddressable", "에셋을 Addressable로 마킹합니다.", {
    assetPath: z.string().describe("에셋 경로"),
    groupName: z.string().optional().describe("대상 그룹 (기본: Default)"),
    address: z.string().optional().describe("커스텀 주소 (기본: 에셋 경로)"),
  }, async (p) => {
    const r = await bridge.request("addressables.markAddressable", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_removeAddressable", "에셋의 Addressable 마킹을 해제합니다.", {
    assetPath: z.string().describe("에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("addressables.removeAddressable", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_setAddress", "Addressable 에셋의 주소를 변경합니다.", {
    assetPath: z.string().describe("에셋 경로"),
    address: z.string().describe("새 주소"),
  }, async (p) => {
    const r = await bridge.request("addressables.setAddress", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_setLabel", "Addressable 에셋에 레이블을 설정합니다.", {
    assetPath: z.string().describe("에셋 경로"),
    label: z.string().describe("레이블"),
    enabled: z.boolean().optional().describe("활성화 여부 (기본: true)"),
  }, async (p) => {
    const r = await bridge.request("addressables.setLabel", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_getEntry", "에셋의 Addressable 정보를 조회합니다.", {
    assetPath: z.string().describe("에셋 경로"),
  }, async (p) => {
    const r = await bridge.request("addressables.getEntry", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_addressables_findEntries", "Addressable 엔트리를 검색합니다.", {
    label: z.string().optional().describe("레이블 필터"),
    groupName: z.string().optional().describe("그룹 필터"),
  }, async (p) => {
    const r = await bridge.request("addressables.findEntries", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
