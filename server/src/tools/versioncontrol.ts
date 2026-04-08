import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerVersionControlTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_vcs_getStatus",
    "Get git status of the Unity project — current branch, modified/untracked files.",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getStatus", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getChanges",
    "Get list of changed files (staged or unstaged) with their modification type.",
    {
      staged: z.boolean().optional().describe("Show staged changes only (default: false = unstaged)"),
    },
    async (p) => {
      const r = await bridge.request("vcs.getChanges", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getHistory",
    "Get git commit history. Optionally filter by file path.",
    {
      count: z.number().optional().describe("Number of commits to return (default: 20)"),
      filePath: z.string().optional().describe("Filter history to a specific file"),
    },
    async (p) => {
      const r = await bridge.request("vcs.getHistory", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getBranches",
    "List git branches. Optionally include remote branches.",
    {
      all: z.boolean().optional().describe("Include remote branches (default: false)"),
    },
    async (p) => {
      const r = await bridge.request("vcs.getBranches", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getCurrentBranch",
    "Get current branch name, HEAD hash, and last commit message.",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getCurrentBranch", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getRemotes",
    "List configured git remotes and their URLs.",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getRemotes", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getDiff",
    "Get git diff output for reviewing code changes. Can filter by file.",
    {
      filePath: z.string().optional().describe("Specific file to diff"),
      staged: z.boolean().optional().describe("Show staged diff (default: false)"),
      contextLines: z.number().optional().describe("Context lines around changes (default: 3)"),
    },
    async (p) => {
      const r = await bridge.request("vcs.getDiff", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_vcs_getStash",
    "List git stash entries.",
    {},
    async (p) => {
      const r = await bridge.request("vcs.getStash", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );
}
