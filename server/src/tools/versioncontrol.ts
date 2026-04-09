import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerVersionControlTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_vcs_getStatus",
    "Get git status",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getStatus", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getCurrentBranch",
    "Get current branch",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getCurrentBranch", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getRemotes",
    "List remotes",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getRemotes", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getStash",
    "List stashes",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getStash", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );
}
