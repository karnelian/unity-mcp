import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { registerGuideResources } from "./guides.js";
import { formatResult } from "../utils/format.js";

function makeResourceHandler(bridge: UnityBridge, method: string) {
  return async (uri: URL) => {
    try {
      const result = await bridge.request(method);
      return {
        contents: [
          {
            uri: uri.href,
            mimeType: "application/json",
            text: formatResult(result),
          },
        ],
      };
    } catch {
      return {
        contents: [
          {
            uri: uri.href,
            mimeType: "text/plain",
            text: "Unity Editor에 연결되지 않았습니다.",
          },
        ],
      };
    }
  };
}

export function registerResources(server: McpServer, bridge: UnityBridge) {

  server.resource(
    "project-info",
    "unity://project/info",
    makeResourceHandler(bridge, "resource.projectInfo")
  );

  server.resource(
    "current-scene",
    "unity://scene/current",
    makeResourceHandler(bridge, "resource.currentScene")
  );

  server.resource(
    "recent-console",
    "unity://console/recent",
    makeResourceHandler(bridge, "resource.recentConsole")
  );

  server.resource(
    "compile-status",
    "unity://compile/status",
    makeResourceHandler(bridge, "resource.compileStatus")
  );

  server.resource(
    "installed-packages",
    "unity://packages/installed",
    makeResourceHandler(bridge, "resource.installedPackages")
  );

  server.resource(
    "project-health",
    "unity://project/health",
    makeResourceHandler(bridge, "resource.projectHealth")
  );

  // Unity workflow guides (static, no bridge needed)
  registerGuideResources(server);
}
