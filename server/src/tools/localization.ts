import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

export function registerLocalizationTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_localization_getLocales",
    "Get locales",
    {},
    async (p) => {
      const r = await bridge.request("localization.getLocales", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_localization_addLocale",
    "Add locale",
    {
      code: z.string(),
    },
    async (p) => {
      const r = await bridge.request("localization.addLocale", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_localization_createStringTable",
    "Create StringTable",
    {
      tableName: z.string(),
      savePath: z.string().optional(),
    },
    async (p) => {
      const r = await bridge.request("localization.createStringTable", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_localization_getStringTable",
    "Get StringTable entries",
    {
      tableName: z.string(),
      locale: z.string().optional(),
    },
    async (p) => {
      const r = await bridge.request("localization.getStringTable", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_localization_setEntry",
    "Set StringTable entry",
    {
      tableName: z.string(),
      key: z.string(),
      locale: z.string(),
      value: z.string(),
    },
    async (p) => {
      const r = await bridge.request("localization.setEntry", p);
      return textResult(r);
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
      return textResult(r);
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
      return textResult(r);
    }
  );

  server.tool(
    "unity_localization_getProjectLocale",
    "Get project locale",
    {},
    async (p) => {
      const r = await bridge.request("localization.getProjectLocale", p);
      return textResult(r);
    }
  );

  server.tool(
    "unity_localization_setProjectLocale",
    "Set project locale",
    {
      code: z.string(),
    },
    async (p) => {
      const r = await bridge.request("localization.setProjectLocale", p);
      return textResult(r);
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
      return textResult(r);
    }
  );
}
