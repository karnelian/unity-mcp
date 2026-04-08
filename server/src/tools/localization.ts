import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerLocalizationTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_localization_getLocales",
    "Get all configured locales in the Unity Localization system. Requires com.unity.localization package.",
    {},
    async (p) => {
      const r = await bridge.request("localization.getLocales", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_addLocale",
    "Add a new locale to the project (e.g. 'en', 'ko', 'ja', 'zh', 'es').",
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
    "Create a new String Table Collection for managing localized text.",
    {
      tableName: z.string().describe("Name of the string table collection"),
      savePath: z.string().optional().describe("Folder to save tables (default: 'Assets/Localization/Tables')"),
    },
    async (p) => {
      const r = await bridge.request("localization.createStringTable", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_getStringTable",
    "Get entries from a String Table. Optionally filter by locale.",
    {
      tableName: z.string().describe("Name of the string table collection"),
      locale: z.string().optional().describe("Locale code to filter (e.g. 'en'). Omit for all locales."),
    },
    async (p) => {
      const r = await bridge.request("localization.getStringTable", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_setEntry",
    "Set a localized string entry in a String Table.",
    {
      tableName: z.string().describe("Name of the string table collection"),
      key: z.string().describe("Entry key"),
      locale: z.string().describe("Locale code (e.g. 'en')"),
      value: z.string().describe("Localized string value"),
    },
    async (p) => {
      const r = await bridge.request("localization.setEntry", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_removeEntry",
    "Remove a key and all its translations from a String Table.",
    {
      tableName: z.string().describe("Name of the string table collection"),
      key: z.string().describe("Entry key to remove"),
    },
    async (p) => {
      const r = await bridge.request("localization.removeEntry", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_findTables",
    "Search for String Table Collections in the project.",
    {
      filter: z.string().optional().describe("Name filter"),
    },
    async (p) => {
      const r = await bridge.request("localization.findTables", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_getProjectLocale",
    "Get the current active locale and list of available locales.",
    {},
    async (p) => {
      const r = await bridge.request("localization.getProjectLocale", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_localization_setProjectLocale",
    "Set the active locale for the project.",
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
    "Export a String Table to CSV file for external translation tools.",
    {
      tableName: z.string().describe("Name of the string table collection"),
      outputPath: z.string().optional().describe("Output CSV file path"),
    },
    async (p) => {
      const r = await bridge.request("localization.exportCsv", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );
}
