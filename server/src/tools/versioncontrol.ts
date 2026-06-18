import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

export function registerVersionControlTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_vcs_getStatus",
    "Get git status",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getStatus", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_vcs_getChanges",
    "Get changes",
    {
      staged: z.boolean().optional(),
    },
    async (p) => {
      const r = await bridge.request("vcs.getChanges", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_vcs_getHistory",
    "Get git history",
    {
      count: z.number().optional(),
      filePath: z.string().optional(),
    },
    async (p) => {
      const r = await bridge.request("vcs.getHistory", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_vcs_getBranches",
    "List branches",
    {
      all: z.boolean().optional(),
    },
    async (p) => {
      const r = await bridge.request("vcs.getBranches", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_vcs_getCurrentBranch",
    "Get current branch",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getCurrentBranch", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_vcs_getRemotes",
    "List remotes",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getRemotes", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_vcs_getDiff",
    "Get git diff",
    {
      filePath: z.string().optional(),
      staged: z.boolean().optional(),
      contextLines: z.number().optional(),
    },
    async (p) => {
      const r = await bridge.request("vcs.getDiff", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_vcs_getStash",
    "List stashes",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getStash", p);
      return textResult(r);
    }
  );
}
