import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerLocalizationTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_localization_getLocales",
    "Get locales",
    {},
    async (p) => {
      const r = await bridge.request("localization.getLocales", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_addLocale",
    "Add locale",
    {
      code: z.string().describe("Locale code (e.g. 'en', 'ko', 'ja')"),
    },
    async (p) => {
      const r = await bridge.request("localization.addLocale", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_createStringTable",
    "Create StringTable",
    {
      tableName: z.string(),
      savePath: z.string().optional().describe("Folder to save tables (default: 'Assets/Localization/Tables')"),
    },
    async (p) => {
      const r = await bridge.request("localization.createStringTable", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_getStringTable",
    "Get StringTable entries",
    {
      tableName: z.string(),
      locale: z.string().optional().describe("Locale code to filter (e.g. 'en'). Omit for all locales."),
    },
    async (p) => {
      const r = await bridge.request("localization.getStringTable", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_setEntry",
    "Set StringTable entry",
    {
      tableName: z.string(),
      key: z.string(),
      locale: z.string().describe("Locale code (e.g. 'en')"),
      value: z.string(),
    },
    async (p) => {
      const r = await bridge.request("localization.setEntry", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_removeEntry",
    "Remove StringTable entry",
    {
      tableName: z.string(),
      key: z.string(),
    },
    async (p) => {
      const r = await bridge.request("localization.removeEntry", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_findTables",
    "Find StringTables",
    {
      filter: z.string().optional(),
    },
    async (p) => {
      const r = await bridge.request("localization.findTables", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_getProjectLocale",
    "Get project locale",
    {},
    async (p) => {
      const r = await bridge.request("localization.getProjectLocale", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_setProjectLocale",
    "Set project locale",
    {
      code: z.string().describe("Locale code to activate (e.g. 'ko')"),
    },
    async (p) => {
      const r = await bridge.request("localization.setProjectLocale", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_exportCsv",
    "Export StringTable CSV",
    {
      tableName: z.string(),
      outputPath: z.string().optional(),
    },
    async (p) => {
      const r = await bridge.request("localization.exportCsv", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );
}
