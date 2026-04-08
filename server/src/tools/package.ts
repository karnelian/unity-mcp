import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerPackageTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_package_list",
    "설치된 패키지 목록을 조회합니다.",
    {
      includeIndirect: z.boolean().optional().describe("간접 의존성 포함 여부 (기본: false)"),
    },
    async (params) => {
      const result = await bridge.request("package.list", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_info",
    "설치된 패키지의 상세 정보를 조회합니다.",
    {
      packageName: z.string().describe("패키지 이름 (예: com.unity.textmeshpro)"),
    },
    async (params) => {
      const result = await bridge.request("package.info", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_add",
    "패키지를 설치합니다.",
    {
      identifier: z.string().describe("패키지 식별자 (이름, 이름@버전, git URL)"),
    },
    async (params) => {
      const result = await bridge.request("package.add", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_remove",
    "패키지를 제거합니다.",
    {
      packageName: z.string().describe("제거할 패키지 이름"),
    },
    async (params) => {
      const result = await bridge.request("package.remove", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_search",
    "Unity 패키지 레지스트리에서 패키지를 검색합니다.",
    {
      query: z.string().optional().describe("검색어 (이름 또는 표시 이름)"),
    },
    async (params) => {
      const result = await bridge.request("package.search", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_getVersion",
    "패키지의 현재/최신/호환 버전 정보를 조회합니다.",
    {
      packageName: z.string().describe("패키지 이름"),
    },
    async (params) => {
      const result = await bridge.request("package.getVersion", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_listBuiltIn",
    "빌트인 패키지 목록을 조회합니다.",
    {},
    async (params) => {
      const result = await bridge.request("package.listBuiltIn", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_package_resolve",
    "패키지 의존성을 다시 해석합니다.",
    {},
    async (params) => {
      const result = await bridge.request("package.resolve", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
