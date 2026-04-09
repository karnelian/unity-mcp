import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerGridLayoutTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_gridLayout_create", "Create GridLayout", {
    name: z.string().optional(),
    cellSize: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    cellGap: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    cellLayout: z.enum(["Rectangle", "Hexagon", "Isometric", "IsometricZAsY"]).optional(),
    cellSwizzle: z.enum(["XYZ", "XZY", "YXZ", "YZX", "ZXY", "ZYX"]).optional(),
  }, async (p) => {
    const r = await bridge.request("gridLayout.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_gridLayout_set", "Set GridLayout", {
    ...goRef,
    cellSize: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    cellGap: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    cellLayout: z.enum(["Rectangle", "Hexagon", "Isometric", "IsometricZAsY"]).optional(),
    cellSwizzle: z.enum(["XYZ", "XZY", "YXZ", "YZX", "ZXY", "ZYX"]).optional(),
  }, async (p) => {
    const r = await bridge.request("gridLayout.set", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_gridLayout_getInfo", "Get GridLayout info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("gridLayout.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_gridLayout_find", "Find GridLayoutGroups", {}, async (p) => {
    const r = await bridge.request("gridLayout.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
